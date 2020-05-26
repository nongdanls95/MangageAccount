import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { loadModules } from 'esri-loader';

import { GisService } from '@gis/gis.service';
import { loaderOptions } from '@gis/config/loader-options';

@Component({
    selector: 'app-gis',
    templateUrl: './gis.component.html',
    styleUrls: ['./gis.component.scss']
})
export class GisComponent implements OnInit {

    // @ViewChild('eleMap', { static: false }) eleMap !: ElementRef;

    constructor(private gisService: GisService) { }

    ngOnInit() {
        // loadModules(['esri/map'], loaderOptions)
        //   .then(([Map]) => {
        //     // you can now create a new FlareClusterLayer and add it to a new Map
        //     this.gisService.map = new Map(this.eleMap.nativeElement, {
        //       center: [-118, 34.5],
        //       zoom: 8,
        //       basemap: 'topo'
        //     });
        //     this.gisService.map.on('load', (event: any) => this.onMapLoad(event));
        //   })
        //   .catch(err => {
        //     // handle any errors
        //     console.error(err);
        //   });

    }

    private onMapLoad(map) {
        console.log(map);
    }

}
