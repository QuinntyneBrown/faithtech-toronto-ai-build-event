import { LocalTimeInput } from './local-time-input';

export interface EventInput {
  timezone: string | null;
  start: LocalTimeInput | null;
  end: LocalTimeInput | null;
  title: string | null;
  venueName: string | null;
  address: string | null;
  latitude: number | null;
  longitude: number | null;
  waitingContent: string | null;
  closingContent: string | null;
  directionsUrl: string | null;
}
