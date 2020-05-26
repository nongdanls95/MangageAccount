import { Injectable } from '@angular/core';
import { loadModules } from 'esri-loader';

import { loaderOptions } from '@gis/config/loader-options';

@Injectable()
export class ProjectionUtilService {
  private webMercatorUtils;
  private jsonUtil;
  private SpatialReference;
  private array;
  private lang;

  /********************************* Constants ********************************/
  private readonly FOURTHPI = Math.PI / 4;
  private readonly DEG_2_RAD = Math.PI / 180;
  private readonly RAD_2_DEG = 180.0 / Math.PI;
  private readonly BLOCK_SIZE = 100000; // size of square identifier (within grid zone designation),
  // (meters)

  // For diagram of zone sets, please see the 'United States National Grid' white paper.
  private readonly GRIDSQUARE_SET_COL_SIZE = 8;  // column width of grid square set  
  private readonly GRIDSQUARE_SET_ROW_SIZE = 20; // row height of grid square set

  // UTM offsets
  private readonly EASTING_OFFSET = 500000.0;   // (meters)
  private readonly NORTHING_OFFSET = 10000000.0; // (meters)

  // scale factor of central meridian
  private readonly k0 = 0.9996;

  constructor() {
    loadModules([
      'esri/geometry/webMercatorUtils',
      'esri/geometry/jsonUtils'
    ], loaderOptions).then(([
      webMercatorUtils,
      jsonUtil
    ]) => {
      this.webMercatorUtils = webMercatorUtils;
      this.jsonUtil = jsonUtil;
    });
  }

  /**
   * แปลงระบบพิกัดฉากแผนที่จาก GCS เป็น UTM (`WGS 84 / UTM zone 47N`)
   * @param geometry Geometry ที่ถูกสร้างจากคลาส esri/geometry/Geometry เช่น Point Polyline Polygon เป็นต้น
   * @param zone ระบุว่าตำแหน่งของ geometry ที่ส่งเข้ามาอยู่โซนอะไร โดยค่าเริ่มต้นคือ 47
   */
  public toUTM(geometry: any, zone: number = 47) {
    let spatialReference = null;
    try {
      geometry = typeof (geometry.toJson) == 'function' ? this.jsonUtil.fromJson(geometry.toJson()) : this.jsonUtil.fromJson(geometry);
    } catch (err) {
      throw new Error('geometry is not correct.');
    }
    if (geometry.spatialReference && geometry.spatialReference.wkid) {
      spatialReference = geometry.spatialReference;
      if (spatialReference.isWebMercator()) {
        geometry = this.webMercatorUtils.webMercatorToGeographic(geometry);
      }
    } else {
      throw new Error('not found spatialReference in geometry.');
    }
    let outSR: number = this._getUTMSpatialreference(spatialReference.wkid, zone);
    let datumConstans: any = this._getDatumConstants(outSR);
    // let SEMI_MAJOR_AXIS = datumConstans.SEMI_MAJOR_AXIS;
    let INVERSE_FLATTENING: number = datumConstans.INVERSE_FLATTENING;
    let ns: string = datumConstans.ns;
    let jsonGeometry = {};
    if (geometry.x) {
      const points: number[] = this._lltoUTM(geometry.y, geometry.x, datumConstans);
      jsonGeometry = { x: points[0], y: points[1] };
    } else if (geometry.points) {
      jsonGeometry = {
        points: geometry.points.map(point => {
          const _point: number[] = this._lltoUTM(point[1], point[0], datumConstans);
          return [_point[0], _point[1]];
        })
      }
    } else if (geometry.paths) {
      jsonGeometry = {
        paths: geometry.paths.map(path => {
          return path.map(point => {
            const _point: number[] = this._lltoUTM(point[1], point[0], datumConstans);
            return [_point[0], _point[1]];
          });
        })
      };
    } else if (geometry.rings) {
      jsonGeometry = {
        rings: geometry.rings.map(ring => {
          return ring.map(point => {
            const _point: number[] = this._lltoUTM(point[1], point[0], datumConstans);
            return [_point[0], _point[1]];
          });
        })
      };
    } else if (geometry.xmax) {
      const points1: number[] = this._lltoUTM(geometry.ymax, geometry.xmax, datumConstans);
      const points2: number[] = this._lltoUTM(geometry.ymin, geometry.xmin, datumConstans);
      jsonGeometry['xmax'] = points1[0];
      jsonGeometry['ymax'] = points1[1];
      jsonGeometry['xmin'] = points2[0];
      jsonGeometry['ymin'] = points2[1];
    }
    jsonGeometry['spatialReference'] = { wkid: outSR };

    return this.jsonUtil.fromJson(jsonGeometry);
  }

