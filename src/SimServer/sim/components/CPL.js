// sim/components/CPL.js
import { generateCoil } from '../utils/fakerCoil.js';
import { buildMESMainDataXML } from '../utils/buildMESMainDataXML.js';
import oracledb from 'oracledb';

function formatDate(date) {
  return date.toISOString().replace('T', ' ').substring(0, 19).replace(/-/g, '.');
}

function buildCPLExitMessage(deposits) {
  const segments = deposits.map((coil, index) => {
    if (!coil) {
      return '               ;00000;0000;0000;00000';
    }
    const matId = (coil.MATERIAL_ID || '').padEnd(15, ' ');
    const weight = String(coil.WEIGHT || '0').padStart(5, '0');
    const width = String(coil.WIDTH || '0').padStart(4, '0');
    const dia = String(coil.OUTSIDE_DIAMETER || '0').padStart(4, '0');
    const deposit = String((index + 1) * 100).padStart(5, '0');
    return `${matId};${weight};${width};${dia};${deposit}`;
  });
  while (segments.length < 6) {
    segments.push('               ;00000;0000;0000;00000');
  }
  const timestamp = formatDate(new Date());
  return `*${timestamp};00901;0000;CPL  ;TWMS Data:${segments.join(';')}#`;
}

function printLayout(deposits, transferCar) {
    const layout = deposits.map((c, i) => c ? `[${i + 3}:${c.MATERIAL_ID.slice(-5)}]` : `[${i + 3}:_____]`);
    const tcar = transferCar ? `Transfer: ${transferCar.MATERIAL_ID}` : 'Transfer: empty';
    console.log(`🏗️ Layout → ${tcar} → ${layout.join(' ')}`);
  }

export class CPL {
  constructor({ rate = 1, campaign = {}, socketServer = null, messageInterval = 10, createInterval = 60 }) {
    this.rate = rate;
    this.counter = 0;
    this.remaining = campaign.count || 10;
    this.params = campaign;
    this.socketServer = socketServer;
    this.deposits = [null, null, null];
    this.transferCar = null;
    this.downtime = false;
    this.downtimeTicks = 0;
    this.totalDowntime = 0;
    this.createInterval = createInterval;
    this.lastMessage = '';
  }

  async tick(state) {
    let changed = false;

    if (this.transferCar) {
      for (let i = 2; i >= 0; i--) {
        const precedingFull = this.deposits.slice(i + 1).every(coil => coil !== null);
        if (this.deposits[i] === null && precedingFull) {
          this.deposits[i] = this.transferCar;
          this.transferCar = null;
          changed = true;
          if (this.downtime) {
            console.log(`✅ CPL resumed from downtime (${this.downtimeTicks} ticks)`);
            this.totalDowntime += this.downtimeTicks;
          }
          this.downtime = false;
          this.downtimeTicks = 0;
          console.log(`📤 Coil moved to deposit ${i + 1}`);
          break;
        }
      }

      if (this.transferCar) {
        if (!this.downtime) {
          console.log('⛔ CPL downtime started: all valid deposits blocked');
        }
        this.downtime = true;
        this.downtimeTicks++;
      }
    }


    this.counter++;
    if (this.counter >= this.createInterval && this.remaining > 0 && !this.downtime) {
      this.counter = 0;
      const coil = generateCoil(this.params);
      //come back and reconfigure this to be in the generatCoil() logic
      //const isOnHold = Math.random() < 0.58;
      //coil.ON_HOLD = isOnHold ? 'Y' : 'N';

      console.log(`🔁 CPL created coil ${coil.MATERIAL_ID}`);
      this.remaining--;

      if (!state.injectedCoils) state.injectedCoils = [];
      state.injectedCoils.push(coil);

      const mesXML = buildMESMainDataXML(coil);
      coil.xml = mesXML;

      this.socketServer?.clients.forEach((client) => {
        if (client.readyState === 1) {
          client.send(JSON.stringify({ type: 'mes_data', payload: mesXML }));
        }
      });

      try {
        const conn = await oracledb.getConnection({
          user: 'hotstrip2024',
          password: 'h0t5tr1p202a',
          connectString: 'QTWMS'
        });

        await conn.execute(
          `INSERT INTO mes_receive (
            MESSAGE_NO, STATUS, XML_DATA, PRIORITY, T_CREATED, REMARK
          ) VALUES (
            :message_no, :status, :xml_data, :priority, SYSDATE, :remark
          )`,
          {
            message_no: 2001,
            status: 0,
            xml_data: coil.xml,
            priority: 50,
            remark: 'TWMS-SIM Generated MES'
          },
          { autoCommit: true }
        );

        await conn.close();
      } catch (err) {
        console.error('[DB INSERT ERROR]', err);
      }

      this.transferCar = coil;
      console.log('🚗 Coil placed on transfer car');
    }

    if (changed) {
      const exitMessage = buildCPLExitMessage(this.deposits);
      if (exitMessage !== this.lastMessage) {
        this.lastMessage = exitMessage;
        this.socketServer?.clients.forEach((client) => {
          if (client.readyState === 1) {
            client.send(JSON.stringify({ type: 'cpl_exit_message', payload: exitMessage }));
          }
        });
        printLayout(this.deposits, this.transferCar);
      }
    }
  }

  handleRemoval(materialId) {
    const idx = this.deposits.findIndex(coil => coil?.MATERIAL_ID === materialId);
    if (idx !== -1) {
      console.log(`🗑️ Coil removed from deposit ${idx + 1}`);
      this.deposits[idx] = null;
      const exitMessage = buildCPLExitMessage(this.deposits);
      if (exitMessage !== this.lastMessage) {
        this.lastMessage = exitMessage;
        this.socketServer?.clients.forEach((client) => {
          if (client.readyState === 1) {
            client.send(JSON.stringify({ type: 'cpl_exit_message', payload: exitMessage }));
          }
        });
        printLayout(this.deposits, this.transferCar);
      }
      if (this.downtime) {
        console.log(`✅ CPL resumed due to external removal`);
        this.totalDowntime += this.downtimeTicks;
        this.downtime = false;
        this.downtimeTicks = 0;
      }
    } else {
      console.warn(`⚠️ Coil ${materialId} not found in any deposit`);
    }
  }
}
