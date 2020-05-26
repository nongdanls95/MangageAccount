import { Injectable } from '@angular/core';
import { Observable ,  of } from 'rxjs';
import { map } from 'rxjs/operators';

import { AppService } from '@app-base/app.service';

@Injectable()
export class AuthService {

  // store the URL so we can redirect after logging in
  redirectUrl: string;

  constructor(private appService: AppService) { }

  isLoggedIn(): Observable<boolean> {
    if (this.appService.appConfig != null) {
      return of(true);
    } else {
      return this.appService.reqUrl(`${this.appService.hostName}Handlers/AppInit.ashx`, null).pipe(
        map(response => {
          if (response.success && response.userInfo) {
            // update global user information
            this.appService.user = response.userInfo;

            // update global application config
            this.appService.appConfig = {
              Systems: response.data,
              Functions: response.data2,
              Services: response.data3,
              Layers: response.data4,
              General: response.data5
            };
          }

          return response.success && response.userInfo != undefined;
        })
      );
    }
  }

  login(credential: any): Observable<any> {
    return this.appService.reqUrl(`${this.appService.hostName}/Handlers/AppLogin.ashx`, credential).pipe(
      map(response => {
        if (response.success) {
          this.appService.user = response.data[0];
        }

        return response;
      })
    );
  }

  logout(): Observable<any> {
    return this.appService.reqUrl(`${this.appService.hostName}/Handlers/AppLogout.ashx`, null).pipe(
      map(response => {
        if (response.success) {
          this.appService.user = null;
          this.appService.appConfig = null;
        }

        return response;
      })
    );
  }
}