  /**
   * แปลงระบบพิกัดฉากแผนที่จาก UTM เป็น GCS
   * @param geometry Geometry ของระบบพิกัดฉาก UTM ที่ถูกสร้างจากคลาส esri/geometry/Geometry เช่น Point Polyline Polygon เป็นต้น
   */
  public fromUTM(geometry: any) {
    let spatialReference = null;
    if (geometry.spatialReference && geometry.spatialReference.wkid) {
      spatialReference = geometry.spatialReference;
    } else {
      throw new Error('not found spatialReference in geometry');
    }
    let outSR: number = this._getGCSSpatialreference(spatialReference.wkid);
    let datumConstans: any = this._getDatumConstants(spatialReference.wkid);
    // let SEMI_MAJOR_AXIS = datumConstans.SEMI_MAJOR_AXIS;
    let INVERSE_FLATTENING: number = datumConstans.INVERSE_FLATTENING;
    let jsonGeometry = {};
    if (geometry.x) {
      let points: number[] = this._UTMtoLL(geometry.y, geometry.x, datumConstans);
      jsonGeometry = { x: points[0], y: points[1] };
    } else if (geometry.points) {
      jsonGeometry = {
        points: geometry.points.map(point => {
          let _point: number[] = this._UTMtoLL(point[1], point[0], datumConstans);
          return [_point[0], _point[1]];
        })
      }
    } else if (geometry.paths) {
      jsonGeometry = {
        paths: geometry.paths.map(path => {
          return path.map(point => {
            let _point: number[] = this._UTMtoLL(point[1], point[0], datumConstans);
            return [_point[0], _point[1]];
          })
        })
      };
    } else if (geometry.rings) {
      jsonGeometry = {
        rings: geometry.rings.map(ring => {
          return ring.map(point => {
            let _point: number[] = this._UTMtoLL(point[1], point[0], datumConstans);
            return [_point[0], _point[1]];
          })
        })
      };
    } else if (geometry.xmax) {
      let points1: number[] = this._UTMtoLL(geometry.ymax, geometry.xmax, datumConstans);
      let points2: number[] = this._UTMtoLL(geometry.ymin, geometry.xmin, datumConstans);
      jsonGeometry['xmax'] = points1[1];
      jsonGeometry['ymax'] = points1[0];
      jsonGeometry['xmin'] = points2[1];
      jsonGeometry['ymin'] = points2[0];
    }
    jsonGeometry['spatialReference'] = { wkid: outSR };

    return this.jsonUtil.fromJson(jsonGeometry);
  }

  /**
   * แปลงระบบพิกัดฉากแผนที่จาก esri/geometry/Geometry เป็น MGRS
   * @param geometry Geometry ที่ถูกสร้างจากคลาส esri/geometry/Geometry เช่น Point Polyline Polygon เป็นต้น
   * @param precision จำนวนหลักตของเลข x และ y โดยค่าเริ่มต้นคือ 5
   * ตัวอย่าง 47QPU0167242553 โดยกำหนด precision = 5 
   * ดังนั้น x = 01672; y = 42553;
   */
  public toMGRS(geometry: any, precision: number = 5): string {
    geometry = this.transform(geometry, 4326);

    let lat: number = geometry.y;
    let lon: number = geometry.x;

    let UTMZoneNumber: number = this._getZoneNumber(lat, lon);
    let datumConstants = this._getDatumConstants(4326);
    datumConstants.zone = UTMZoneNumber;

    let coords: number[] = this._lltoUTM(lat, lon, datumConstants);

    let UTMEasting: number = coords[0];
    let UTMNorthing: number = coords[1];

    //console.log(UTMEasting, UTMNorthing);

    // ...then convert UTM to USNG

    // southern hemispher case
    if (lat < 0) {
      // Use offset for southern hemisphere
      UTMNorthing += this.NORTHING_OFFSET;
    }

    let USNGLetters: string = this._findGridLetters(UTMZoneNumber, UTMNorthing, UTMEasting);
    let USNGNorthing: number = Math.round(UTMNorthing) % this.BLOCK_SIZE;
    let USNGEasting: number = Math.round(UTMEasting) % this.BLOCK_SIZE;

    // added... truncate digits to achieve specified precision
    USNGNorthing = Math.floor(USNGNorthing / Math.pow(10, (5 - precision)))
    USNGEasting = Math.floor(USNGEasting / Math.pow(10, (5 - precision)))

    let USNG: string = UTMZoneNumber + this._utmLetterDesignator(lat) + ' ' + USNGLetters + ' ';

    // REVISIT: Modify to incorporate dynamic precision ?
    for (let i = String(USNGEasting).length; i < precision; i++) {
      USNG += '0';
    }

    USNG += USNGEasting + ' ';

    for (let i = String(USNGNorthing).length; i < precision; i++) {
      USNG += '0';
    }

    USNG += USNGNorthing;

    USNG = USNG.replace(/ /g, '');

    return USNG;
  }

