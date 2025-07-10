// sim/utils/fakerDispatch.js
import { faker } from '@faker-js/faker';

export function generateMaterialForDispatch(overrides = {}) {
  const materials = overrides.MATERIALS || [];
  if (!materials.length || !materials[0].MATERIAL_ID) {
    throw new Error("MATERIAL_ID is required in MATERIALS[0]");
  }

  return {
    SHIPPING_ORDER_NUMBER: overrides.SHIPPING_ORDER_NUMBER || faker.string.numeric(10),
    SHIPMENT_PRIORITY: overrides.SHIPMENT_PRIORITY || faker.helpers.arrayElement(['1', '2', '3', '4', '5']),
    TRANSPORT_MODE: overrides.TRANSPORT_MODE || faker.helpers.arrayElement(['01', '02', '03', '04', '05']),
    LOADING_INSTRUCTION: overrides.LOADING_INSTRUCTION || faker.lorem.sentence().slice(0, 256),
    SHIP_TO_TEXT: overrides.SHIP_TO_TEXT || faker.location.streetAddress(),
    SHIP_TO_ADDRESS: overrides.SHIP_TO_ADDRESS || faker.location.secondaryAddress(),
    SHIP_TO_COUNTRY: overrides.SHIP_TO_COUNTRY || faker.location.countryCode('alpha-2'),
    SHIP_TO_NUMBER: overrides.SHIP_TO_NUMBER || faker.string.numeric(10),
    SPECIAL_INSTRUCTION: overrides.SPECIAL_INSTRUCTION || faker.lorem.sentence().slice(0, 256),
    EXECUTION_DATE: overrides.EXECUTION_DATE || new Date().toISOString().split('T')[0].replace(/-/g, ''),
    SHIPMENT_STATUS: overrides.SHIPMENT_STATUS || faker.helpers.arrayElement(['1', '2', '3', '4', '5']),
    SAP_MATERIAL_CODE: overrides.SAP_MATERIAL_CODE || faker.string.numeric(12),
    MATERIALS: materials.map((m) => ({
      SAP_MATERIAL_CODE: m.sap_material_code || faker.string.numeric(12),
      SELECT: m.select || '1',
      MATERIAL_ID: m.MATERIAL_ID,
      COUNT: m.count || '1',
      MES_QUALITY_STATUS: m.mes_quality_status || 'R',
      SAP_RTS: m.sap_rts || 'Y',
      MES_RFD: m.mes_rfd || 'Y',
      REQUESTED_DATE: m.requested_date || new Date().toISOString().split('T')[0].replace(/-/g, ''),
      INSTRUCTION: m.instruction || 'No Restrictions'
    }))
  };
}

export function createFakeShippingOrderNumber(){
    return faker.string.numeric(10);
}


export function createFakeTruckName(){
    return "SIM_TRUCK-" + faker.string.alphanumeric(6);
}
export function createFakeRailcarName(){
    return "SIM_WAGON-" + faker.string.alphanumeric(6);
}