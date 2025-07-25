import { getCoilsThatNeedRailcars } from "../../db/selectReadyForShipment.js";
import { createFakeRailcarName } from "../utils/fakerDispatch.js";
import { apiManager } from "../../api/apiManager.js";
import { getCoilsReadyForRailShipment } from "../../db/dbRail.js";
import { createFakeShippingOrderNumber } from "../utils/fakerDispatch.js";
import { generateMaterialForDispatch } from "../utils/fakerDispatch.js";
import { buildMESDispatchXML } from "../utils/buildMESDispatchXML.js";
import { insertDispatchData } from "../../db/insertMesData.js";
import { chunkArray } from "../utils/chunkArrray.js";

export class RailShipmentAgent {
    constructor(bay){
        this.bay = bay;
        this.apiManager = new apiManager();
    }

    async createAndRegisterRailcarsForShipments() {

        
        const rows = await getCoilsThatNeedRailcars();
      
        // Step 1: Group by shipping order
        let railShipments = rows.reduce((acc, { materialId, shippingOrderNumber }) => {
          if (!acc[shippingOrderNumber]) {
            acc[shippingOrderNumber] = {
              shippingOrderNumber,
              materials: []
            };
          }
          acc[shippingOrderNumber].materials.push(materialId);
          return acc;
        }, {});
      
        // Step 2: Convert to array and limit to first 8
        const selectedShipments = Object.entries(railShipments)
          .slice(0, 8)
          .map(([shippingOrderNumber, shipment], i) => {
            const railcarName = createFakeRailcarName(); // e.g., "WAGON_001"
            shipment.railcarName = railcarName;
            shipment.index = i;
            return shipment;
          });
      
        // Step 3: Register each railcar
        for (const shipment of selectedShipments) {
          await this.apiManager.registerRailcar(this.bay, shipment.railcarName, shipment.index);
        }
      
        // Optional: Store for follow-up phases (loadplan, monitor, confirm)
        this.activeRailcarShipments = selectedShipments;
      
        console.log("🚂 Registered Railcar Shipments:", selectedShipments);
      }

      async planLoadsForActiveRailcars() {
        if (!this.activeRailcarShipments || this.activeRailcarShipments.length === 0) {
          console.warn("⚠️ No active railcar shipments to plan.");
          return;
        }
      
        for (const { railcarName, index: position, shippingOrderNumber, materials } of this.activeRailcarShipments) {
          await this.apiManager.planLoadForRailcars(
            railcarName,
            position,
            shippingOrderNumber,
            materials
          );
        }
      
        console.log("✅ All active railcars have been planned.");
      }

      async checkRailShipmentComplete(vehicleId) {
        try {
          const res = await this.apiManager.checkWagonLoadComplete(vehicleId);
          console.log(`🔍 Load complete status for ${vehicleId}: ${res}`);
          return res;
        } catch (err) {
          console.error(`❌ Error checking load status for ${vehicleId}:`, err);
          return false;
        }
      }
      
      async confirmRailShipmentIfComplete(vehicleId) {
        try {
          const res = await this.apiManager.confirmShipmentIfComplete(vehicleId); // 🔧 FIXED HERE
          console.log(`📦 Confirmation result for ${vehicleId}: ${res}`);
          return res;
        } catch (err) {
          console.error(`❌ Error confirming shipment for ${vehicleId}:`, err);
          return false;
        }
      }

      startPollingRailShipmentStatus(intervalMs = 15000) {
        console.log("starting poller");
        if (!this.activeRailcarShipments || this.activeRailcarShipments.length === 0) {
          console.warn("⚠️ No active railcar shipments to monitor.");
          return;
        }
      
        if (this.pollingInterval) {
          clearInterval(this.pollingInterval); // avoid duplicates
        }
      
        this.pollingInterval = setInterval(() => {
          // wrap async in IIFE to ensure exceptions bubble out
          (async () => {
            console.log("🔄 Polling railcar shipment status...");
      
            for (const shipment of this.activeRailcarShipments) {
              if (shipment.isComplete) continue;
      
              const isLoaded = await this.checkRailShipmentComplete(shipment.railcarName);
      
              if (isLoaded) {
                const isConfirmed = await this.confirmRailShipmentIfComplete(shipment.railcarName);
                if (isConfirmed) {
                  await this.apiManager.deregisterRailcar(shipment.railcarName);
                  shipment.isComplete = true;
                  console.log(`✅ Railcar ${shipment.railcarName} fully loaded, confirmed, and deregistered.`);
                } else {
                  console.log(`⏳ Railcar ${shipment.railcarName} is loaded but not yet confirmed.`);
                }
              }
            }
      
            const allDone = this.activeRailcarShipments.every(s => s.isComplete);
            if (allDone) {
              clearInterval(this.pollingInterval);
              this.pollingInterval = null;
              console.log("🎉 All railcar shipments complete. Polling stopped.");
            }
          })().catch(err => {
            console.error("🔥 Uncaught error during polling:", err);
          });
        }, intervalMs);
      }

      async dispatchCoilsThatAreRailShipments() {
        const existingCoilsReadyToDispatch = await getCoilsThatNeedRailcars(this.bay);
        if (existingCoilsReadyToDispatch.length >= 40) {
          console.log("There are enough coils ready for rail shipment, no need to create new ones.");
          return;
        }
      
        const coils = await getCoilsReadyForRailShipment(this.bay);
      
        if (coils.length === 0) {
          console.log("No coils ready for rail shipment.");
          return;
        }
      
        const maxShipments = 8;
        const maxCoilsPerShipment = 8;
      
        const limitedCoils = coils.slice(0, maxShipments * maxCoilsPerShipment);
        const batches = chunkArray(limitedCoils, maxCoilsPerShipment);
        const limitedBatches = batches.slice(0, maxShipments);
      
        this.activeRailcarShipments = this.activeRailcarShipments || [];
      
        for (const [i, batch] of limitedBatches.entries()) {
          const shippingOrderNumber = createFakeShippingOrderNumber(); // e.g., `RAIL-${Date.now()}`
          const railcarName = createFakeRailcarName();
          const materialIds = batch.map(c => c.MATERIAL_ID);
      
          console.log("Creating rail shipment:", shippingOrderNumber, materialIds);
      
          const dispatch = generateMaterialForDispatch({
            SHIPPING_ORDER_NUMBER: shippingOrderNumber,
            SHIPMENT_PRIORITY: "1",
            TRANSPORT_MODE: "02", // 01 = Truck, 02 = Rail
            SAP_MATERIAL_CODE: "000000000001",
            MATERIALS: materialIds.map(id => ({ MATERIAL_ID: id, COUNT: "1" }))
          });
      
          const dispatchXML = buildMESDispatchXML(dispatch);
          await insertDispatchData(dispatchXML);
      
          this.activeRailcarShipments.push({
            shippingOrderNumber,
            railcarName,
            index: this.activeRailcarShipments.length,
            materials: materialIds,
            isComplete: false
          });
        }
      }
      
      
      createRailShipmentBatch(batch) {
        // maybe register a wagon and load these coils
        //console.log("Batch: ", batch);
        const coilIds = batch.map(c => c.MATERIAL_ID);
        console.log(`🚛 Creating rail shipment for ${coilIds.join(', ')}`);
        // call your API here or trigger simulation behavior
      }

}


