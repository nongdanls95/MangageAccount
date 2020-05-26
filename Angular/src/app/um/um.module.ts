import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { UmRoutingModule } from '@um/um-routing.module';
import { UmComponent } from '@um/um.component';

@NgModule({
  imports: [
    CommonModule,
    UmRoutingModule
  ],
  declarations: [UmComponent]
})
export class UmModule { }
