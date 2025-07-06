import { create } from 'xmlbuilder2';

export function buildMESDispatchXML(dispatch) {
  const header = {
    MESSAGE_NUMBER: '2004',
    MESSAGE_TYPE: 'MaterialForDispatch',
    DESTINATION: 'Hot Strip',
    TIMESTAMP: new Date().toISOString()
  };

  const doc = create({ version: '1.0', encoding: 'UTF-16' });
  const root = doc.ele('MESSAGE')
    .ele('HEADER')
      .ele('MESSAGE_NUMBER').txt(header.MESSAGE_NUMBER).up()
      .ele('MESSAGE_TYPE').txt(header.MESSAGE_TYPE).up()
      .ele('DESTINATION').txt(header.DESTINATION).up()
      .ele('TIMESTAMP').txt(header.TIMESTAMP).up()
    .up()
    .ele('MATERIAL_FOR_DISPATCH');

  // All normal dispatch fields except MATERIALS
  for (const [key, value] of Object.entries(dispatch)) {
    if (key === 'MATERIALS') continue;
    root.ele(key).txt(value);
  }

  // Optional MATERIALS array
  if (Array.isArray(dispatch.MATERIALS)) {
    for (const mat of dispatch.MATERIALS) {
      const { SAP_MATERIAL_CODE, SELECT, ...children } = mat;
      const matElem = root.ele('MATERIAL', {
        SAP_MATERIAL_CODE,
        SELECT
      });
      for (const [k, v] of Object.entries(children)) {
        matElem.ele(k).txt(v);
      }
      matElem.up();
    }
  }

  return doc.end({ prettyPrint: true });
}
