// The file contents for the current environment will overwrite these during build.
// The build system defaults to the dev environment which uses `environment.ts`, but if you do
// `ng build --env=prod` then `environment.prod.ts` will be used instead.
// The list of which env maps to which file can be found in `.angular-cli.json`.

export const environment = {
  production: false,

  arcgisjsapi: {
    js: 'https://appserver2.cdg.co.th/arcgis_js_api/library/3.27/3.27/init.js',
    style: 'https://appserver2.cdg.co.th/arcgis_js_api/library/3.27/3.27/esri/css/esri.css',
    dojo: 'https://appserver2.cdg.co.th/arcgis_js_api/library/3.27/3.27/dijit/themes/claro/claro.css'
  }

  // arcgisjsapi: {
  //   js: location.protocol + '//localhost/arcgis_js_api/library/3.30/init.js',
  //   style: location.protocol + '//localhost/arcgis_js_api/library/3.30/esri/css/esri.css',
  //   dojo: location.protocol + '//localhost/arcgis_js_api/library/3.30/dijit/themes/claro/claro.css',
  //   styleMesurement: location.protocol + '//localhost/arcgis_js_api/library/3.30/esri/themes/calcite/dijit/calcite.css',
  //   styleMesurement_2: location.protocol + '//localhost/arcgis_js_api/library/3.30/esri/themes/calcite/esri/esri.css'
  // },
};
