import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

import { LoginRoutingModule } from '@app-base/login/login-routing.module';
import { LoginComponent } from '@app-base/login/login.component';
import { SwitcherComponent } from '@app-base/switcher/switcher.component';
import { AppService } from '@app-base/app.service';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    HttpClientModule,
    LoginRoutingModule
  ],
  declarations: [
    LoginComponent,
    SwitcherComponent
  ],
  providers: [AppService]
})
export class AppBaseModule { }
