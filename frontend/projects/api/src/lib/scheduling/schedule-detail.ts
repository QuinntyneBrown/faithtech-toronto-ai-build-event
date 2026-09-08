import { ScheduleInput } from './schedule-input';

export interface ScheduleDetail extends ScheduleInput {
  id: string;
  version: string;
  selectionClosed: boolean;
  completed: boolean;
}