  /**
   * แปลงระบบพิกัดฉากแผนที่จาก MGRS เป็น esri/geometry/Geometry (Decimal Degrees)
   * @param usng U.S. National Grid (USNG) Coordinates string
   */
  public fromMGRS(usng: string) {
    // latlon is a 2-element array declared by calling routine
    let usngp: any = this._parseUSNG_str(usng);
    // convert USNG coords to UTM; this routine counts digits and sets precision
    let coords: any = this._USNGtoUTM(usngp.zone, usngp.let, usngp.sq1, usngp.sq2, usngp.east, usngp.north);

    // southern hemisphere case
    if (usngp.let < 'N') {
      coords.N -= this.NORTHING_OFFSET;
    }
    let datumConstants = this._getDatumConstants(4326);
    datumConstants.zone = coords.zone;

    coords = this._UTMtoLL(coords.N, coords.E, datumConstants);

    return this.jsonUtil.fromJson({ x: coords[0], y: coords[1], spatialReference: { wkid: 4326 } });

  }

  /**
   * แปลง esri/geometry/Point แบบ decimal degree เป็น DMS
   * @param point Geometry ประเภท esri/geometry/Point แบบ latitude longitude (wkid=4326)
   */
  public toDMS(point: any) {
    let dms: any = {};
    let pointDD = this.transform(point, 4326);

    if (point.x && point.y) {
      dms.x = this._ddToDMS(point.x);
      dms.x.direction = Math.sign(point.x) >= 0 ? 'E' : 'W';
      dms.y = this._ddToDMS(point.y);
      dms.y.direction = Math.sign(point.y) >= 0 ? 'N' : 'S';
    }
    return dms;
  }

  /**
   * แปลง DMS แบบ decimal degree เป็น esri/geometry/Point แบบ decimal degree
   * @param dms ระบบพิกัดแบบ DMS(degrees minutes seconds)
   */
  public fromDMS(dms: any) {
    return this.jsonUtil.fromJson({
      x: this._dmsToDD(dms.x.degrees, dms.x.minutes, dms.x.seconds, dms.x.direction),
      y: this._dmsToDD(dms.y.degrees, dms.y.minutes, dms.y.seconds, dms.y.direction),
      spatialReference: { wkid: 4326 }
    });
  }

  /**
   * แปลงระบบพิกัดฉากแผนที่ โดยอ้างอิงจาก spatialreference(wkid)
   * @param geometry Geometry ที่ถูกสร้างจากคลาส esri/geometry/Geometry เช่น Point Polyline Polygon เป็นต้น
   * @param outSR เลขอ้างอิงระบบพิกัดฉากแผนที่ (wkid)
   */
  public transform(geometry: any, outSR: number) {
    let datumConstant = this._getDatumConstants(outSR);
    let spatialReference = null;
    let inSR: number = 0;
    try {
      geometry = typeof (geometry.toJson) == 'function' ? this.jsonUtil.fromJson(geometry.toJson()) : this.jsonUtil.fromJson(geometry);
    } catch (err) { }
    if (geometry.spatialReference && geometry.spatialReference.wkid) {
      spatialReference = geometry.spatialReference;
      inSR = spatialReference.wkid;
    } else {
      throw new Error('not found spatialReference in geometry');
    }
    if (inSR != outSR) {
      if (spatialReference.isWebMercator()) {
        geometry = this.webMercatorUtils.webMercatorToGeographic(geometry);
      } else if (!this._isGeographic(spatialReference.wkid)) {
        geometry = this.fromUTM(geometry);
      }
      let jsonGeometry = {};
      if (this._getGCSSpatialreference(outSR) != this._getGCSSpatialreference(inSR)) {
        if (geometry.x) {
          let points = this._toCartecian(geometry.x, geometry.y, inSR, outSR);
          jsonGeometry = { x: points[0], y: points[1] };
        } else if (geometry.points) {
          jsonGeometry = {
            points: geometry.points.map(point => this._toCartecian(point[1], point[0], inSR, outSR))
          }
        } else if (geometry.paths) {
          jsonGeometry = {
            paths: geometry.paths.map(path => {
              return path.map(point => this._toCartecian(point[1], point[0], inSR, outSR))
            })
          };
        } else if (geometry.rings) {
          jsonGeometry = {
            rings: geometry.ring.map(ring => {
              return ring.map(point => this._toCartecian(point[1], point[0], inSR, outSR))
            })
          };
        } else if (geometry.xmax) {
          let points1 = this._toCartecian(geometry.ymax, geometry.xmax, inSR, outSR);
          let points2 = this._toCartecian(geometry.ymin, geometry.xmin, inSR, outSR);
          jsonGeometry['xmax'] = points1[1];
          jsonGeometry['ymax'] = points1[0];
          jsonGeometry['xmin'] = points2[1];
          jsonGeometry['ymin'] = points2[0];
        }
        jsonGeometry['spatialReference'] = { wkid: this._getGCSSpatialreference(outSR) };
        jsonGeometry = this.jsonUtil.fromJson(jsonGeometry);
      } else {
        jsonGeometry = geometry;
      }

      if (this._isWebMercator(outSR)) {
        jsonGeometry = this.webMercatorUtils.geographicToWebMercator(jsonGeometry);
      } else if (!this._isGeographic(outSR)) {
        jsonGeometry = this.toUTM(jsonGeometry, datumConstant.zone);
      }
      return jsonGeometry;
    } else {
      return geometry;
    }
  }


