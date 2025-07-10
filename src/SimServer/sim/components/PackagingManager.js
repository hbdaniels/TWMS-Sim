// src/SimServer/sim/components/PackagingManager.js
import { getCoilsAtPackagingOrCM, getCoilsOnHold } from '../../db/dbWob.js';
import axios from 'axios';
import https from 'https';

export class PackagingManager {
  constructor({
    holdTickRate = 5,
    packagingTickRate = 8,
    coilMasterTickRate = 6,
    refreshTickRate = 30
  } = {}) {
    this._tickCounter = 0;

    this.holdTickRate = holdTickRate;
    this.packagingTickRate = packagingTickRate;
    this.coilMasterTickRate = coilMasterTickRate;
    this.refreshTickRate = refreshTickRate;

    this.coilsOnHold = [];              // [{ MATERIAL_ID, unheld }]
    this.coilsAtPackaging = [];         // [{ MATERIAL_ID, holdReleased, stageUpdated, PACKINGSTAGE, etc }]
    this.coilsAtCoilMaster = [];        // [{ same }]

    this._unheldCount = 0;
    this._packagingUpdated = 0;
    this._coilMasterUpdated = 0;

    this.axios = axios.create({
      baseURL: 'https://localhost:44378/api',
      httpsAgent: new https.Agent({ rejectUnauthorized: false })
    });
  }

  async init() {
    await this.refreshCoilLists();
    console.log(`📦 PackagingManager initialized with ${this.coilsAtPackaging.length} packaging coils, ${this.coilsAtCoilMaster.length} coil master coils, ${this.coilsOnHold.length} on hold`);
  }

  tick() {
    this._tickCounter++;

    if (this._tickCounter % this.refreshTickRate === 0) {
      this.refreshCoilLists();
    }

    if (this._tickCounter % this.holdTickRate === 0) {
      this.removeHold();
    }

    if (this._tickCounter % this.packagingTickRate === 0) {
      this.updatePackagingLine();
    }

    if (this._tickCounter % this.coilMasterTickRate === 0) {
      this.updateCoilMaster();
    }
  }

  async refreshCoilLists() {
    const rawPackaging = await getCoilsAtPackagingOrCM();
    const rawOnHold = await getCoilsOnHold();

    // Split by ROWNAME
    const packagingCoils = rawPackaging.filter(c => c.ROWNAME === 'PAC');
    const coilMasterCoils = rawPackaging.filter(c => c.ROWNAME === 'BR');

    const knownPackaging = new Set(this.coilsAtPackaging.map(c => c.MATERIAL_ID));
    const knownCoilMaster = new Set(this.coilsAtCoilMaster.map(c => c.MATERIAL_ID));
    const prevUnheld = new Map(this.coilsOnHold.map(c => [c.MATERIAL_ID, c.unheld]));

    const newPackaging = packagingCoils
      .filter(c => !knownPackaging.has(c.MATERIAL_ID))
      .map(c => ({
        ...c,
        holdReleased: c.ON_HOLD === 'N',
        stageUpdated: false
      }));

    const newCoilMaster = coilMasterCoils
      .filter(c => !knownCoilMaster.has(c.MATERIAL_ID))
      .map(c => ({
        ...c,
        holdReleased: c.ON_HOLD === 'N',
        stageUpdated: false
      }));

    this.coilsAtPackaging.push(...newPackaging);
    this.coilsAtCoilMaster.push(...newCoilMaster);

    this.coilsOnHold = rawOnHold.map(c => ({
      ...c,
      unheld: prevUnheld.get(c.MATERIAL_ID) || false
    }));

    console.log(`🔄 Refreshed: ${this.coilsAtPackaging.length} at packaging, ${this.coilsAtCoilMaster.length} at CM, ${this.coilsOnHold.length} on hold`);
  }

  async removeHold() {
    for (const coil of this.coilsOnHold) {
      if (!coil.unheld) {
        try {
          await this.axios.post('/coil/set-hold-status', {
            MaterialId: coil.MATERIAL_ID,
            OnHold: false
          });
          coil.unheld = true;
          this._unheldCount++;
          console.log(`✅ Unheld: ${coil.MATERIAL_ID}`);
        } catch (err) {
          console.error(`❌ Failed to unhold ${coil.MATERIAL_ID}:`, err.message);
        }
        break;
      }
    }
  }

  async updatePackagingLine() {
    for (const coil of this.coilsAtPackaging) {
      if (!coil.stageUpdated) {
        let flagUpdatePlantCodes = false;
        const loc = `${coil.BAY}:${coil.AREA}:${coil.ROWNAME}:${coil.LOCATION}`;
        console.log(`📍 Packaging line decision for ${coil.MATERIAL_ID} at ${loc}`);

        let stage = 'PrepackingDone';
        if (coil.PACKINGSTAGE === 'C') {
          stage = 'BargePackagingDone';
          flagUpdatePlantCodes = true;
        } else if (coil.SUCCESIVE_PLANT_CODE === 'ANBA') {
          stage = 'BandingDone';
        }

        try {
          await this.axios.post('/coil/set-packing-stage', {
            MaterialId: coil.MATERIAL_ID,
            PackingStage: stage
          });
            if (flagUpdatePlantCodes) {
                flagUpdatePlantCodes = false;
                await this.axios.post('/coil/set-packed-plant-codes', {
                  MaterialId: coil.MATERIAL_ID,
                });
            }
          coil.PACKINGSTAGE = stage;
          coil.stageUpdated = true;
          this._packagingUpdated++;
          console.log(`🎁 Packaging updated: ${coil.MATERIAL_ID} → ${stage}`);
        } catch (err) {
          console.error(`❌ Packaging update failed: ${coil.MATERIAL_ID}`, err.message);
        }
        break;
      }
    }
  }

  async updateCoilMaster() {
    for (const coil of this.coilsAtCoilMaster) {
      if (!coil.stageUpdated) {
        const loc = `${coil.BAY}:${coil.AREA}:${coil.ROWNAME}:${coil.LOCATION}`;
        console.log(`📍 Coil Master update for ${coil.MATERIAL_ID} at ${loc}`);
        try {
          await this.axios.post('/coil/set-packing-stage', {
            MaterialId: coil.MATERIAL_ID,
            PackingStage: 'CoilMasterDone'
          });
            // If transport mode is external truck or railcar, update plant codes
          if (coil.TRANSPORT_MODE_ORDER === 'TRUCK_EXTERNAL' || coil.TRANSPORT_MODE_ORDER === 'RAILCAR') {
            await this.axios.post('/coil/set-packed-plant-codes', {
              MaterialId: coil.MATERIAL_ID,
            });
          }
          
          coil.PACKINGSTAGE = 'CoilMasterDone';
          coil.stageUpdated = true;
          this._coilMasterUpdated++;
          console.log(`📦 CM updated: ${coil.MATERIAL_ID}`);
        } catch (err) {
          console.error(`❌ CM update failed: ${coil.MATERIAL_ID}`, err.message);
        }
        break;
      }
    }
  }

  getSummary() {
    return {
      onHold: this.coilsOnHold.length,
      unheld: this._unheldCount,
      atPackaging: this.coilsAtPackaging.length,
      packagingUpdated: this._packagingUpdated,
      atCoilMaster: this.coilsAtCoilMaster.length,
      coilMasterUpdated: this._coilMasterUpdated
    };
  }
}
