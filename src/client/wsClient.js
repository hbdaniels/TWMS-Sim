// /src/client/wsClient.js
const socket = new WebSocket('ws://localhost:3000');

socket.onopen = () => {
  console.log('🌐 Connected to TWMS-SIM server');
};

socket.onmessage = (event) => {
  const message = JSON.parse(event.data);
  switch (message.type) {
    case 'welcome':
      console.log('✅ Server:', message.payload);
      break;
    case 'mes_data':
      console.log('📨 MES Message:\n', message.payload);
      break;
    case 'coil_injected':
      console.log('🧲 Coil Injected:', message.payload.MATERIAL_ID);
      break;
    default:
      console.warn('❓ Unknown message type:', message);
  }
};

socket.onerror = (err) => {
  console.error('🚨 WebSocket error:', err);
};

socket.onclose = () => {
  console.warn('🔌 Disconnected from TWMS-SIM server');
};