  private _getUTMSpatialreference(sr: number, zone: number): number {
    if (sr >= 32600 || sr == 4326 || sr == 102100 || sr == 3857) {
      return 32600 + zone;
    } else if (sr >= 24000 || sr == 4240) {
      return 24000 + zone;
    } else {
      throw new Error('this "srid" is not support');
    }
  }

  private _getDatumConstants(sr: number) {
    let EQUATORIAL_RADIUS: number = 0;
    let INVERSE_FLATTENING: number = 0;
    let ECC_SQUARED: number = 0;
    let ns: string = 'N';
    let zone: number = 47;
    if (sr >= 32600 || sr == 4326 || sr == 102100 || sr == 3857) {
      EQUATORIAL_RADIUS = 6378137.0;
      INVERSE_FLATTENING = 298.25722356300003;
      let f: number = 1.0 / INVERSE_FLATTENING;
      ECC_SQUARED = (2.0 * f) - (f * f);
      if (sr >= 32700 && sr < 32800) {
        zone = sr - 32700;
        ns = 'S'
      } else if (sr >= 32600 && sr < 32700) {
        zone = sr - 32600;
        ns = 'N'
      } else {
        zone = null;
        ns = null;
      }
    } else if (sr >= 24000 || sr == 4240) {
      EQUATORIAL_RADIUS = 6377276.3449999997;
      INVERSE_FLATTENING = 300.80169999999998;
      let f: number = 1.0 / INVERSE_FLATTENING;
      ECC_SQUARED = (2.0 * f) - (f * f);
      if (sr >= 24000 && sr < 24100) {
        zone = sr - 24000;
        ns = 'N'
      } else {
        zone = null;
        ns = null;
        //throw new Error('invalid 'srid': ' + sr);
      }
    } else {
      throw new Error('this "srid" is not support');
    }

    let ECC_PRIME_SQUARED: number = ECC_SQUARED / (1 - ECC_SQUARED);
    let E1: number = (1 - Math.sqrt(1 - ECC_SQUARED)) / (1 + Math.sqrt(1 - ECC_SQUARED));

    return {
      EQUATORIAL_RADIUS: EQUATORIAL_RADIUS,
      INVERSE_FLATTENING: INVERSE_FLATTENING,
      ECC_SQUARED: ECC_SQUARED,
      ECC_PRIME_SQUARED: ECC_PRIME_SQUARED,
      E1: E1,
      zone: zone,
      ns: ns
    };
  }

  private _lltoUTM(lat: number, lon: number, cnts: any): number[] {
    let utmcoords: number[] = [];
    // utmcoords is a 2-D array declared by the calling routine

    // lat = parseFloat(lat);
    // lon = parseFloat(lon);

    // Constrain reporting USNG coords to the latitude range [80S .. 84N]
    /////////////////
    if (lat > 84.0 || lat < -80.0) {
      //return (UNDEFINED_STR);
      throw new Error('Bad input');
    }
    //////////////////////

    // sanity check on input - turned off when testing with Generic Viewer
    /////////////////////  /*
    if (lon > 360 || lon < -180 || lat > 90 || lat < -90) {
      //alert('Bad input. lat: ' + lat + ' lon: ' + lon);
      //return null;
      throw new Error('Bad input');
    }
    //////////////////////  */

    // Make sure the longitude is between -180.00 .. 179.99..
    // Convert values on 0-360 range to this range.
    let lonTemp: number = (lon + 180) - Math.floor((lon + 180) / 360) * 360 - 180;
    let latRad: number = lat * this.DEG_2_RAD;
    let lonRad: number = lonTemp * this.DEG_2_RAD;

    // user-supplied zone number will force coordinates to be computed in a particular zone
    let zoneNumber: number;
    if (!cnts.zone) {
      zoneNumber = this._getZoneNumber(lat, lon);
    }
    else {
      zoneNumber = cnts.zone
    }

    let lonOrigin: number = (zoneNumber - 1) * 6 - 180 + 3;  // +3 puts origin in middle of zone
    let lonOriginRad: number = lonOrigin * this.DEG_2_RAD;

    // compute the UTM Zone from the latitude and longitude
    let UTMZone: string = zoneNumber + '' + this._utmLetterDesignator(lat) + ' ';

    let N: number = cnts.EQUATORIAL_RADIUS / Math.sqrt(1 - cnts.ECC_SQUARED * Math.sin(latRad) * Math.sin(latRad));
    let T: number = Math.tan(latRad) * Math.tan(latRad);
    let C: number = cnts.ECC_PRIME_SQUARED * Math.cos(latRad) * Math.cos(latRad);
    let A: number = Math.cos(latRad) * (lonRad - lonOriginRad);

    // Note that the term Mo drops out of the 'M' equation, because phi 
    // (latitude crossing the central meridian, lambda0, at the origin of the
    //  x,y coordinates), is equal to zero for UTM.
    let M: number = cnts.EQUATORIAL_RADIUS * ((1 - cnts.ECC_SQUARED / 4
      - 3 * (cnts.ECC_SQUARED * cnts.ECC_SQUARED) / 64
      - 5 * (cnts.ECC_SQUARED * cnts.ECC_SQUARED * cnts.ECC_SQUARED) / 256) * latRad
      - (3 * cnts.ECC_SQUARED / 8 + 3 * cnts.ECC_SQUARED * cnts.ECC_SQUARED / 32
        + 45 * cnts.ECC_SQUARED * cnts.ECC_SQUARED * cnts.ECC_SQUARED / 1024)
      * Math.sin(2 * latRad) + (15 * cnts.ECC_SQUARED * cnts.ECC_SQUARED / 256
        + 45 * cnts.ECC_SQUARED * cnts.ECC_SQUARED * cnts.ECC_SQUARED / 1024) * Math.sin(4 * latRad)
      - (35 * cnts.ECC_SQUARED * cnts.ECC_SQUARED * cnts.ECC_SQUARED / 3072) * Math.sin(6 * latRad));

    let UTMEasting: number = (this.k0 * N * (A + (1 - T + C) * (A * A * A) / 6
      + (5 - 18 * T + T * T + 72 * C - 58 * cnts.ECC_PRIME_SQUARED)
      * (A * A * A * A * A) / 120)
      + this.EASTING_OFFSET);

    let UTMNorthing: number = (this.k0 * (M + N * Math.tan(latRad) * ((A * A) / 2 + (5 - T + 9
      * C + 4 * C * C) * (A * A * A * A) / 24
      + (61 - 58 * T + T * T + 600 * C - 330 * cnts.ECC_PRIME_SQUARED)
      * (A * A * A * A * A * A) / 720)));

    // added by LRM 2/08...not entirely sure this doesn't just move a bug somewhere else
    // utm values in southern hemisphere
    //  if (UTMNorthing < 0) {
    //	UTMNorthing += NORTHING_OFFSET;
    //  }

    utmcoords[0] = UTMEasting
    utmcoords[1] = UTMNorthing
    utmcoords[2] = zoneNumber

    return utmcoords;
  }

