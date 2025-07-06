// sim/SimClock.js
export class SimClock {
    constructor(tickInterval = 1000) {
      this.tickInterval = tickInterval;
      this.components = [];
      this.globalState = {};
    }
  
    addComponent(component) {
      this.components.push(component);
    }
  
    start() {
      setInterval(() => {
        this.components.forEach((c) => c.tick(this.globalState));
      }, this.tickInterval);
    }
  }
  