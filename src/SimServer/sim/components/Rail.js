// sim/components/Rail.js
export class Rail {
    constructor({ rate = 1 }) {
      this.rate = rate;
      this.counter = 0;
    }
  
    tick(state) {
      this.counter++;
      if (this.counter >= this.rate) {
        this.counter = 0;
        console.log(`🚆 Simulating rail car pickup event`);
      }
    }
  }