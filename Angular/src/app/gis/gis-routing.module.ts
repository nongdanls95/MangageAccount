import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AuthGuard } from '@app-base/auth.guard';
import { GisComponent } from '@gis/gis.component';

const GIS_ROUTES: Routes = [
  {
    path: 'gis',
    component: GisComponent,
    canActivate: [AuthGuard]
  }
];

@NgModule({
  imports: [
    RouterModule.forChild(GIS_ROUTES)
  ],
  exports: [RouterModule]
})
export class GisRoutingModule { }
