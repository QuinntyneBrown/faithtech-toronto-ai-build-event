import { ScheduleDetail } from './schedule-detail';
import { ScheduleInput } from './schedule-input';

export interface IScheduleService {
  get(id: string): Promise<ScheduleDetail>;
  save(id: string, input: ScheduleInput, version: string, operationId: string): Promise<ScheduleDetail>;
  applyReference(id: string, version: string, operationId: string): Promise<ScheduleDetail>;
}
