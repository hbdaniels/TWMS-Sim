// pollWobData.js
import oracledb from 'oracledb';
import WebSocket from 'ws';


// ---- CONFIG ----
const config = {
  db: {
    user: 'hotstrip2024',
    password: 'h0t5tr1p202a',
    connectString: 'QTWMS'
  },
  ws: {
    url: 'ws://localhost:3000'
  },
  pollIntervalMs: 2000
};

const monitoredZones = {
    CPL:    { matchRow: 'CPL' },
    SLH:    { matchRow: 'SLH' },
    ANBA:   { matchRow: 'ANBA' },
    TRUCKS: { matchBay: 'TRUCK' },
    RAIL:   { matchBay: 'RAIL' },
    PACK:   { matchRow: 'PAC' },
    COIL_MASTER: { matchRow: 'CM' }
  };
  
  function matchesZone(location, { matchRow, matchBay }) {
    if (!location) return false;
    const parts = location.split(":");
    const bay = parts[1];
    const row = parts[3];
    return (matchRow && row === matchRow) || (matchBay && bay === matchBay);
  }
  
  let previousMap = new Map();
  let ws;
  
  async function main() {
    try {
      const connection = await oracledb.getConnection(config.db);
      console.log('[DB] Connected to Oracle');
  
      function connectWebSocket() {
        ws = new WebSocket(config.ws.url);
      
        ws.on('open', () => {
          console.log('[WS] Connected to TWMS-SIM');
        });
      
        ws.on('error', (err) => {
          console.error('[WS ERROR]', err.message);
        });
      
        ws.on('close', () => {
          console.warn('[WS] Disconnected. Retrying in 5 seconds...');
          setTimeout(connectWebSocket, 5000);
        });
      }
      
      
      connectWebSocket();
      

      
  
      setInterval(async () => {
        try {
          const result = await connection.execute(
            `SELECT MATERIAL_ID,
                    MAINAREA || ':' || BAY || ':' || AREA || ':' || ROWNAME || ':' || LOCATION AS STORAGE_LOCATION,
                    LAST_CHANGE,
                    VEHICLE_ID
             FROM WOB_DATA
             WHERE LAST_CHANGE > SYSDATE - (1/24/60/60)`
          );
  
          const currentMap = new Map();
  
          for (const row of result.rows) {
            const [materialId, location, lastChange, vehicleId] = row;
            currentMap.set(materialId, location);
  
            const prevLocation = previousMap.get(materialId);
  
            for (const [zoneName, criteria] of Object.entries(monitoredZones)) {
              const wasInZone = matchesZone(prevLocation, criteria);
              const nowInZone = matchesZone(location, criteria);
  
              if (wasInZone && !nowInZone) {
                const dest = vehicleId || location?.split(":" )[3] || '[UNKNOWN]';
                console.log(`[PICKUP] ${materialId} picked up from ${zoneName}, now at ${dest}`);
  
                if (zoneName === 'CPL' && ws && ws.readyState === WebSocket.OPEN) {
                  ws.send(JSON.stringify({
                    type: 'remove_coil',
                    payload: materialId
                  }));
                }
              }
  
              if (!wasInZone && nowInZone) {
                console.log(`[PLACE] ${materialId} placed at ${zoneName}`);
              }
            }
          }
  
          previousMap = currentMap;
        } catch (err) {
          console.error('[Poll Error]', err);
        }
      }, config.pollIntervalMs);
    } catch (err) {
      console.error('[Startup Error]', err);
    }
  }
  
  main();
  
