import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AuthGuard } from '@app-base/auth.guard';
import { UmComponent } from '@um/um.component';

const UM_ROUTES: Routes = [
  {
    path: 'um',
    component: UmComponent,
    canActivate: [AuthGuard]
  }
];

@NgModule({
  imports: [
    RouterModule.forChild(UM_ROUTES)
  ],
  exports: [RouterModule]
})
export class UmRoutingModule { }
