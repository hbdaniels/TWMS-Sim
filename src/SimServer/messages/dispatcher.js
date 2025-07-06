// messages/dispatcher.js
export async function handleMessage(ws, message) {
    const { type, payload } = message;
    switch (type) {
      case 'inject_coil':
        return (await import('./handleInjectCoil.js')).handle(ws, payload);
      default:
        ws.send(JSON.stringify({ type: 'error', payload: `Unknown message type: ${type}` }));
    }
  }