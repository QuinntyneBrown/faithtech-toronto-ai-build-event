import { ServiceFailure } from '../access/service-failure';
import { EventDetail } from './event-detail';

export class EventFailure extends ServiceFailure {
  constructor(status: number, public readonly code = '', public readonly errors: Record<string, string[]> = {}, public readonly current?: EventDetail) {
    super(status);
  }
}
