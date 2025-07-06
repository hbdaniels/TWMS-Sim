// sim/components/SLH.js
export class SLH {
    constructor({ rate = 1 }) {
      this.rate = rate;
      this.counter = 0;
    }
  
    tick(state) {
      this.counter++;
      if (this.counter >= this.rate) {
        this.counter = 0;
        const coilId = `SLH-exit-coil-${Math.floor(Math.random() * 10000)}`;
        console.log(`🔁 SLH injecting coil at exit: ${coilId}`);
      }
    }
  }