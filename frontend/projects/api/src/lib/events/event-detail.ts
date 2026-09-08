import { EventInput } from './event-input';
import { EventSummary } from './event-summary';

export interface EventDetail extends EventSummary, EventInput {}
