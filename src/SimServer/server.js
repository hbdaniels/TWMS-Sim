// server.js
import { WebSocketServer } from 'ws';
import { handleMessage } from './messages/dispatcher.js';
import { SimClock } from './sim/SimClock.js';
import { CPL } from './sim/components/CPL.js';
import { SLH } from './sim/components/SLH.js';
import { ANBA } from './sim/components/ANBA.js';
import { Trucks } from './sim/components/Trucks.js';
import { Rail } from './sim/components/Rail.js';
import { buildMESMainDataXML } from './sim/utils/buildMESMainDataXML.js';
import { buildMESDispatchXML } from './sim/utils/buildMESDispatchXML.js';
import { createCoilQueueProvider } from './db/coilQueueProvider.js';

const wss = new WebSocketServer({ port: 3000 });
const simClock = new SimClock();
const coilProvider = createCoilQueueProvider({ batchSize: 10 });


import fs from 'fs';

//const campaign = JSON.parse(fs.readFileSync('./sim/campaigns/campaign_truck_steelgrade_dx54d.json', 'utf-8'));
import { generateMaterialForDispatch } from './sim/utils/fakerDispatch.js';

const dispatch = generateMaterialForDispatch({
    MATERIALS: [
      {
        material_id: '2104457270'  // Only this is required
      }
    ]
  });
const xml = buildMESDispatchXML(dispatch);
console.log(xml);

const cpl = new CPL({
    rate: 5,
    socketServer: wss,
    messageInterval: 10,     // every 10 ticks
    createInterval: 10,      // one coil every 2 (120) minutes (if tick = 1s)
    campaign: {
        count: 4,
        prefix: 'HD',
        //mat_type: 1001,
        width: 1220,
        weight: 22500,
        outside_diameter: 1800,
        internal_steelgrade: 'DX54D',
        customer_application_text: 'AUTOMOTIVE INC.',
        transport_mode: 'TRUCK_EXTERNAL',
        succesive_plant_code: "PACK",
        on_hold: 'N',
        prod_group: 'HRP'
      }
      
  });
  
const slh = new SLH({ rate: 4 });
const anba = new ANBA({ rate: 6 });
const trucks = new Trucks({
    rate: 10,
    coilProvider,
    socketServer: wss
  });

//const trucks = new Trucks({ rate: 10 });
const rail = new Rail({ rate: 12 });

simClock.addComponent(cpl);
simClock.addComponent(slh);
simClock.addComponent(anba);
simClock.addComponent(trucks);
simClock.addComponent(rail);
simClock.start();

console.log('🧠 TWMS-SIM WebSocket server running on ws://localhost:3000');

wss.on('connection', (ws) => {
  console.log('🔌 Client connected');

  ws.on('message', async (data) => {
    try {
      const message = JSON.parse(data);
      console.log('📥 Received:', message);
  
      if (message.type === 'remove_coil') {
        cpl.handleRemoval(message.payload);
      }
  
      await handleMessage(ws, message); // still call this if needed
    } catch (err) {
      console.error('❌ Invalid message:', err);
      ws.send(JSON.stringify({ type: 'error', payload: 'Invalid message format' }));
    }
  });
  

  ws.send(JSON.stringify({ type: 'welcome', payload: 'Connected to TWMS-SIM' }));
});