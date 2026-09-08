import { EntryHeader } from './entry-header';
import { EntryResult } from './entry-result';

export interface IEntryService {
  header(eventId: string): Promise<EntryHeader | null>;
  authenticate(eventId: string, email: string, entryCode: string, returnTo?: string): Promise<EntryResult>;
}
