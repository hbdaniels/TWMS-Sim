export class Rail {
    constructor({ rate = 20 }) {
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
        console.log('🚆 Building railcars for coils with shipments...');
        await this.shipmentManager.buildRailcarsForCoilsWithShipments();
      }
    }
  }
  