  private _getZoneNumber(lat: number, lon: number): number {
    // sanity check on input
    ////////////////////////////////   /*
    if (lon > 360 || lon < -180 || lat > 90 || lat < -90) {
      //alert('Bad input. lat: ' + lat + ' lon: ' + lon);
      //return null;
      throw new Error('Bad input');
    }
    ////////////////////////////////  */

    // convert 0-360 to [-180 to 180] range
    let lonTemp: number = (lon + 180) - Math.floor((lon + 180) / 360) * 360 - 180;
    let zoneNumber: number = Math.floor((lonTemp + 180) / 6) + 1;

    // Handle special case of west coast of Norway
    if (lat >= 56.0 && lat < 64.0 && lonTemp >= 3.0 && lonTemp < 12.0) {
      zoneNumber = 32;
    }

    // Special zones for Svalbard
    if (lat >= 72.0 && lat < 84.0) {
      if (lonTemp >= 0.0 && lonTemp < 9.0) {
        zoneNumber = 31;
      }
      else if (lonTemp >= 9.0 && lonTemp < 21.0) {
        zoneNumber = 33;
      }
      else if (lonTemp >= 21.0 && lonTemp < 33.0) {
        zoneNumber = 35;
      }
      else if (lonTemp >= 33.0 && lonTemp < 42.0) {
        zoneNumber = 37;
      }
    }
    return zoneNumber;
  }

  private _utmLetterDesignator(lat: number): string {
    let letterDesignator: string = '';
    // lat = parseFloat(lat);

    if ((84 >= lat) && (lat >= 72))
      letterDesignator = 'X';
    else if ((72 > lat) && (lat >= 64))
      letterDesignator = 'W';
    else if ((64 > lat) && (lat >= 56))
      letterDesignator = 'V';
    else if ((56 > lat) && (lat >= 48))
      letterDesignator = 'U';
    else if ((48 > lat) && (lat >= 40))
      letterDesignator = 'T';
    else if ((40 > lat) && (lat >= 32))
      letterDesignator = 'S';
    else if ((32 > lat) && (lat >= 24))
      letterDesignator = 'R';
    else if ((24 > lat) && (lat >= 16))
      letterDesignator = 'Q';
    else if ((16 > lat) && (lat >= 8))
      letterDesignator = 'P';
    else if ((8 > lat) && (lat >= 0))
      letterDesignator = 'N';
    else if ((0 > lat) && (lat >= -8))
      letterDesignator = 'M';
    else if ((-8 > lat) && (lat >= -16))
      letterDesignator = 'L';
    else if ((-16 > lat) && (lat >= -24))
      letterDesignator = 'K';
    else if ((-24 > lat) && (lat >= -32))
      letterDesignator = 'J';
    else if ((-32 > lat) && (lat >= -40))
      letterDesignator = 'H';
    else if ((-40 > lat) && (lat >= -48))
      letterDesignator = 'G';
    else if ((-48 > lat) && (lat >= -56))
      letterDesignator = 'F';
    else if ((-56 > lat) && (lat >= -64))
      letterDesignator = 'E';
    else if ((-64 > lat) && (lat >= -72))
      letterDesignator = 'D';
    else if ((-72 > lat) && (lat >= -80))
      letterDesignator = 'C';
    else
      letterDesignator = 'Z'; // This is here as an error flag to show 
    // that the latitude is outside the UTM limits
    return letterDesignator;
  }

