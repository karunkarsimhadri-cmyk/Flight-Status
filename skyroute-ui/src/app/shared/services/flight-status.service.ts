import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { FlightStatusResult } from '../models/flight-status.model';

@Injectable({ providedIn: 'root' })
export class FlightStatusService {
  private readonly apiBase = '';
  private readonly http = inject(HttpClient);

  getStatus(flightNumber: string, date: string): Observable<FlightStatusResult> {
    const params = new HttpParams()
      .set('flightNumber', flightNumber)
      .set('date', date);
    return this.http
      .get<FlightStatusResult>(`${this.apiBase}/flights/status`, { params })
      .pipe(catchError(this.handleError));
  }

  private handleError(err: HttpErrorResponse): Observable<never> {
    const msg = err.error?.error ?? err.message ?? `HTTP ${err.status}`;
    return throwError(() => new Error(msg));
  }
}
