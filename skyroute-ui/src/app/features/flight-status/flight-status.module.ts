import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { FlightStatusComponent } from './flight-status.component';

const routes: Routes = [
  { path: '', component: FlightStatusComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
})
export class FlightStatusModule {}