  private _getGCSSpatialreference(sr: number): number {
    if (sr >= 32600 || sr == 4326 || sr == 102100 || sr == 3857) {
      return 4326;
    } else if (sr >= 24000 || sr == 4240) {
      return 4240;
    } else {
      throw new Error('this "srid" is not support');
    }
  }

  private _UTMtoLL(UTMNorthing: number, UTMEasting: number, cnts: any): number[] {
    // remove 500,000 meter offset for longitude
    let xUTM: number = UTMEasting - this.EASTING_OFFSET;
    let yUTM: number = UTMNorthing;
    let zoneNumber: number = cnts.zone;

    // origin longitude for the zone (+3 puts origin in zone center) 
    let lonOrigin: number = (zoneNumber - 1) * 6 - 180 + 3;

    // M is the 'true distance along the central meridian from the Equator to phi
    // (latitude)
    let M: number = yUTM / this.k0;
    let mu: number = M / (cnts.EQUATORIAL_RADIUS * (1 - cnts.ECC_SQUARED / 4 - 3 * cnts.ECC_SQUARED *
      cnts.ECC_SQUARED / 64 - 5 * cnts.ECC_SQUARED * cnts.ECC_SQUARED * cnts.ECC_SQUARED / 256));

    // phi1 is the 'footprint latitude' or the latitude at the central meridian which
    // has the same y coordinate as that of the point (phi (lat), lambda (lon) ).
    let phi1Rad: number = mu + (3 * cnts.E1 / 2 - 27 * cnts.E1 * cnts.E1 * cnts.E1 / 32) * Math.sin(2 * mu)
      + (21 * cnts.E1 * cnts.E1 / 16 - 55 * cnts.E1 * cnts.E1 * cnts.E1 * cnts.E1 / 32) * Math.sin(4 * mu)
      + (151 * cnts.E1 * cnts.E1 * cnts.E1 / 96) * Math.sin(6 * mu);
    let phi1: number = phi1Rad * this.RAD_2_DEG;

    // Terms used in the conversion equations
    let N1: number = cnts.EQUATORIAL_RADIUS / Math.sqrt(1 - cnts.ECC_SQUARED * Math.sin(phi1Rad) * Math.sin(phi1Rad));
    let T1: number = Math.tan(phi1Rad) * Math.tan(phi1Rad);
    let C1: number = cnts.ECC_PRIME_SQUARED * Math.cos(phi1Rad) * Math.cos(phi1Rad);
    let R1: number = cnts.EQUATORIAL_RADIUS * (1 - cnts.ECC_SQUARED) / Math.pow(1 - cnts.ECC_SQUARED * Math.sin(phi1Rad) * Math.sin(phi1Rad), 1.5);
    let D: number = xUTM / (N1 * this.k0);

    // Calculate latitude, in decimal degrees
    let lat: number = phi1Rad - (N1 * Math.tan(phi1Rad) / R1) * (D * D / 2 - (5 + 3 * T1 + 10
      * C1 - 4 * C1 * C1 - 9 * cnts.ECC_PRIME_SQUARED) * D * D * D * D / 24 + (61 + 90 *
        T1 + 298 * C1 + 45 * T1 * T1 - 252 * cnts.ECC_PRIME_SQUARED - 3 * C1 * C1) * D * D *
      D * D * D * D / 720);
    lat = lat * this.RAD_2_DEG;

    // Calculate longitude, in decimal degrees
    let lon: number = (D - (1 + 2 * T1 + C1) * D * D * D / 6 + (5 - 2 * C1 + 28 * T1 - 3 *
      C1 * C1 + 8 * cnts.ECC_PRIME_SQUARED + 24 * T1 * T1) * D * D * D * D * D / 120) /
      Math.cos(phi1Rad);

    lon = lonOrigin + lon * this.RAD_2_DEG;
    //ret.lat = lat;
    //ret.lon = lon;
    return [lon, lat];
  }

  private _isGeographic(srid: number): boolean {
    if (srid >= 32600) {
      return false;
    } else if (srid >= 24000) {
      return false;
    }
    return true;
  }

