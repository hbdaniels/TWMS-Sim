// sim/utils/fakerCoil.js
import { faker } from '@faker-js/faker';

export function generateCoil(overrides = {}) {
  const prefix = overrides.prefix || '11';
  const axisValue = overrides.mat_axis ?? faker.helpers.arrayElement([0, 2]);

  const internalSteelGrades = ['', '36AA8', '3CB21', '50AA2', 'M6R78', '430710', 'M4A34', '37AB1'];
  const productGroups = ['HR', '', 'HRP', 'CRFH', 'CR'];
  const sortGrades = ['0', '3', '4', '6', '7'];
  const previousPlantCodes = ['', 'HSM', 'PACK', 'SLH'];
  const successivePlantCodes = ['', 'PLTCM', 'PACK', 'CPL', 'SLH', 'LC_SP', 'ANBA'];
  const packagingTypes = ['', 'EX', 'UN', 'NL', 'LP', 'NW', 'HB'];
  const customerBriefDesignations = ['IND', '', 'AUT'];
  const customerApplicationTexts = [
    '', 'IDS EXPOSED', 'AUTOMOTIVE UNEXPOSED PART', 'IDS UNEXPOSED', 'AUTOMOTIVE SAFETY PART',
    'PRESS HARDENED STEEL', 'PIPE TUBE API', 'AUSTENITIC C', 'PIPE TUBE IDS', 'AUTO VERTICAL EXPOSED PART',
    'FERRITIC C', 'AUTO HORIZONTAL EXPOSED PART', 'IDS EXPOSEDELECTRICAL PANEL', 'AUSTENITIC H',
    'AUTOMOTIVE SEMI EXPOSED', 'AUTO EXPOSED', 'STAINLESS', 'IDS SEMI EXPOSED', 'IDS SAFETY PART',
    'DRUM BODY', 'IDSAPPLIANCE UNEXPOSED', 'RESALEMISC', 'PIPE TUBE MECHANICAL', 'IDS PREPAINT',
    'APPLIANCE EXPOSED', 'PIPE TUBE STRUCTURAL', 'APPLIANCE SEMI EXPOSED', 'BRACKET',
    'AUTO UNEXPOSED SAFETY PART', 'LASER BURNEDFABRICATED PARTS'
  ];

  return {
    TWMS_REQ_ID: overrides.twms_req_id || '',
    MATERIAL_ID: overrides.material_id || prefix + faker.string.numeric(10),
    FOREIGN_ID: overrides.foreign_id || faker.string.alphanumeric(10),
    RFID_ID: overrides.rfid_id || faker.string.alphanumeric(17),
    MAT_TYPE: overrides.mat_type || faker.number.int({ min: 2, max: 3 }),
    FOREIGN_MATERIAL_INDEX: overrides.foreign_material_index || faker.helpers.arrayElement(['','3','P','T','L','CSA','4']),
    MAT_AXIS: axisValue,
    MAT_PALLET: overrides.mat_pallet || axisValue,
    WEIGHT: overrides.weight || faker.number.int({ min: 14000, max: 45000 }),
    WEIGHT_ORIGIN: overrides.weight_origin || faker.helpers.arrayElement(['C', 'A']),
    HEIGHT: overrides.height || faker.number.int({ min: 100, max: 999 }),
    THICKNESS: overrides.thickness || faker.number.float({ min: 0.1, max: 10, precision: 0.001 }),
    LENGTH: overrides.length || faker.number.int({ min: 100, max: 650 }),
    WIDTH: overrides.width || faker.number.int({ min: 919, max: 1600 }),
    INSIDE_DIAMETER: overrides.inside_diameter || faker.helpers.arrayElement(['610', '760']),
    OUTSIDE_DIAMETER: overrides.outside_diameter || faker.number.int({ min: 1000, max: 2000 }),
    TRANSFER_BAR_INDEX: overrides.transfer_bar_index || faker.helpers.arrayElement(['Y', 'N']),
    SCRAP_INDEX: overrides.scrap_index || faker.helpers.arrayElement(['Y', 'N']),
    TRANSITION_COIL_INDEX: overrides.transition_coil_index || faker.helpers.arrayElement(['Y', 'N']),
    FLAG_STAINLESS: overrides.flag_stainless || faker.helpers.arrayElement(['Y', 'N']),
    FLAG_MANDATORY_HOT_CHARGE: overrides.flag_mandatory_hot_charge || faker.helpers.arrayElement(['Y', 'N']),
    FLAG_PRODUCED: overrides.flag_produced || faker.helpers.arrayElement(['Y', 'N']),
    PLANNED_PRODUCTION_DATE: overrides.planned_production_date || new Date().toISOString(),
    OILED: overrides.oiled || faker.helpers.arrayElement(['Y', 'N']),
    SURFACE_SENSITIVITY: overrides.surface_sensitivity || faker.helpers.arrayElement(['E', 'U', 'S']),
    FLAG_DESEAM: overrides.flag_deseam || faker.helpers.arrayElement(['Y', 'N']),
    FLAG_INSPEC: overrides.flag_inspec || faker.helpers.arrayElement(['Y', 'N']),
    REWORK_INDEX: overrides.rework_index || faker.helpers.arrayElement(['Y', 'N']),
    FLAG_SAMPLE: overrides.flag_sample || faker.helpers.arrayElement(['Y', 'N']),
    FLAG_BLACK_MAT: overrides.flag_black_mat || faker.helpers.arrayElement(['Y', 'N']),
    INTERNAL_STEELGRADE: overrides.internal_steelgrade || faker.helpers.arrayElement(internalSteelGrades),
    PROD_GROUP: overrides.prod_group || faker.helpers.arrayElement(productGroups),
    SORT_GRADE: overrides.sort_grade || faker.helpers.arrayElement(sortGrades),
    RIO_CODE: overrides.rio_code || '',
    ARTICLE: overrides.article || '',
    HEAT_NUMBER: overrides.heat_number || '',
    DATE_CREATION_OF_MATERIAL: overrides.date_creation_of_material || new Date().toISOString(),
    COIL_TEMPERATURE_BODY: overrides.coil_temperature_body || '0',
    TEMPERATURE_DATE: overrides.temperature_date || '',
    PREVIOUS_PLANT_CODE: overrides.previous_plant_code || faker.helpers.arrayElement(previousPlantCodes),
    SUCCESIVE_PLANT_CODE: overrides.successive_plant_code || faker.helpers.arrayElement(successivePlantCodes),
    EST_PROD_DATE: overrides.est_prod_date || new Date().toISOString(),
    PIECE_STATUS: overrides.piece_status || '',
    ON_HOLD: overrides.on_hold || faker.helpers.arrayElement(['Y', 'N']),
    ON_HOLD_REASON: overrides.on_hold_reason || faker.lorem.sentence().slice(0, 256),
    PACKAGING_TYPE: overrides.packaging_type || faker.helpers.arrayElement(packagingTypes),
    PACKAGING_STATUS: overrides.packaging_status || faker.helpers.arrayElement(['N', 'Y']),
    GROSS_WEIGHT: overrides.gross_weight || 0,
    MATERIAL_TYPE: overrides.material_type || faker.number.int({ min: 0, max: 1 }),
    CUSTOMER_BRIEF_DESIGNATION: overrides.customer_brief_designation || faker.helpers.arrayElement(customerBriefDesignations),
    CUSTOMER_APPLICATION_TEXT: overrides.customer_application_text || faker.helpers.arrayElement(customerApplicationTexts)
  };
}