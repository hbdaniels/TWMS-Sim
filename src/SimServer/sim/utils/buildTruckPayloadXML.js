// src/SimServer/xml/buildTruckPayloadXML.js

import { create } from 'xmlbuilder2';

/**
 * Builds XML payload for ArrayOfCVehicle.CPlannedWob
 * @param {Array<string>} materialIds - List of material IDs
 * @returns {string} - Serialized XML string
 */
export function buildTruckPayloadXML(materialIds) {
  const doc = create({ version: '1.0' })
    .ele('ArrayOfCVehicle.CPlannedWob', {
      xmlns: 'http://schemas.datacontract.org/2004/07/WCF',
      'xmlns:i': 'http://www.w3.org/2001/XMLSchema-instance'
    });

  for (const id of materialIds) {
    const wob = doc.ele('CVehicle.CPlannedWob');
    wob.ele('m_Coil', { 'i:nil': 'true' });
    wob.ele('m_MaterialID').txt(id);
    wob.ele('m_OrderKey').txt('0');
    wob.ele('m_TrayCoord').txt('0');
    wob.ele('m_VehicleLocation', { 'i:nil': 'true' });
    wob.up();
  }

  return doc.end({ prettyPrint: false });
}
