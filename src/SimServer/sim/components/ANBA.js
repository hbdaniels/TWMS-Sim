// sim/components/ANBA.js
export class ANBA {
    constructor({ rate = 1 }) {
      this.rate = rate;
      this.counter = 0;
    }
  
    tick(state) {
      this.counter++;
      if (this.counter >= this.rate) {
        this.counter = 0;
        const coilId = `ANBA-coil-${Math.floor(Math.random() * 10000)}`;
        console.log(`🔁 ANBA shared I/O coil event: ${coilId}`);
      }
    }
  }