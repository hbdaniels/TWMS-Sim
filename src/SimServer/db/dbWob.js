// src/SimServer/db/dbWob.js

//import dotenv from 'dotenv';
import { getConnection } from './connection.js';
dotenv.config();

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
