import { Injectable } from '@angular/core';
import { loadModules } from 'esri-loader';

import { loaderOptions } from '@gis/config/loader-options';

@Injectable()
/**
 * test
 */
export class GeometryUtilService {

  private jsonUtils: any;

  /**
   * เก็บค่า wkid เริ่มต้นเป็น EPSG Projection 32647 - WGS 84 / UTM Zone 47N
   * - หากแอพฯ ใช้ระบบพิกัดฉากแบบไหนให้เปลี่ยนค่านี้ให้ตรงกับแอพฯ ด้วย เช่น
   *   WGS 1984 Web Mercator(102100) กำหนด _dbProjection = 102100
   */
  protected _dbProjection: number = 32647;

  constructor() {
    loadModules(['esri/geometry/jsonUtils'], loaderOptions).then(([jsonUtils]) => this.jsonUtils = jsonUtils);
  }

  /**
     * Convert from st_geometry string to geometry(esri.geometry.Geometry) json.
     */
  public fromST(stringST: string): any {
    let geomJson: object = {};
    let stringSplite: string[] = [];
    let stringSplite2: string[] = [];
    let stringSplite3: string[] = [];
    let i: number = 0;
    let j: number = 0;

    stringST = stringST.trim();
    
    if (stringST.indexOf('POINT') == 0) {
      stringST = stringST.replace('POINT', '');
      stringST = stringST.trim();
      stringST = stringST.replace('(', '').replace(')', '');
      stringST = stringST.trim();
      stringSplite = stringST.split(' ');
      geomJson['x'] = Number(stringSplite[0]);
      geomJson['y'] = Number(stringSplite[1]);
    } else if (stringST.indexOf('MULTIPOINT') == 0) {
      geomJson['points'] = new Array();
      stringST = stringST.replace('MULTIPOINT', '');
      stringST = stringST.trim();
      stringST = stringST.replace(/\(/igm, '').replace(/\)/igm, '');
      stringSplite2 = stringST.split(',');
      for (j = 0; j < stringSplite2.length; j++) {
        stringSplite3 = stringSplite2[j].trim().split(' ');
        geomJson['points'][j] = [Number(stringSplite3[0].trim()), Number(stringSplite3[1].trim())];
      }
    } else if (stringST.indexOf('LINESTRING') == 0) {
      geomJson['paths'] = new Array();
      stringST = stringST.replace('LINESTRING', '');
      stringST = stringST.trim();
      stringSplite = stringST.split(/\(*?\)\,/);
      for (i = 0; i < stringSplite.length; i++) {
        geomJson['paths'][i] = new Array();
        stringST = stringSplite[i].replace(/\(/igm, '').replace(/\)/igm, '');
        stringSplite2 = stringST.split(',');
        for (j = 0; j < stringSplite2.length; j++) {
          stringSplite3 = stringSplite2[j].trim().split(' ');
          geomJson['paths'][i].push([Number(stringSplite3[0].trim()), Number(stringSplite3[1].trim())]);
        }
      }
    } else if (stringST.indexOf('MULTILINESTRING') == 0) {
      geomJson['paths'] = new Array();
      stringST = stringST.replace('MULTILINESTRING', '');
      stringST = stringST.trim();
      stringSplite = stringST.split(/\(*?\)\,/);
      for (i = 0; i < stringSplite.length; i++) {
        geomJson['paths'][i] = new Array();
        stringST = stringSplite[i].replace(/\(/igm, '').replace(/\)/igm, '');
        stringSplite2 = stringST.split(',');
        for (j = 0; j < stringSplite2.length; j++) {
          stringSplite3 = stringSplite2[j].trim().split(' ');
          geomJson['paths'][i].push([Number(stringSplite3[0].trim()), Number(stringSplite3[1].trim())]);
        }
      }
    } else if (stringST.indexOf('POLYGON') == 0) {
      geomJson['rings'] = new Array();
      stringST = stringST.replace('POLYGON', '');
      stringST = stringST.trim();
      stringSplite = stringST.split(/\(*?\)\,/);
      for (i = 0; i < stringSplite.length; i++) {
        geomJson['rings'][i] = new Array();
        stringST = stringSplite[i].replace(/\(/igm, '').replace(/\)/igm, '');
        stringSplite2 = stringST.split(',');
        for (j = 0; j < stringSplite2.length; j++) {
          stringSplite3 = stringSplite2[j].trim().split(' ');
          geomJson['rings'][i].push([Number(stringSplite3[0].trim()), Number(stringSplite3[1].trim())]);
        }
      }
    } else if (stringST.indexOf('MULTIPOLYGON') == 0) {
      geomJson['rings'] = new Array();
      stringST = stringST.replace('MULTIPOLYGON', '');
      stringST = stringST.trim();
      stringSplite = stringST.split(/\(*?\)\,/);
      for (i = 0; i < stringSplite.length; i++) {
        geomJson['rings'][i] = new Array();
        stringST = stringSplite[i].replace(/\(/igm, '').replace(/\)/igm, '');
        stringSplite2 = stringST.split(',');
        for (j = 0; j < stringSplite2.length; j++) {
          stringSplite3 = stringSplite2[j].trim().split(' ');
          geomJson['rings'][i].push([Number(stringSplite3[0].trim()), Number(stringSplite3[1].trim())]);
        }
      }
    }
    geomJson['spatialReference'] = { 'wkid': this._dbProjection };

    return this.jsonUtils.fromJson(geomJson);
  }

  /**
   * Convert from geometry(esri.geometry.Geometry) json to st_geometry string.
   */
  public toST(geometry: any): string {
    let stringST: string = '';
    if (geometry.x && geometry.y) {
      stringST = `POINT(${geometry.x} ${geometry.y})`;
    } else if (geometry.points) {
      stringST = 'MULTIPOINT({0})'.replace('{0}', geometry.points.map(r => r.join(' ')).join(','));
    } else if (geometry.paths) {
      if (geometry.paths.length == 1) {
        stringST = 'LINESTRING{0}';
      } else if (geometry.paths.length > 1) {
        stringST = 'MULTILINESTRING({0})';
      }
      stringST = stringST.replace('{0}', geometry.paths.map(r => {
        return '({0})'.replace('{0}', r.map(p => {
          return p.join(' ');
        }).join(','));
      }).join(','));
    } else if (geometry.rings) {
      if (geometry.rings.length == 1) {
        stringST = 'POLYGON{0}';
      } else if (geometry.rings.length > 1) {
        stringST = 'MULTIPOLYGON({0})';
      }
      stringST = stringST.replace('{0}', geometry.rings.map(r => {
        return '(({0}))'.replace('{0}', r.map(p => {
          return p.join(' ');
        }).join(','));
      }).join(','));
    } else if (geometry.xmin && geometry.ymin) {
      stringST = 'POLYGON(({xmin} {ymax},{xmax} {ymax},{xmax} {ymin},{xmin} {ymin},{xmin} {ymax}))';
      stringST = stringST.replace(/{xmin}/g, geometry.xmin);
      stringST = stringST.replace(/{xmax}/g, geometry.xmax);
      stringST = stringST.replace(/{ymin}/g, geometry.ymin);
      stringST = stringST.replace(/{ymax}/g, geometry.ymax);
    }

    return stringST;
  }

}
