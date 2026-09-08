import { WindowInput } from './window-input';

export interface StageInput extends WindowInput {
  id: string;
  name: string | null;
  phase: string | null;
  screenType: string;
  content: string | null;
  resourceUrl: string | null;
}
