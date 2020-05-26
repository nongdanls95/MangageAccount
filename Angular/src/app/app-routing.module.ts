import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AuthGuard } from '@app-base/auth.guard';
import { SwitcherComponent } from '@app-base/switcher/switcher.component';
import { GisComponent } from '@gis/gis.component';


const APP_ROUTES: Routes = [
  {
    path: '',
    component: GisComponent,
    // canActivate: [AuthGuard]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(APP_ROUTES)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
