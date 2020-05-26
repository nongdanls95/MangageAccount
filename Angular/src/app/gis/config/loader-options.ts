import { ILoadScriptOptions } from 'esri-loader';

import { environment } from '@environments/environment';

import { dojoConfig } from '@gis/config/dojo-config';

export const loaderOptions: ILoadScriptOptions = {
  url: environment.arcgisjsapi.js,
  css: environment.arcgisjsapi.style,
  dojoConfig: dojoConfig
};
