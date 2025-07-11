import { getCoilsThatNeedTrucks, getTrucksThatNeedRegistration } from "../../db/selectReadyForShipment.js";
import { insertDispatchData } from "../../db/insertMesData.js";
import { createFakeShippingOrderNumber, createFakeTruckName, generateMaterialForDispatch } from "../utils/fakerDispatch.js";
import { buildMESDispatchXML } from "../utils/buildMESDispatchXML.js";
import { buildTruckPayloadXML } from "../utils/buildTruckPayloadXML.js";
import { apiManager } from "../../api/apiManager.js";

export class TruckShipmentAgent {
  constructor(bay) {
    this.loadingStations = [
      {
        name: "dangerZone",
        mainArea: "SST2",
        bay: "ST21",
        area: "01",
        row: "L11"
      },
      {
        name: "poundTown",
        mainArea: "SST2",
        bay: "ST21",
        area: "01",
        row: "L12"
      }
    ];
    this.bay = bay;
    this.apiManager = new apiManager();
    this.pendingTrucks = []; // trucks inserted but not registered yet
    this.registeredTrucks = []; // trucks that have been registered and are awaiting loading
  }

  async createAndInsertTrucksForShipments() {
    // Step 0a: Read already registered trucks
    const registered = await this.apiManager.getRegisteredTrucks();
    for (const r of registered) {
      this.registeredTrucks.push({
        truckName: r.VehicleId,
        shippingOrderNumber: null,
        materials: [],
        status: "REGISTERED",
        bay: r.Bay,
        mainArea: r.MainArea,
        row: r.Row
      });
    }

    // Step 0b: Initialize from existing but not registered trucks
    const existingTrucks = await getTrucksThatNeedRegistration();
    for (const truck of existingTrucks) {
      this.pendingTrucks.push({
        truckName: truck.VEHICLEID,
        shippingOrderNumber: truck.SHIPPINGORDERNUMBER,
        materials: [],
        status: "PENDING"
      });
    }

    // Step 1: Add new trucks if needed
    const rows = await getCoilsThatNeedTrucks();

    const shipments = rows.reduce((acc, { materialId, shippingOrderNumber }) => {
      if (!acc[shippingOrderNumber]) {
        acc[shippingOrderNumber] = {
          shippingOrderNumber,
          materials: []
        };
      }
      acc[shippingOrderNumber].materials.push(materialId);
      return acc;
    }, {});

    const shipmentList = Object.entries(shipments).map(([shippingOrderNumber, shipment]) => {
      const truckName = createFakeTruckName();
      shipment.truckName = truckName;
      return shipment;
    });
    console.log("🚚 Truck shipments to be created:", shipmentList);
    for (const shipment of shipmentList) {
      const { truckName, shippingOrderNumber, materials } = shipment;
      console.log("creating material for dispatch", truckName, shippingOrderNumber, materials);
      const dispatch = generateMaterialForDispatch({
        SHIPPING_ORDER_NUMBER: shippingOrderNumber,
        SHIPMENT_PRIORITY: "1",
        TRANSPORT_MODE: "01",
        SAP_MATERIAL_CODE: "000000000001",
        MATERIALS: materials.map(id => ({ MATERIAL_ID: id, COUNT: "1" }))
      });
      const dispatchXML = buildMESDispatchXML(dispatch);
      await insertDispatchData(dispatchXML);

      await this.apiManager.insertTruck({
        VehicleID: truckName,
        ShippingOrderNumber: shippingOrderNumber,
        Description: "SimTruck",
        IsExtern: true,
        Type: "TRUCK",
        LoadingMode: "LOAD",
        ShippingDueDate: "2025-07-10T00:00:00",
        ShippingWeight: 120000,
        LoadPlanned: materials.map((materialId, i) => ({
          m_MaterialID: materialId,
          m_VehicleLocation: "",
          m_OrderKey: i + 1,
          m_TrayCoord: 0
        })),
        VehicleType: {
          m_TypeName: `EXT_TRUCK_${truckName}`,
          m_Length: 13000,
          m_Width: 2500,
          m_MaxWeight: 40000,
          m_MaxCoilCount: 6,
          m_MinCoilCount: 1,
          m_CoilSpacing: 200,
          m_VehicleBaseType: "TRUCK",
          m_Locations: []
        }
      });

      this.pendingTrucks.push({
        truckName,
        shippingOrderNumber,
        materials,
        status: "PENDING"
      });
    }

    console.log("🚛 Inserted or recovered trucks for shipments:", this.pendingTrucks);
  }

  startPollingTruckRegistration(intervalMs = 5000) {
    const isTruckBayAvailable = () => {
      for (const station of this.loadingStations) {
        const isOccupied = this.registeredTrucks.some(
          truck => truck.bay === station.bay && truck.row === station.row && truck.status === "REGISTERED"
        );
        if (!isOccupied) {
          return station;
        }
      }
      return null;
    };
    if (this.pollingInterval) clearInterval(this.pollingInterval);

    this.pollingInterval = setInterval(async () => {
        try {
          // Only try to register one truck per tick
          const station = isTruckBayAvailable();
          if (station) {
            const nextTruck = this.pendingTrucks.find(t => t.status === "PENDING");
            if (nextTruck) {
              const success = await this.apiManager.registerTruck({
                MainArea: station.mainArea,
                Bay: station.bay,
                Row: station.row,
                VehicleId: nextTruck.truckName
              });
      
              if (success) {
                nextTruck.status = "REGISTERED";
                nextTruck.bay = station.bay;
                nextTruck.row = station.row;
                nextTruck.mainArea = station.mainArea;
                this.registeredTrucks.push(nextTruck);
                this.pendingTrucks = this.pendingTrucks.filter(t => t !== nextTruck);
              }
            }
          }
      
          // Check for load completion
          for (const truck of this.registeredTrucks) {
            const result = await this.apiManager.isTruckLoaded(truck.truckName);
            const isLoaded = result?.IsFullyLoaded === true;
      
            if (isLoaded) {
              await this.apiManager.deregisterTruck(truck.truckName);
              truck.status = "COMPLETE";
              console.log(`✅ Truck ${truck.truckName} loaded and deregistered.`);
            }
          }
      
          this.registeredTrucks = this.registeredTrucks.filter(t => t.status !== "COMPLETE");
      
          if (this.pendingTrucks.length === 0 && this.registeredTrucks.length === 0) {
            clearInterval(this.pollingInterval);
            this.pollingInterval = null;
            console.log("🎉 All truck shipments complete. Polling stopped.");
          }
        } catch (err) {
          console.error("🔥 Error in truck polling:", err);
        }
      }, intervalMs);
      
  }
}
