// src/SimServer/db/coilQueueProvider.js
import { getReadyToShipCoils } from './selectReadyForShipment.js';

export function createCoilQueueProvider({ batchSize = 10 } = {}) {
  let queue = [];

  return async function getNextCoil() {
    if (queue.length === 0) {
      queue = await getReadyToShipCoils(batchSize);
    }
    const material_id = queue.shift();
    return material_id ? { material_id } : null;
  };
}
