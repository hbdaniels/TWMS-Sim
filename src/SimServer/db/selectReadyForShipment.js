//selectReadyForShipment.js
// Handles DB for ShipmentManager.js

import oracledb from 'oracledb';
import { getConnection } from './connection.js';

export async function getCoilsThatNeedShipment() {
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

export async function getCoilsWithShippingOrderNumber() {
    const conn = await getConnection();
  
    const result = await conn.execute(
      `
      SELECT material_id
      FROM wob_data
      WHERE wob_id IN (
        SELECT wob_id
        FROM coil_data
        WHERE SHIPPINGORDERNUMBER IS NOT NULL
      )
      `,
      {}, // No bind variables
      { outFormat: oracledb.OUT_FORMAT_OBJECT } // Options
    );
  
    await conn.close();
  
    return result.rows.map(row => row.MATERIAL_ID);
  }

  export async function getCoilsThatNeedTrucks() {
    const conn = await getConnection();
  
    const result = await conn.execute(
      `
      SELECT w.material_id, c.shippingordernumber
      FROM wob_data w
      JOIN coil_data c ON w.wob_id = c.wob_id
      LEFT JOIN vehicle v ON c.shippingordernumber = v.shippingordernumber
      WHERE (c.shippingordernumber IS NOT NULL AND c.TRANSPORT_MODE_ORDER = 'TRUCK_EXTERNAL')
        AND v.shippingordernumber IS NULL
      `,
      {},
      { outFormat: oracledb.OUT_FORMAT_OBJECT }
    );
  
    await conn.close();
  
    return result.rows.map(row => ({
      materialId: row.MATERIAL_ID,
      shippingOrderNumber: row.SHIPPINGORDERNUMBER
    }));
  }

  export async function getTrucksThatNeedRegistration() {
    const conn = await getConnection();
  
    const result = await conn.execute(
      `
      SELECT * FROM VEHICLE WHERE VTYPE='T' AND SHIPPINGORDERNUMBER IS NOT NULL
      `,
      {}, // No bind variables
      { outFormat: oracledb.OUT_FORMAT_OBJECT } // Options
    );
  
    await conn.close();
  
    return result.rows;
  }
  
  export async function getCoilsThatNeedRailcars() {
    const conn = await getConnection();
  
    const result = await conn.execute(
      `
      SELECT w.material_id, c.shippingordernumber
      FROM wob_data w
      JOIN coil_data c ON w.wob_id = c.wob_id
      LEFT JOIN vehicle v ON c.shippingordernumber = v.shippingordernumber
      WHERE (c.shippingordernumber IS NOT NULL AND c.TRANSPORT_MODE_ORDER = 'RAILCAR' AND c.TRANSPORT_MODE != 'TRUCK_EXTERNAL')
        AND v.shippingordernumber IS NULL
      `,
      {},
      { outFormat: oracledb.OUT_FORMAT_OBJECT }
    );
  
    await conn.close();
  
    return result.rows.map(row => ({
      materialId: row.MATERIAL_ID,
      shippingOrderNumber: row.SHIPPINGORDERNUMBER
    }));
  }
  

  
