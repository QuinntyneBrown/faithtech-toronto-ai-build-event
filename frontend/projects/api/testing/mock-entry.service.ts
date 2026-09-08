import { Injectable } from '@angular/core';
import { IEntryService, EntryHeader, EntryResult, EntryFailure } from '@faithtech/api';

@Injectable()
export class MockEntryService implements IEntryService {
  async header(eventId: string): Promise<EntryHeader | null> {
    try { return await this.call<EntryHeader>('header', { eventId }); }
    catch (error) {
      if (error instanceof EntryFailure && error.status === 404) return null;
      throw error;
    }
  }
  authenticate(eventId: string, email: string, entryCode: string, returnTo?: string) {
    return this.call<EntryResult>('authenticate', { eventId, email, entryCode, returnTo });
  }
  private async call<T>(operation: string, args: object): Promise<T> {
    const bridge = window as unknown as { __faithtechEntry(operation: string, args: object): Promise<{
      result: T; failed?: boolean; status?: number; code?: string; errors?: Record<string, string[]>;
    }> };
    const response = await bridge.__faithtechEntry(operation, args);
    if (response.failed || response.status) throw new EntryFailure(response.status ?? 0, response.code, response.errors);
    return response.result;
  }
}
