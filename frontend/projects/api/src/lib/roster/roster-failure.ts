import { ServiceFailure } from '../access/service-failure';

export class RosterFailure extends ServiceFailure {
  constructor(status: number, public readonly code = '', public readonly errors: Record<string, string[]> = {}) { super(status); }
}
