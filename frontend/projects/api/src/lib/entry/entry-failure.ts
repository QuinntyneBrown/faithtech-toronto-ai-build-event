import { ServiceFailure } from '../access/service-failure';

export class EntryFailure extends ServiceFailure {
  constructor(status: number, public readonly code = '', public readonly errors: Record<string, string[]> = {}, retryAfterSeconds = 0) {
    super(status, retryAfterSeconds);
  }
}
