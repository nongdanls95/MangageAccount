import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AuthGuard } from '@app-base/auth.guard';
import { AuthService } from '@app-base/auth.service';
import { LoginComponent } from '@app-base/login/login.component';

const LOGIN_ROUTES: Routes = [
  {
    path: 'login',
    component: LoginComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(LOGIN_ROUTES)],
  exports: [RouterModule],
  providers: [AuthGuard, AuthService]
})
export class LoginRoutingModule { }