  private _toCartecian(lon: number, lat: number, inSR: number, outSR: number): number[] {
    let inDatumConstants = this._getDatumConstants(inSR);
    let outDatumConstants = this._getDatumConstants(outSR);
    let transformConstants = this._getTransfromConstants(inSR, outSR);
    //console.log(inDatumConstants, outDatumConstants, transformConstants);
    let f: number = 1.0 / inDatumConstants.INVERSE_FLATTENING;
    let e2: number = (2.0 * f) - (f * f);

    let rLat: number = lat * Math.PI / 180.0;
    let rLon: number = lon * Math.PI / 180.0;
    let sinLat: number = Math.sin(rLat);
    let cosLat: number = Math.cos(rLat);
    let v: number = inDatumConstants.EQUATORIAL_RADIUS / Math.sqrt(1.0 - (e2 * sinLat * sinLat));
    let h: number = 0;
    let x: number = (v + h) * (cosLat * Math.cos(rLon));
    let y: number = (v + h) * cosLat * Math.sin(rLon);
    let z: number = ((1 - e2) * v + h) * sinLat;

    let x2: number = x + transformConstants.x;
    let y2: number = y + transformConstants.y;
    let z2: number = z + transformConstants.z;


    f = 1.0 / outDatumConstants.INVERSE_FLATTENING;
    e2 = (2.0 * f) - (f * f);

    let p: number = Math.sqrt((x2 * x2) + (y2 * y2));
    let r: number = Math.sqrt((p * p) + (z2 * z2));
    let u: number = Math.atan((z2 / p) * ((1.0 - f) + (e2 * outDatumConstants.EQUATORIAL_RADIUS) / r));
    let sinU: number = Math.sin(u);
    let cosU: number = Math.cos(u);
    let lattopLine: number = z2 * (1.0 - f) + e2 * outDatumConstants.EQUATORIAL_RADIUS * sinU * sinU * sinU;
    let latBottomLine: number = (1.0 - f) * (p - e2 * outDatumConstants.EQUATORIAL_RADIUS * cosU * cosU * cosU);
    rLon = Math.atan(y2 / x2);
    rLat = Math.atan(lattopLine / latBottomLine);
    rLon = rLon < 0 ? Math.PI + rLon : rLon;
    let lonNew: number = rLon * 180.0 / Math.PI;
    let latNew: number = rLat * 180.0 / Math.PI;

    return [lonNew, latNew];
  }

  private _getTransfromConstants(inSR: number, outSR: number) {
    let x: number, y: number, z: number;
    if (inSR >= 32600 || inSR == 4326 || inSR == 102100 || inSR == 3857) {
      if (outSR >= 24000 || outSR == 4240) {
        x = -209.0;
        y = -818.0;
        z = -290.0;
      }
    } else if (inSR >= 24000 || inSR == 4240) {
      if (outSR >= 32600 || outSR == 4326 || outSR == 102100 || outSR == 3857) {
        x = 209.0;
        y = 818.0;
        z = 290.0;
      }
    } else {
      throw new Error('this "srid" is not support');
    }
    return { x: x, y: y, z: z };
  }

  private _findGridLetters(zoneNum: number, northing: number, easting: number): string {

    let row: number = 1;

    // northing coordinate to single-meter precision
    let north_1m: number = Math.round(northing);

    // Get the row position for the square identifier that contains the point
    while (north_1m >= this.BLOCK_SIZE) {
      north_1m = north_1m - this.BLOCK_SIZE;
      row++;
    }

    // cycle repeats (wraps) after 20 rows
    row = row % this.GRIDSQUARE_SET_ROW_SIZE;
    let col: number = 0;

    // easting coordinate to single-meter precision
    let east_1m: number = Math.round(easting)

    // Get the column position for the square identifier that contains the point
    while (east_1m >= this.BLOCK_SIZE) {
      east_1m = east_1m - this.BLOCK_SIZE;
      col++;
    }

    // cycle repeats (wraps) after 8 columns
    col = col % this.GRIDSQUARE_SET_COL_SIZE;

    return this._lettersHelper(this._findSet(zoneNum), row, col);
  }

  private _lettersHelper(set: number, row: number, col: number): string {
    let lettersHelper: string;
    let l1: string; // column ids
    let l2: string; // row ids

    // handle case of last row
    if (row == 0) {
      row = this.GRIDSQUARE_SET_ROW_SIZE - 1;
    }
    else {
      row--;
    }

    // handle case of last column
    if (col == 0) {
      col = this.GRIDSQUARE_SET_COL_SIZE - 1;
    }
    else {
      col--;
    }

    switch (set) {
      case 1:
        l1 = 'ABCDEFGH';
        l2 = 'ABCDEFGHJKLMNPQRSTUV';
        lettersHelper = l1.charAt(col) + l2.charAt(row);
        break;
      case 2:
        l1 = 'JKLMNPQR';
        l2 = 'FGHJKLMNPQRSTUVABCDE';
        lettersHelper = l1.charAt(col) + l2.charAt(row);
        break;
      case 3:
        l1 = 'STUVWXYZ';
        l2 = 'ABCDEFGHJKLMNPQRSTUV';
        lettersHelper = l1.charAt(col) + l2.charAt(row);
        break;
      case 4:
        l1 = 'ABCDEFGH';
        l2 = 'FGHJKLMNPQRSTUVABCDE';
        lettersHelper = l1.charAt(col) + l2.charAt(row);
        break;
      case 5:
        l1 = 'JKLMNPQR';
        l2 = 'ABCDEFGHJKLMNPQRSTUV';
        lettersHelper = l1.charAt(col) + l2.charAt(row);
        break;
      case 6:
        l1 = 'STUVWXYZ';
        l2 = 'FGHJKLMNPQRSTUVABCDE';
        lettersHelper = l1.charAt(col) + l2.charAt(row);
        break;
    }

    return lettersHelper;
  }

