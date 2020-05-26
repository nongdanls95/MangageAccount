import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, take } from 'rxjs/operators';

import { User } from '@app-base/interface/user';
import { AppConfig } from '@app-base/interface/app-config';

@Injectable()
export class AppService {
  user: User = null;
  appConfig: AppConfig = null;

  constructor(private http: HttpClient) { }

  /**
   * Get application host name.
   *
   * `const host = this.app.hostName;`
   */
  get hostName(): string {
    return document.getElementsByTagName('base')[0].href;
  }

  /**
   * Request web service with url
   * 
   * `const req: Observable<any> = this.appServicce.reqUrl('http://example.com/api/..');`
   * 
   * @param url Url for web service
   * @param params (Optional) Parameters with JSON data format
   */
  reqUrl(url: string, params: any = {}): Observable<any> {
    return this._reqUrl(url, params);
  }

  /**
   * Request stored procedure
   * 
   * `const req: Observable<any> = this.appServicce.reqSP('USER_Q', {...});`
   * 
   * @param storeName Procedure name
   * @param params (Optional) Parameters with JSON data format
   */
  reqSP(storeName: string, params: any = {}): Observable<any> {
    let paramsToSend = this.softCopyJSON(params);
    paramsToSend['APP_DATA_PROCEDURE'] = storeName;

    return this._reqUrl(`${this.hostName}/Handlers/AppData.ashx`, paramsToSend).pipe(take(1));
  }


  /**
   * Get cookie value with name reference
   * 
   * `const csrf: string = this.app.getCookie('CSRF_TOKEN');`
   * 
   * @param name ชื่อของ cookie
   */
  getCookie(name): string {
    let value = '; ' + document.cookie;
    let parts = value.split('; ' + name + '=');
    if (parts.length == 2) return parts.pop().split(';').shift();
  }

  /**
   * Soft copy JSON data
   * 
   * `let myJson: object = this.app.softCopyJSON({...});`
   * 
   * @param data JSON data format
   */
  softCopyJSON(data: any): object {
    let cloned: object = {};
    for (let key in data)
      cloned[key] = data[key];

    return cloned;
  }

  /**
   * Get domain name from url
   * 
   * `const domain: string = this.app.getDomainName('http://example.com/path/to/service');`
   * 
   * @param url Url
   */
  getDomainName(url: string): string {
    let domainNameStartIndex = url.indexOf('//');
    let domainName = '';

    if (domainNameStartIndex >= 0)
      domainName = url.substring(domainNameStartIndex + 2);
    else
      domainName = url;

    let domainNameEndIndex = domainName.indexOf('/');

    if (domainNameEndIndex >= 0)
      domainName = domainName.substring(0, domainNameEndIndex);

    return domainName;
  }


  private _reqUrl(url: string, params: any): Observable<any> {
    // let headers = new Headers({ 'Content-Type': 'application/json' });
    let headers = new HttpHeaders();
    headers.append('Content-Type', 'application/json');
    // let options = new RequestOptions({ headers: headers });

    let paramsToSend = this.softCopyJSON(params);
    let domainName = this.getDomainName(url);
    if (domainName.toLowerCase() == window.location.host.toLowerCase())
      paramsToSend['CSRF_TOKEN'] = this.getCookie('CSRF_TOKEN');

    return this.http.post(url, paramsToSend, {
      headers: headers
    });
  }
}
