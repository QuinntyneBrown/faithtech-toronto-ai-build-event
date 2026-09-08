import { LocalTimeInput } from './local-time-input';

export interface EventSummary {
  timezone: string | null;
  start: LocalTimeInput | null;
  end: LocalTimeInput | null;
  startsAtUtc: string | null;
  endsAtUtc: string | null;
  id: string;
  title: string | null;
  published: boolean;
  useLiturgy: boolean;
  version: string;
}
