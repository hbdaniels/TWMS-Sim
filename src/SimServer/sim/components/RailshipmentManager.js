import { getCoilsThatNeedRailcars } from "../../db/selectReadyForShipment.js";
import { createFakeRailcarName } from "../utils/fakerDispatch.js";
import { apiManager } from "../../api/apiManager.js";

export class RailShipmentManager {
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
          .slice(0, 1)
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

      startPollingRailShipmentStatus(intervalMs = 5000) {
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
      
      


}