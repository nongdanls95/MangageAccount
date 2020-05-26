import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { GisRoutingModule } from '@gis/gis-routing.module';
import { GisComponent } from '@gis/gis.component';
import { GisService } from '@gis/gis.service';
import { EsriConfigService } from '@gis/services/esri-config.service';
import { GeometryUtilService } from '@gis/services/geometry-util.service';
import { ProjectionUtilService } from '@gis/services/projection-util.service';

@NgModule({
    imports: [
        CommonModule,
        GisRoutingModule
    ],
    declarations: [
        GisComponent
    ],
    providers: [
        GisService,
        EsriConfigService,
        GeometryUtilService,
        ProjectionUtilService
    ]
})
export class GisModule { }
