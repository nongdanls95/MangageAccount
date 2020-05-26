import { AppConfig } from '@app-base/interface/app-config';

export const APP_CONFIG: AppConfig = {
  Systems: [
    {
      SYSTEM_ID: 'GIS',
      SYSTEM_NAME: 'ระบบภูมิศาสตร์สารสนเทศ',
      SYSTEM_DETAIL: null,
      ORDER_NO: 1
    }, {
      SYSTEM_ID: 'UM',
      SYSTEM_NAME: 'ระบบจัดการผู้ใช้งาน',
      SYSTEM_DETAIL: null,
      ORDER_NO: 2
    }
  ],

  Functions: [
    {
      FUNCTION_ID: '1',
      FUNCTION_NAME: 'ค้นหา',
      FUNCTION_DETAIL: null,
      FUNCTION_PARENT: null,
      SYSTEM_ID: 'GIS',
      ORDER_NO: 1
    }, {
      FUNCTION_ID: '10',
      FUNCTION_NAME: 'เครื่องมือวาด',
      FUNCTION_DETAIL: null,
      FUNCTION_PARENT: null,
      SYSTEM_ID: 'GIS',
      ORDER_NO: 2
    }, {
      FUNCTION_ID: '6',
      FUNCTION_NAME: 'พิมพ์ภาพแผนที่',
      FUNCTION_DETAIL: null,
      FUNCTION_PARENT: null,
      SYSTEM_ID: 'GIS',
      ORDER_NO: 3
    }
  ],

  Services: [
    {
      SERVICE_ID: 'World_Imagery',
      SERVICE_NAME: 'แผนที่ภาพถ่ายดาวเทียม',
      SERVICE_URL: 'https://services.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer',
      SERVICE_TYPE: 'TILED',
      SERVICE_VISIBLE: 0,
      OPTIONS: null,
      SYSTEM_ID: 'GIS',
      ADD_MAP: 0,
      ADD_TOC: 0,
      IDENTIFY: 0,
      ORDER_NO: 1
    }, {
      SERVICE_ID: 'World_Street_Map',
      SERVICE_NAME: 'แผนที่ถนน',
      SERVICE_URL: 'https://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer',
      SERVICE_TYPE: 'TILED',
      SERVICE_VISIBLE: 0,
      OPTIONS: null,
      SYSTEM_ID: 'GIS',
      ADD_MAP: 0,
      ADD_TOC: 0,
      IDENTIFY: 0,
      ORDER_NO: 2
    }
  ],

  Layers: [
    {
      LAYER_ID: '5',
      LAYER_NAME: 'Landscape Trees',
      SERVICE_ID: 'Landscape_Trees',
      IDENTIFY: 1,
      INDEX_NO: 0,
      ORDER_NO: 1,
      OPACITY: 0.45
    }, {
      LAYER_ID: '1',
      LAYER_NAME: 'Census Block Points',
      SERVICE_ID: 'Census',
      IDENTIFY: 1,
      INDEX_NO: 0,
      ORDER_NO: 1,
      OPACITY: 1
    }
  ],

  General: [
    {
      CONFIG_NAME: 'TEST_NAME',
      CONFIG_VALUE: 'TEST_VALUE',
      CONFIG_DESCR: 'TEST_DESCR'
    }
  ]
};
