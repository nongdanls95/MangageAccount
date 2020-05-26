import { Component, OnInit } from '@angular/core';

import { AuthService } from '@app-base/auth.service';
import { AppService } from '@app-base/app.service';
import { System } from '@app-base/interface/app-config';

@Component({
  selector: 'app-switcher',
  templateUrl: './switcher.component.html',
  styleUrls: ['./switcher.component.scss']
})
export class SwitcherComponent implements OnInit {

  systems: System[];

  constructor(
    private appService: AppService,
    private authService: AuthService
  ) { }

  ngOnInit() {
    this.systems = this.appService.appConfig.Systems;
  }

  logout() {
    this.authService.logout().subscribe(response => {
      if (response.success) {
        window.location.reload();
      }
    });
  }

}
