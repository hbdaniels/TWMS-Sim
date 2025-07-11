import { SimClock } from './sim/SimClock.js';
import { PackagingAgent } from './sim/components/PackagingAgent.js';

const packagingAgent = new PackagingAgent({
  // optionally override default rates here
});

await packagingAgent.init(); // recommended before ticking starts

const simClock = new SimClock(200); // or whatever your tick interval is
simClock.addComponent(packagingAgent);
simClock.start();