  private _findSet(zoneNum): number {
    let setZone: number = -1;
    zoneNum = parseInt(zoneNum);
    zoneNum = zoneNum % 6;
    switch (zoneNum) {
      case 0:
        setZone = 6;
        break;
      case 1:
        setZone = 1;
        break;
      case 2:
        setZone = 2;
        break;
      case 3:
        setZone = 3;
        break;
      case 4:
        setZone = 4;
        break;
      case 5:
        setZone = 5;
        break;
      default:
        setZone = -1;
        break;
    }

    return setZone;
  }

  private _isWebMercator(srid: number): boolean {
    if (srid != 102100 && srid != 3857) {
      return false;
    }
    return true;
  }

  private _parseUSNG_str(usngStrInput: string) {
    let j: number = 0;
    let k: number;
    let usngStr: string;

    usngStrInput = usngStrInput.toUpperCase();

    // put usgn string in 'standard' form with no space delimiters
    let regexp: RegExp = /%20/g;
    usngStr = usngStrInput.replace(regexp, '');
    regexp = / /g;
    usngStr = usngStrInput.replace(regexp, '');

    if (usngStr.length < 7) {
      //alert('This application requires minimum USNG precision of 10,000 meters')
      return 0;
    }

    // break usng string into its component pieces
    let parts: any = {};
    parts.zone = Number(usngStr.charAt(j++)) * 10 + Number(usngStr.charAt(j++)) * 1;
    parts.let = usngStr.charAt(j++);
    parts.sq1 = usngStr.charAt(j++);
    parts.sq2 = usngStr.charAt(j++);

    parts.precision = (usngStr.length - j) / 2;
    parts.east = '';
    parts.north = '';
    for (k = 0; k < parts.precision; k++) {
      parts.east += usngStr.charAt(j++);
    }

    if (usngStr[j] == ' ') { j++ }
    for (k = 0; k < parts.precision; k++) {
      parts.north += usngStr.charAt(j++);
    }

    return parts;
  }

  private _USNGtoUTM(zone: number, letN: string, sq1: string, sq2: string, east: string, north: string) {
    let USNGSqEast: string = 'ABCDEFGHJKLMNPQRSTUVWXYZ'
    //Starts (southern edge) of N-S zones in millons of meters
    let zoneBase: number[] = [1.1, 2.0, 2.9, 3.8, 4.7, 5.6, 6.5, 7.3, 8.2, 9.1, 0, 0.8, 1.7, 2.6, 3.5, 4.4, 5.3, 6.2, 7.0, 7.9];

    let segBase: number[] = [0, 2, 2, 2, 4, 4, 6, 6, 8, 8, 0, 0, 0, 2, 2, 4, 4, 6, 6, 6];  //Starts of 2 million meter segments, indexed by zone 

    // convert easting to UTM
    let eSqrs: number = USNGSqEast.indexOf(sq1);
    let appxEast: number = 1 + eSqrs % 8;

    // convert northing to UTM
    let letNorth: number = 'CDEFGHJKLMNPQRSTUVWX'.indexOf(letN);
    let nSqrs: number;
    if (zone % 2)  //odd number zone
      nSqrs = 'ABCDEFGHJKLMNPQRSTUV'.indexOf(sq2)
    else        // even number zone
      nSqrs = 'FGHJKLMNPQRSTUVABCDE'.indexOf(sq2);

    let zoneStart: number = zoneBase[letNorth];
    let appxNorth: number = Number(segBase[letNorth]) + nSqrs / 10;
    if (appxNorth < zoneStart)
      appxNorth += 2;

    let ret: any = {};
    ret.N = appxNorth * 1000000 + Number(north) * Math.pow(10, 5 - north.length);
    ret.E = appxEast * 100000 + Number(east) * Math.pow(10, 5 - east.length)
    ret.zone = zone;
    ret.letter = letN;

    return ret;
  }

  private _ddToDMS(decimalDegree: number) {
    let dms: any = {};
    let minutesTemp: number;
    let minutes: number;

    if ((decimalDegree * -1) == Math.abs(decimalDegree)) {
      minutesTemp = (decimalDegree - Math.ceil(decimalDegree));
      dms.degrees = Math.ceil(decimalDegree);
    } else {
      minutesTemp = (decimalDegree - Math.floor(decimalDegree));
      dms.degrees = Math.floor(decimalDegree);
    }
    minutes = Math.abs(minutesTemp * 60);

    dms.degrees = Math.abs(dms.degrees);
    dms.minutes = Math.floor(minutes);
    dms.seconds = (minutes - Math.floor(minutes)) * 60;

    return dms;
  }

  private _dmsToDD(degrees: number, minutes: number, seconds: number, direction: string): number {
    let decimalDegree = degrees + (minutes / 60) + (seconds / (60 * 60));
    if (direction == 'S' || direction == 'W') {
      decimalDegree = decimalDegree * -1;
    }
    return decimalDegree;
  }

}
