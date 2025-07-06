// src/SimServer/db/insertMesData.js

import oracledb from 'oracledb';
import dotenv from 'dotenv';
import { getConnection } from './connection.js';
dotenv.config();

// const poolPromise = oracledb.createPool({
//   user: process.env.DB_USER,
//   password: process.env.DB_PASSWORD,
//   connectString: process.env.DB_CONNECT_STRING,
//   poolMin: 1,
//   poolMax: 4,
//   poolIncrement: 1
// });

export async function insertMesData(coil) {
  try {
    const conn = await getConnection();
    // const pool = await poolPromise;
    // const conn = await pool.getConnection();

    const result = await conn.execute(
      `INSERT INTO mes_receive (
        MESSAGE_NO, STATUS, XML_DATA, PRIORITY, T_CREATED, REMARK 
      ) VALUES (
        :message_no, :status, :xml_data, :priority, SYSDATE, :remark
      )`,
      {
        message_no: '2001', // arbitrary ID for test
        status: 0,
        xml_data: coil.xml,
        priority: 50,
        remark: 'TWMS-SIM Generated MES',
      },
      { autoCommit: true }
    );

    await conn.close();
    return result;
  } catch (err) {
    console.error('[DB INSERT ERROR]', err);
    return null;
  }
}

export async function insertDispatchData(dispatch) {
    try {
      const conn = await getConnection(); // ✅ Use the new connection method
  
      const result = await conn.execute(
        `INSERT INTO mes_receive (
          MESSAGE_NO, STATUS, XML_DATA, PRIORITY, T_CREATED, REMARK
        ) VALUES (
          :message_no, :status, :xml_data, :priority, SYSDATE, :remark
        )`,
        {
          message_no: '2004',
          status: 0,
          xml_data: dispatch.xml,
          priority: 50,
          remark: 'TWMS-SIM MaterialForDispatch',
        },
        { autoCommit: true }
      );
  
      await conn.close();
      return result;
    } catch (err) {
      console.error('[DB INSERT ERROR]', err);
      return null;
    }
  }
  
