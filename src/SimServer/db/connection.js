// src/SimServer/db/connection.js
import oracledb from 'oracledb';

let pool;

export async function getConnection() {
  if (!pool) {
    pool = await oracledb.createPool({
      user: process.env.DB_USER || 'hotstrip2024',
      password: process.env.DB_PASSWORD || 'h0t5tr1p202a',
      connectString: process.env.DB_CONNECT_STRING || 'QTWMS',
      poolMin: 1,
      poolMax: 4,
      poolIncrement: 1
    });
  }
  return pool.getConnection();
}
