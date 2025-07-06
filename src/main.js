import { showScreen } from './ui/nav.js';
import './client/wsClient.js';


document.addEventListener('DOMContentLoaded', () => {
  document.querySelectorAll('[data-screen]').forEach(btn => {
    btn.addEventListener('click', () => {
      const screen = btn.getAttribute('data-screen');
      showScreen(screen);
    });
  });

  showScreen('cpl'); // default view
});
