// testCPLAgent.js
import { WebSocketServer } from 'ws';
import { SimClock } from './sim/SimClock.js'; // <-- your production SimClock
import { CPLAgent } from './sim/components/CPLAgent.js';

(async () => {
  //Setup WebSocket server (simulates Node-RED or other clients)
  const wss = new WebSocketServer({ port: 3000 });

  wss.on('connection', (ws) => {
    console.log('🧩 Node-RED connected to WS');
    ws.on('message', (data) => {
        const message = JSON.parse(data);
        console.log('📥 WS Message:', message);
      
        if (message.type === 'remove_coil') {
          cplAgent.handleRemoval(message.payload);
        }
      });
      
  });

  //Create CPLAgent with a short campaign
  const cplAgent = new CPLAgent({
    campaign: {
      count: 100,
      prefix: 'SIM',
      width: 1000,
      weight: 19000,
      //outside_diameter: 1600,
      internal_steelgrade: 'DX52D',
      customer_application_text: 'TEST COILS INC.',
      transport_mode: '02', //01= Truck, 02=Rail, 03=Barge
      packaging_type: 'EX',
      successive_plant_code: 'PACK',
      previous_plant_code: 'CPL',
      flag_stainless: 'N',
      on_hold: 'N',
      prod_group: 'HRP',
      scrap_index: 'N',
      forceTick: () => cplAgent.tick({}),
    },
    createInterval: 5,
    socketServer: wss,
    
  });

  // 🕰️ Setup and start SimClock just like in prod
  const simClock = new SimClock(1000); // 1 second per tick
  simClock.addComponent(cplAgent);
  simClock.start();

  console.log('🚀 SimClock + CPLAgent test started.\n');

  // Optional: Stop after all coils are produced
  const monitor = setInterval(() => {
    if (cplAgent.remaining <= 0 && cplAgent.transferCar === null && cplAgent.deposits.every(d => d === null)) {
      console.log('✅ CPLAgent campaign complete. Shutting down.');
      simClock.stop();
      wss.close();
      clearInterval(monitor);
    }
  }, 1000);
})();
