//selectReadyForShipment.js
import oracledb from 'oracledb';
import { getConnection } from './connection.js';

export async function getReadyToShipCoils() {
  const conn = await getConnection();

  const result = await conn.execute(
    `
    SELECT material_id
    FROM wob_data
    WHERE wob_id IN (
      SELECT wob_id
      FROM coil_data
      WHERE transport_mode_order = 'TRUCK_EXTERNAL'
      AND PACKINGSTAGE = 'C'
      AND SHIPPINGORDERNUMBER IS NULL
    )
    `,
    {}, // No bind variables
    { outFormat: oracledb.OUT_FORMAT_OBJECT } // Options
  );

  await conn.close();

  return result.rows.map(row => row.MATERIAL_ID);
}
