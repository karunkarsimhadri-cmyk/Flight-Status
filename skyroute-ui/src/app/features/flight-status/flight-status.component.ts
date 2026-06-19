import { Component, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FlightStatusService } from '../../shared/services/flight-status.service';
import { FlightStatusResult, FlightStatus } from '../../shared/models/flight-status.model';

type ViewState = 'idle' | 'loading' | 'result' | 'error';

@Component({
  selector: 'app-flight-status',
  imports: [CommonModule, FormsModule],
  templateUrl: './flight-status.component.html',
  styleUrl: './flight-status.component.css',
})
export class FlightStatusComponent {
  private svc = inject(FlightStatusService);

  flightNumber = signal('');
  date = signal(new Date().toISOString().slice(0, 10));
  viewState = signal<ViewState>('idle');
  result = signal<FlightStatusResult | null>(null);
  errorMsg = signal('');

  readonly statusIcons: Record<FlightStatus, string> = {
    OnTime: '✓', Delayed: '⚠', Cancelled: '✕', Diverted: '↪', Unknown: '?',
  };

  readonly sampleFlights = ['AA100', 'AA200', 'AA300', 'AA400', 'AA500', 'AA600', 'BA100'];

  hasAeroTrackFields = computed(() => {
    const r = this.result();
    return !!(r?.terminal || r?.gate || r?.delayReason);
  });

  lookup(): void {
    const flight = this.flightNumber().trim().toUpperCase();
    const date = this.date();

    if (!flight || !date) {
      this.errorMsg.set('Please enter both a flight number and date.');
      this.viewState.set('error');
      return;
    }

    this.viewState.set('loading');
    this.result.set(null);

    this.svc.getStatus(flight, date).subscribe({
      next: (data: FlightStatusResult) => {
        this.result.set(data);
        this.viewState.set('result');
      },
      error: (err: Error) => {
        this.errorMsg.set(
          err.message || 'Could not reach the API. Make sure the backend is running on port 5050.'
        );
        this.viewState.set('error');
      },
    });
  }

  fmtTime(iso?: string): string {
    if (!iso) return '—';
    return new Date(iso).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  fmtUpdated(iso: string): string {
    return new Date(iso).toLocaleTimeString();
  }

  fillFlight(code: string): void {
    this.flightNumber.set(code);
    this.lookup();
  }
}
