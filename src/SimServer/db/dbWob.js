// src/SimServer/db/dbWob.js

//import dotenv from 'dotenv';
import oracledb from 'oracledb';
import { getConnection } from './connection.js';
//dotenv.config();

export async function getCoilData(materialId) {
  const conn = await getConnection();

  const result = await conn.execute(
    `
    SELECT *
    FROM COIL_DATA
    WHERE MATERIAL_ID = :material_id
    `,
    { material_id: materialId }, 
    { outFormat: oracledb.OUT_FORMAT_OBJECT }
  );

  await conn.close();

  return result.rows;
}

export async function getCoilsAtPackagingOrCM() {
    const conn = await getConnection();
    //console.log('🔍 Fetching packing coils from DB...');
    const result = await conn.execute(
        `
        SELECT 
            w.*, 
            c.PACKINGSTAGE, c.SUCCESIVE_PLANT_CODE, c.TRANSPORT_MODE_ORDER
        FROM 
            wob_data w
        JOIN 
            coil_data c ON w.WOB_ID = c.WOB_ID
        WHERE 
            w.ROWNAME IN ('PAC', 'BR')
            AND
            w.BAY = 'ST21'
    
        `,
        [], // no bind variables
        { outFormat: oracledb.OUT_FORMAT_OBJECT }
      );    
  
    await conn.close();
  
    //console.log('📦 Raw packing coils from DB:', result.rows);
    return result.rows;
  }

  export async function getCoilsOnHold() {
    const conn = await getConnection();
    //console.log('🔍 Fetching on hold coils from DB...');
    const result = await conn.execute(
        `
        SELECT * FROM wob_data
        WHERE WOB_ID IN (
            SELECT WOB_ID 
            FROM COIL_DATA
            WHERE ON_HOLD = 'Y'
        )
        AND BAY = 'ST21'
        `,
        [], // no bind variables
        { outFormat: oracledb.OUT_FORMAT_OBJECT }
      );    
  
    await conn.close();

    //console.log('📦 Raw on hold coils from DB:', result.rows);
  
    return result.rows;
  }
