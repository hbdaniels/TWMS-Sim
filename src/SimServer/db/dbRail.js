//dbRail.js
//import dotenv from 'dotenv';
import oracledb from 'oracledb';
import { getConnection } from './connection.js';
//dotenv.config();

// Undefined = 0,
// Prime = 1,
// Excess_Prime = 2,
// Secondary = 3,
// Scrap = 4,
// Transition_Piece = 5,
// Salvage = 6,
// Bulk_Scrap = 7

export async function getCoilsReadyForRailShipment(bay) {
  const conn = await getConnection();

  const result = await conn.execute(
    `
    SELECT w.MATERIAL_ID, c.*
    FROM COIL_DATA c
    JOIN WOB_DATA w ON c.WOB_ID = w.WOB_ID
    WHERE (c.TRANSPORT_MODE = 'RAILCAR' 
      OR c.TRANSPORT_MODE_ORDER = 'RAILCAR')
      AND c.TRANSPORT_MODE_ORDER != 'TRUCK_EXTERNAL' 
      AND c.SHIPPINGORDERNUMBER IS NULL
      AND c.PACKINGSTAGE = 'C'
      AND c.MATERIAL_TYPE NOT IN (3, 4, 5, 6, 7) -- Exclude Secondary, Scrap, Transition_Piece, Salvage, Bulk_Scrap;
    `, 
    [],
    { outFormat: oracledb.OUT_FORMAT_OBJECT }
  );

  await conn.close();

  return result.rows;
}