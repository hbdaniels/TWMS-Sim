import * as cpl from './screens/cpl.js';
import * as slh from './screens/slh.js';
import * as anba from './screens/anba.js';
import * as trucks from './screens/trucks.js';
import * as rail from './screens/rail.js';

const screens = { cpl, slh, anba, trucks, rail };

export function showScreen(name) {
  const container = document.getElementById('screen');
  container.innerHTML = '';
  if (screens[name]) {
    screens[name].render(container);
  }
}
