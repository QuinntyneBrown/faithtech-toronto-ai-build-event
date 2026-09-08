import { WindowInput } from './window-input';
import { StageInput } from './stage-input';

export interface ScheduleInput extends WindowInput {
  timezone: string | null;
  stages: StageInput[];
  selection: WindowInput | null;
  presentation: WindowInput | null;
}
