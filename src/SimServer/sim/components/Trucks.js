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
      
    }
  }
}
