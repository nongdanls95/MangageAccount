export interface System {
  SYSTEM_ID: string;
  SYSTEM_NAME: string;
  SYSTEM_DETAIL: string;
  ORDER_NO: number;
}

export interface Function {
  FUNCTION_ID: string;
  FUNCTION_NAME: string;
  FUNCTION_DETAIL: string;
  FUNCTION_PARENT: string;
  SYSTEM_ID: string;
  ORDER_NO: number;
}

export interface Service {
  SERVICE_ID: string;
  SERVICE_NAME: string;
  SERVICE_URL: string;
  SERVICE_TYPE: string;
  SERVICE_VISIBLE: number;
  OPTIONS: string;
  SYSTEM_ID: string;
  ADD_MAP: number;
  ADD_TOC: number;
  IDENTIFY: number;
  ORDER_NO: number;
}

export interface Layer {
  LAYER_ID: string;
  LAYER_NAME: string;
  SERVICE_ID: string;
  IDENTIFY: number;
  INDEX_NO: number;
  ORDER_NO: number;
  OPACITY: number;
}

export interface General {
  CONFIG_NAME: string;
  CONFIG_VALUE: any;
  CONFIG_DESCR: string;
}

/**
 * export เฉพาะ export interface นี้เท่านั้น
 */
export interface AppConfig {
  Systems?: System[];
  Functions?: Function[];
  Services?: Service[];
  Layers?: Layer[];
  General?: General[];
}
