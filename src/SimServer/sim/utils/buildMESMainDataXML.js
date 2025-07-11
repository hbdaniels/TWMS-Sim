// sim/utils/buildMESMainDataXML.js
import { create } from 'xmlbuilder2';

export function buildMESMainDataXML(coil) {
  const header = {
    MESSAGE_NUMBER: '2001',
    MESSAGE_TYPE: 'MaterialMainData',
    DESTINATION: 'Hot Strip',
    TIMESTAMP: new Date().toISOString()
  };

  const root = create({ version: '1.0', encoding: 'UTF-8' })
    .ele('MESSAGE')
      .ele('HEADER')
        .ele('MESSAGE_NUMBER').txt(header.MESSAGE_NUMBER).up()
        .ele('MESSAGE_TYPE').txt(header.MESSAGE_TYPE).up()
        .ele('DESTINATION').txt(header.DESTINATION).up()
        .ele('TIMESTAMP').txt(header.TIMESTAMP).up()
      .up()
      .ele('MATERIAL_MAIN_DATA');

      for (const [key, value] of Object.entries(coil)) {
        if (Array.isArray(value)) {
          const arrayNode = root.ele(key);
          value.forEach((entry) => {
            const itemNode = arrayNode.ele('ITEM');
            for (const [k, v] of Object.entries(entry)) {
              itemNode.ele(k).txt(v ?? '');
            }
            itemNode.up();
          });
          arrayNode.up();
        } else if (value && typeof value === 'object') {
          const objectNode = root.ele(key);
          for (const [subKey, subVal] of Object.entries(value)) {
            objectNode.ele(subKey).txt(subVal ?? '');
          }
          objectNode.up();
        } else {
          root.ele(key).txt(value ?? '');
        }
      }
      

  return root.end({ prettyPrint: true });
}
