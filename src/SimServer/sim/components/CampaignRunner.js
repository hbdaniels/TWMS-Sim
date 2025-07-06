// sim/components/CampaignRunner.js

//Yet to be implemented!
import { CPL } from './CPL.js';

export class CampaignRunner {
  constructor({ campaigns = [], socketServer, tickRate = 1000 }) {
    this.campaigns = campaigns;
    this.socketServer = socketServer;
    this.tickRate = tickRate;
    this.state = {};
    this.currentIndex = 0;
    this.currentCPL = null;
    this.tickInterval = null;
  }

  start() {
    if (!this.campaigns.length) {
      console.warn('No campaigns to run.');
      return;
    }

    this.loadNextCampaign();

    this.tickInterval = setInterval(() => {
      if (this.currentCPL) {
        this.currentCPL.tick(this.state).then(() => {
          if (
            this.currentCPL.remaining <= 0 &&
            !this.currentCPL.transferCar &&
            this.currentCPL.deposits.every((d) => d === null)
          ) {
            this.loadNextCampaign();
          }
        });
      }
    }, this.tickRate);
  }

  loadNextCampaign() {
    if (this.currentIndex >= this.campaigns.length) {
      console.log('✅ All campaigns complete.');
      clearInterval(this.tickInterval);
      return;
    }

    const campaign = this.campaigns[this.currentIndex];
    this.currentIndex++;
    console.log(`🚦 Starting campaign ${this.currentIndex}/${this.campaigns.length}`);

    this.currentCPL = new CPL({
      rate: 5,
      socketServer: this.socketServer,
      messageInterval: 10,
      createInterval: 10,
      campaign
    });
  }
}

// Example usage elsewhere:
// const runner = new CampaignRunner({ campaigns: [...], socketServer: wss });
// runner.start();
