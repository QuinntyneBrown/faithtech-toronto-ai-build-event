import { ServiceFailure } from '../access/service-failure';
import { ScheduleDetail } from './schedule-detail';

export class ScheduleFailure extends ServiceFailure {
  constructor(status: number, public readonly code = '', public readonly errors: Record<string, string[]> = {}, public readonly current?: ScheduleDetail) { super(status); }
}
