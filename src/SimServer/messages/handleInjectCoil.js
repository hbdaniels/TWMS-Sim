// messages/handleInjectCoil.js
export function handle(ws, payload) {
    const coilId = `coil-${Math.floor(Math.random() * 10000)}`;
    console.log(`🧪 Injecting coil: ${coilId}`, payload);
  
    ws.send(
      JSON.stringify({
        type: 'coil_injected',
        payload: { id: coilId, ...payload },
      })
    );
  }