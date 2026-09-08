import { EventSummary } from './event-summary';
import { EventDetail } from './event-detail';
import { EventInput } from './event-input';

export interface IEventService {
  getLogo(id: string): Promise<Blob>;
  uploadLogo(id: string, file: File, version: string, operationId: string): Promise<EventDetail>;
  get(id: string): Promise<EventDetail>;
  saveDraft(id: string, input: EventInput, version: string, operationId: string): Promise<EventDetail>;
  list(): Promise<EventSummary[]>;
  createDraft(title: string, operationId: string): Promise<EventSummary>;
}
