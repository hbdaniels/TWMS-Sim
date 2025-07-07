// ShipmentManager.js
import { insertDispatchData} from '../../db/insertMesData.js';
import { getCoilsThatNeedShipment, getCoilsWithShippingOrderNumber, getCoilsThatNeedTrucks } from '../../db/selectReadyForShipment.js';
import { buildMESDispatchXML } from '../utils/buildMESDispatchXML.js';
import { buildTruckPayloadXML } from '../utils/buildTruckPayloadXML.js';
import { createFakeShippingOrderNumber, createFakeTruckName, generateMaterialForDispatch } from '../utils/fakerDispatch.js';

import axios from 'axios';
import https from 'https';

/**
 * Manages shipments and their associated coils.
 * Flow: get coils ready for shipment -> send material for dispatch -> create vehicle with loadplan for coil -> collect vehicle que for registration
 */
const httpsAgent = new https.Agent({ rejectUnauthorized: false });

async function insertVehicle(vehicle) {
    try {
      const response = await axios.post(
        'https://localhost:44378/api/vehicle/insert-vehicle',
        {
          VehicleID: vehicle.VEHICLEID,
          ShippingOrderNumber: vehicle.SHIPPINGORDERNUMBER,
          Description: 'TWMS SimTruck',
          IsExtern: true,
          Type: 'TRUCK',
          LoadingMode: 'LOAD',
          ShippingDueDate: '2025-07-10T00:00:00',
          ShippingWeight: 40000,
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
            m_MaxCoilCount: 4,
            m_MinCoilCount: 1,
            m_CoilSpacing: 200,
            m_VehicleBaseType: 'TRUCK',
            m_Locations: []
          }
        },
        { httpsAgent }
      );
  
      console.log('✅ Insert Success:', response.data);
    } catch (err) {
      if (err.response) {
        console.error('❌ Insert Failed:', err.response.status, err.response.data);
      } else {
        console.error('❌ Error:', err.message);
      }
    }
  }
  


export class ShipmentManager {
    static transportModes = {
        TRUCK_EXTERNAL: '01',
        RAILCAR: '02',
        BARGE: '03'
    };

    constructor() {
        this.shipments = [];
        this.shipmentIdCounter = 0;
    }

    addShipment(shipment) {
        shipment.id = this.shipmentIdCounter++;
        this.shipments.push(shipment);
        return shipment.id;
    }

    getShipment(id) {
        return this.shipments.find(s => s.id === id);
    }

    getAllShipments() {
        return this.shipments;
    }

    removeShipment(id) {
        const index = this.shipments.findIndex(s => s.id === id);
        if (index !== -1) {
            this.shipments.splice(index, 1);
            return true;
        }
        return false;
    }

    getCoilsThatNeedShipment(){
        getCoilsThatNeedShipment()
        .then(coils => {
            return coils.map(coil => ({
                material_id: coil.material_id,
                status: coil.status
            }));
        })
        .catch(err => {
            console.error('Error fetching coils ready for shipment:', err);
            return [];
        });
    }

    //TODO: Implement this method to return coils ready for shipment
    getCoilsForShipment(shipmentId) {
        const shipment = this.getShipment(shipmentId);
        if (!shipment) {
            throw new Error(`Shipment with ID ${shipmentId} not found`);
        }
        return shipment.coils || [];
    }

    //These Coils have shipments but need trucks
    async buildTrucksForCoilsWithShipments() {
        let rows = await getCoilsThatNeedTrucks();
        //console.log(rows);
        rows.forEach(row => {
            try {
                let truckName = createFakeTruckName();
                //let truckLoadPlannedXML = buildTruckPayloadXML([row.MATERIAL_ID]);
                //insertVehicleForShipment({VEHICLEID: truckName, SHIPPINGORDERNUMBER: shippingOrderNumber, LOAD_PLANNED: truckLoadPlannedXML});
                insertVehicle({VEHICLEID: truckName, MATERIAL_ID: row.materialId, SHIPPINGORDERNUMBER: row.shippingOrderNumber})
                this.shipments.push({SHIPPINGORDERNUMBER: row.SHIPPINGORDERNUMBER, VEHICLEID: truckName, COILS: [row]})
            }
            catch (error) {
                console.error(`Error building truck for coil ${row.materialId}:`, error);
            }

        });
        console.log("Trucks built for coils with shipments:", this.shipments);
        return;
    }

    async buildOutFakeTruckShipment(shipmentDetails) {
        
        let coils = [];
        coils = await getCoilsThatNeedShipment();
        console.log("Coils that need shipments: ", coils);
        coils.forEach(coil => {
            let shippingOrderNumber = createFakeShippingOrderNumber();
            //console.log(coil);
    //   const dispatch = generateTruckMaterialForDispatch({
    //     TRANSPORT_MODE: '01', // TRUCK_EXTERNAL
    //     MATERIALS: [{ material_id }]
    //   });
            let dispatch = generateMaterialForDispatch({
                SHIPPING_ORDER_NUMBER: shippingOrderNumber,
                SHIPMENT_PRIORITY: '1',
                TRANSPORT_MODE: ShipmentManager.transportModes.TRUCK_EXTERNAL,
                SAP_MATERIAL_CODE: '000000000001',
                MATERIALS: [{
                    MATERIAL_ID: coil,
                    COUNT: '1',
                }]
            })

            let dispatchXML = buildMESDispatchXML(dispatch);
            console.log('Dispatch XML:', dispatchXML);
            insertDispatchData(dispatchXML);
            console.log('Dispatch ready:', dispatch);   
            let truckName = createFakeTruckName();
            let truckLoadPlannedXML = buildTruckPayloadXML(coil);
            //insertVehicleForShipment({VEHICLEID: truckName, SHIPPINGORDERNUMBER: shippingOrderNumber, LOAD_PLANNED: truckLoadPlannedXML});
            insertVehicle({VEHICLEID: truckName, SHIPPINGORDERNUMBER: shippingOrderNumber})
            this.shipments.push({SHIPPINGORDERNUMBER: shippingOrderNumber, VEHICLEID: truckName, LOAD_PLANNED: truckLoadPlannedXML, COILS: [coil]});
    
        })
    
    
    
        return null
    }

}