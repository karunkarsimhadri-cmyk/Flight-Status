export type FlightStatus = 'OnTime' | 'Delayed' | 'Cancelled' | 'Diverted' | 'Unknown';

export interface FlightStatusResult {
  flightNumber: string;
  date: string;
  status: FlightStatus;
  scheduledDeparture?: string;
  actualDeparture?: string;
  scheduledArrival?: string;
  actualArrival?: string;
  terminal?: string;
  gate?: string;
  delayReason?: string;
  lastUpdatedUtc: string;
  source: string;
  message?: string;
}
