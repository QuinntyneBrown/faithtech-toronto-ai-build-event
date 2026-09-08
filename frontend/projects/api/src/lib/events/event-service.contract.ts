import { EventSummary } from './event-summary';
import { EventDetail } from './event-detail';

export interface IEventService {
  get(id: string): Promise<EventDetail>;
  list(): Promise<EventSummary[]>;
  createDraft(title: string, operationId: string): Promise<EventSummary>;
}
