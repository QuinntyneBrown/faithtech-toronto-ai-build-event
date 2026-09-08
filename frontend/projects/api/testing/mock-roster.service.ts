import { Injectable } from '@angular/core';
import { IRosterService, RegistrationInput, RosterEntry, RosterIssuance, RosterFailure } from '@faithtech/api';

@Injectable()
export class MockRosterService implements IRosterService {
  list(eventId: string) { return this.call<RosterEntry[]>('list', { eventId }); }
  add(eventId: string, input: RegistrationInput, operationId: string) { return this.call<RosterIssuance>('add', { eventId, input, operationId }); }
  rename(eventId: string, registrationId: string, displayName: string, version: string, operationId: string) {
    return this.call<RosterEntry>('rename', { eventId, registrationId, displayName, version, operationId });
  }
  private async call<T>(operation: string, args: object): Promise<T> {
    const bridge = window as unknown as { __faithtechRoster(operation: string, args: object): Promise<{
      result: T; failed?: boolean; status?: number; code?: string; errors?: Record<string, string[]>;
    }> };
    const response = await bridge.__faithtechRoster(operation, args);
    if (response.failed || response.status) throw new RosterFailure(response.status ?? 0, response.code, response.errors);
    return response.result;
  }
}
