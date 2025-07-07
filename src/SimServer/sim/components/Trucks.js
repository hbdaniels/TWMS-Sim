import { generateMaterialForDispatch } from '../utils/fakerDispatch.js';
import { buildMESDispatchXML } from '../utils/buildMESDispatchXML.js';
import { insertDispatchData } from '../../db/insertMesData.js';
import { getCoilsThatNeedShipment } from '../../db/selectReadyForShipment.js';
import { ShipmentManager } from './ShipmentManager.js';

export class Trucks {
  constructor({ rate = 1 }) {
    this.rate = rate;
    this.counter = 0;
    this.queue = [];
    this.ready = false;
    this.shipmentManager = new ShipmentManager();
  }

  async tick() {
    this.counter++;

    if (this.counter >= this.rate) {
      this.counter = 0;
      console.log('🚚 Building trucks for coils with shipments...');
      this.shipmentManager.buildTrucksForCoilsWithShipments();
      console.log('📥 Building out fake shipments...');
      this.shipmentManager.buildOutFakeTruckShipment();
      
    //   if (!this.ready || this.queue.length === 0) {
    //     console.log('📥 Querying TWMS for coils ready to ship...');
    //     this.queue = await getReadyToShipCoils(10);
    //     this.ready = true;
    //   }

    //   const material_id = this.queue.shift();
    //   if (!material_id) return;

    //   const dispatch = generateTruckMaterialForDispatch({
    //     TRANSPORT_MODE: '01', // TRUCK_EXTERNAL
    //     MATERIALS: [{ material_id }]
    //   });

    //   const xml = buildMESDispatchXML(dispatch);
    //   console.log(xml); 

    //   await insertDispatchData({
    //     xml,
    //     material_id
    //   });

    //   console.log(`🚚 Sent MaterialForDispatch for coil ${material_id}`);
    }
  }
}
