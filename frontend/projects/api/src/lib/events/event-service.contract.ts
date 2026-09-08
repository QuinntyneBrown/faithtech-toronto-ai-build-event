import { EventSummary } from './event-summary';

export interface IEventService {
  list(): Promise<EventSummary[]>;
  createDraft(title: string, operationId: string): Promise<EventSummary>;
}
