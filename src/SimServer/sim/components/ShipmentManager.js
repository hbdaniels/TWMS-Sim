import { insertDispatchData } from '../../db/insertMesData.js';
import {
  getCoilsThatNeedShipment,
  getCoilsWithShippingOrderNumber,
  getCoilsThatNeedTrucks,
} from '../../db/selectReadyForShipment.js';
import { buildMESDispatchXML } from '../utils/buildMESDispatchXML.js';
import { buildTruckPayloadXML } from '../utils/buildTruckPayloadXML.js';
import {
  createFakeShippingOrderNumber,
  createFakeTruckName,
  generateMaterialForDispatch,
} from '../utils/fakerDispatch.js';

import { apiManager } from '../../api/apiManager.js';
import axios from 'axios';
import https from 'https';

const httpsAgent = new https.Agent({ rejectUnauthorized: false });

/**
 * Manages coil shipments and vehicle creation.
 */
export class ShipmentManager {
  static transportModes = {
    TRUCK_EXTERNAL: '01',
    RAILCAR: '02',
    BARGE: '03'
  };

  constructor() {
    this.shipments = [];
    this.shipmentIdCounter = 0;
    this.apiManager = new apiManager();
  }

  /** Adds a new shipment and returns its ID. */
  addShipment(shipment) {
    shipment.id = this.shipmentIdCounter++;
    this.shipments.push(shipment);
    return shipment.id;
  }

  /** Gets shipment by ID. */
  getShipment(id) {
    return this.shipments.find(s => s.id === id);
  }

  /** Returns all known shipments. */
  getAllShipments() {
    return this.shipments;
  }

  /** Removes a shipment by ID. */
  removeShipment(id) {
    const index = this.shipments.findIndex(s => s.id === id);
    if (index !== -1) {
      this.shipments.splice(index, 1);
      return true;
    }
    return false;
  }

  /** Gets coils that need a shipment created. */
  async getCoilsThatNeedShipment() {
    try {
      const coils = await getCoilsThatNeedShipment();
      return coils.map(coil => ({
        material_id: coil.material_id,
        status: coil.status
      }));
    } catch (err) {
      console.error('❌ Error fetching coils ready for shipment:', err);
      return [];
    }
  }

  /** Returns coils linked to a shipment ID. */
  getCoilsForShipment(shipmentId) {
    const shipment = this.getShipment(shipmentId);
    if (!shipment) {
      throw new Error(`Shipment with ID ${shipmentId} not found`);
    }
    return shipment.coils || [];
  }

  /** Creates trucks for coils that already have a shipping order but no truck. */
  async buildTrucksForCoilsWithShipments() {
    const rows = await getCoilsThatNeedTrucks();

    for (const row of rows) {
      try {
        const truckName = createFakeTruckName();
        await this._insertVehicle({
          VEHICLEID: truckName,
          SHIPPINGORDERNUMBER: row.shippingOrderNumber,
          MATERIAL_ID: row.materialId
        });

        this.shipments.push({
          SHIPPINGORDERNUMBER: row.shippingOrderNumber,
          VEHICLEID: truckName,
          COILS: [row]
        });
      } catch (error) {
        console.error(`❌ Error building truck for coil ${row.materialId}:`, error);
      }
    }

    console.log('✅ Trucks built for coils with shipments:', this.shipments);
  }

  /** Builds fake shipping orders + dispatch XML + vehicle. */
  async buildOutFakeTruckShipment() {
    const coils = await getCoilsThatNeedShipment();
    console.log('🚚 Coils that need shipments:', coils);

    for (const coil of coils) {
      const shippingOrderNumber = createFakeShippingOrderNumber();
      const truckName = createFakeTruckName();

      const dispatch = generateMaterialForDispatch({
        SHIPPING_ORDER_NUMBER: shippingOrderNumber,
        SHIPMENT_PRIORITY: '1',
        TRANSPORT_MODE: ShipmentManager.transportModes.TRUCK_EXTERNAL,
        SAP_MATERIAL_CODE: '000000000001',
        MATERIALS: [{
          MATERIAL_ID: coil,
          COUNT: '1'
        }]
      });

      const dispatchXML = buildMESDispatchXML(dispatch);
      console.log('📤 Dispatch XML:', dispatchXML);
      await insertDispatchData(dispatchXML);

      const loadPlannedXML = buildTruckPayloadXML(coil);
      await this._insertVehicle({
        VEHICLEID: truckName,
        SHIPPINGORDERNUMBER: shippingOrderNumber
      });

      this.shipments.push({
        SHIPPINGORDERNUMBER: shippingOrderNumber,
        VEHICLEID: truckName,
        LOAD_PLANNED: loadPlannedXML,
        COILS: [coil]
      });
    }
  }

  /** Internal: inserts vehicle into system. */
  async _insertVehicle(vehicle) {
    try {
      const response = await axios.post(
        'https://localhost:44378/api/trucks/insert-vehicle',
        {
          VehicleID: vehicle.VEHICLEID,
          ShippingOrderNumber: vehicle.SHIPPINGORDERNUMBER,
          Description: vehicle.DESCRIPTION || 'SimTruck',
          IsExtern: true,
          Type: vehicle.TYPE || 'TRUCK',
          LoadingMode: 'LOAD',
          ShippingDueDate: '2025-07-10T00:00:00',
          ShippingWeight: 120000,
          LoadPlanned: [
            {
              m_MaterialID: vehicle.MATERIAL_ID || '2104465400',
              m_VehicleLocation: '',
              m_OrderKey: 1,
              m_TrayCoord: 0
            }
          ],
          VehicleType: {
            m_TypeName: `EXT_TRUCK_${vehicle.VEHICLEID}`,
            m_Length: 13000,
            m_Width: 2500,
            m_MaxWeight: 40000,
            m_MaxCoilCount: 6,
            m_MinCoilCount: 1,
            m_CoilSpacing: 200,
            m_VehicleBaseType: vehicle.m_VehicleBaseType || 'TRUCK',
            m_Locations: []
          }
        },
        { httpsAgent }
      );

      console.log(`✅ Vehicle inserted: ${vehicle.VEHICLEID}`, response.data);
    } catch (err) {
      if (err.response) {
        console.error('❌ Vehicle insert failed:', err.response.status, err.response.data);
      } else {
        console.error('❌ Insert error:', err.message);
      }
    }
  }
}
