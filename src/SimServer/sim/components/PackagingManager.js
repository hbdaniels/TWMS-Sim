import axios from 'axios';
import https from 'https';

export class PackagingManager {
    constructor({
      holdTickRate = 5,         // every 5 ticks
      packingStageTickRate = 8 // every 8 ticks
    } = {}) {
      this.coilsInPackingProcess = [];
      this.holdTickRate = holdTickRate;
      this.packingStageTickRate = packingStageTickRate;
  
      this._tickCounter = 0;
  
      this.axios = axios.create({
        baseURL: 'https://localhost:5001/api/vehicle',
        httpsAgent: new https.Agent({ rejectUnauthorized: false })
      });
    }
  
    tick() {
      this._tickCounter++;
  
      if (this._tickCounter % this.holdTickRate === 0) {
        //this.removeHold();
        console.log(`🔄 Hold release tick: ${this._tickCounter}`);
      }
  
      if (this._tickCounter % this.packingStageTickRate === 0) {
        //this.setPackingStage();
        console.log(`🔄 Packing stage tick: ${this._tickCounter}`);
      }
    }
  
    addCoil(materialId) {
      this.coilsInPackingProcess.push({ materialId, holdReleased: false, packed: false });
    }
  
    async removeHold() {
      for (const coil of this.coilsInPackingProcess) {
        if (!coil.holdReleased) {
          try {
            await this.axios.post('/coil/set-hold-status', {
              MaterialId: coil.materialId,
              OnHold: false
            });
            coil.holdReleased = true;
            console.log(`✅ Unheld: ${coil.materialId}`);
          } catch (err) {
            console.error(`❌ Failed to unhold ${coil.materialId}:`, err.message);
          }
          break;
        }
      }
    }
  
    async setPackingStage() {
      for (const coil of this.coilsInPackingProcess) {
        if (coil.holdReleased && !coil.packed) {
          try {
            await this.axios.post('/coil/setPackingStage', {
              MaterialId: coil.materialId,
              PackingStage: 'WrappingDone'
            });
            coil.packed = true;
            console.log(`🎁 Packed: ${coil.materialId}`);
          } catch (err) {
            console.error(`❌ Failed to pack ${coil.materialId}:`, err.message);
          }
          break;
        }
      }
    }
  
    getCoilsInPackingProcess() {
      return this.coilsInPackingProcess;
    }
  }
  