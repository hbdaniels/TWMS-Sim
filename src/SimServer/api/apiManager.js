import axios from 'axios';
import https from 'https';

export class apiManager {
    constructor() {
        this.axios = axios.create({
            baseURL: 'https://localhost:44378/api',
            httpsAgent: new https.Agent({ rejectUnauthorized: false })
          });

    }
    getApiManager() {
        return this;
    }
    
    async init() {
        // Initialization logic for the API manager
        console.log('API Manager initialized');
    }
    async getCoilsAtPackagingOrCM() {
        // Placeholder for actual API call to get coils at packaging or coil master
        return [];
    }
    async getCoilsOnHold() {
        // Placeholder for actual API call to get coils on hold
        return [];
    }
    async removeHold() {
        // Placeholder for actual API call to remove hold from coils
        console.log('Removing hold from coils');
    }
    async registerRailcar(bay, vehicleId, position) {
        try {
            await this.axios.post('/railcar/register-wagon', {
              VehicleId: vehicleId,
              MainArea: 'SST2',
              Bay: bay,
              Row: 'R1',
              WagonTypeId: 3,
              Position: position,
              UseDefaultType: false
            });
            console.log(`✅ Successfully registered railcar ${vehicleId} at bay ${bay}, position ${position}`);
          } catch (err) {
            console.error(`❌ Error registering railcar ${vehicleId} at bay ${bay}, position ${position}:`, err);
          }
          
    }


    async planLoadForRailcars(vehicleId, position, shippingOrderNumber, coils){
        try {
            await this.axios.post('/railcar/plan-wagon-load', {
                VehicleId: vehicleId,
                Position: position,
                ShippingOrderNumber: shippingOrderNumber,
                MaterialIds: coils
            });
            console.log(`✅ Successfully planned load for railcar ${vehicleId} at position ${position} with shipping order ${shippingOrderNumber}`);
        }catch (err) {
            console.error(`❌ Error planning load for railcar ${vehicleId} at position ${position}:`, err);
        }
    }

    async checkWagonLoadComplete(vehicleId) {
        try {
          const res = await this.axios.post('/railcar/check-wagon-load-complete', {
            VehicleId: vehicleId
          });
      
          const isComplete = res?.data?.IsFullyLoaded ?? false;
          console.log(`ℹ️ Wagon ${vehicleId} is ${isComplete ? "✅ fully loaded" : "❌ not fully loaded yet"}`);
          return isComplete;
        } catch (err) {
          console.error(`❌ Error checking load status for wagon ${vehicleId}:`, err);
          return false;
        }
      }
      
      async confirmShipmentIfComplete(vehicleId) {
        try {
          const res = await this.axios.post('/railcar/confirm-if-complete', {
            VehicleId: vehicleId
          });
      
          const confirmed = res?.data?.confirmed ?? false;
          const message = res?.data?.message ?? "No message";
      
          if (confirmed) {
            console.log(`✅ ${message}`);
          } else {
            console.warn(`⚠️ ${message}`);
          }
      
          return confirmed;
        } catch (err) {
          console.error(`❌ Error confirming shipment for wagon ${vehicleId}:`, err);
          return false;
        }
      }
      
      
    //   async confirmShipmentIfComplete(vehicleId) {
    //     try {
    //       const res = await this.axios.post('/railcar/confirm-if-complete', {
    //         VehicleId: vehicleId
    //       });
      
    //       const isConfirmed = res?.data?.isComplete ?? false;
    //       console.log(`🚚 Confirm result for ${vehicleId}: ${isConfirmed ? "✅ confirmed" : "❌ not confirmed"}`);
    //       return isConfirmed;
    //     } catch (err) {
    //       console.error(`❌ Error confirming shipment for wagon ${vehicleId}:`, err);
    //       return false;
    //     }
    //   }
      
      

    async deregisterRailcar(vehicleId) {    
        try {
            await this.axios.post('/railcar/deregister-wagon', {
                VehicleId: vehicleId
            });
            console.log(`✅ Successfully deregistered wagon ${vehicleId}`);
        }catch (err) {
            console.error(`❌ Error deregistering wagon ${vehicleId}:`, err);
        }
    }

    async checkRailcarLoadingComplete(coilId, railcarId) {  
        // Placeholder for actual API call to check if railcar loading is complete
        console.log(`Checking if railcar ${railcarId} loading is complete for coil ${coilId}`);
        return true; // Simulating a successful check
    }

    async insertTruck(payload) {
        return axios.post(
          "https://localhost:44378/api/trucks/insert-vehicle",
          payload,
          { httpsAgent: new https.Agent({ rejectUnauthorized: false }) }
        );
      }
      async registerTruck(payload) {
        try {
          const res = await this.axios.post('/trucks/register-truck', {
            VehicleId: payload.VehicleId,
            MainArea: payload.MainArea,
            Bay: payload.Bay,
            Row: payload.Row
          });
      
          if (res.status === 200) {
            console.log(`✅ Registered truck ${payload.VehicleId} at ${payload.Bay}/${payload.Row}`);
            return true;
          }
      
          console.warn(`⚠️ Unexpected status during truck registration: ${res.status}`);
          return false;
        } catch (err) {
          console.error(`❌ Failed to register truck ${payload.VehicleId}:`, err.response?.data || err.message);
          return false;
        }
      }

      async deregisterTruck(vehicleId) {
        try {
          const res = await this.axios.post('/trucks/deregister-truck', {
            VehicleId: vehicleId
          });
      
          if (res.status === 200) {
            console.log(`✅ Truck ${vehicleId} deregistered.`);
            return true;
          } else {
            console.warn(`⚠️ Unexpected status during truck deregistration: ${res.status}`);
            return false;
          }
        } catch (err) {
          console.error(`❌ Failed to deregister truck ${vehicleId}:`, err.response?.data || err.message);
          return false;
        }
      }
      
      

    async getRegisteredTrucks() {
      try {
        const res = await this.axios.get('/trucks/registered-trucks');
        return res.data;
      } catch (err) {
        console.error('❌ Failed to fetch registered trucks:', err);
        return [];
      }
    }
    
    async isTruckLoaded(vehicleId) {
      try {
        const res = await this.axios.post('/trucks/is-loaded', { VehicleId: vehicleId });
        return res.data; // { VehicleId: "...", IsFullyLoaded: true/false }
      } catch (err) {
        console.error(`❌ Failed to check if truck ${vehicleId} is loaded:`, err);
        return { IsFullyLoaded: false };
      }
    }
      
      
      

}