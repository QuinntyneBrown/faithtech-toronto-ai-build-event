import { RegistrationInput } from './registration-input';
import { RosterEntry } from './roster-entry';
import { RosterIssuance } from './roster-issuance';

export interface IRosterService {
  list(eventId: string): Promise<RosterEntry[]>;
  add(eventId: string, input: RegistrationInput, operationId: string): Promise<RosterIssuance>;
  rename(eventId: string, registrationId: string, displayName: string, version: string, operationId: string): Promise<RosterEntry>;
  deactivate(eventId: string, registrationId: string, version: string, operationId: string): Promise<RosterEntry>;
  replaceCode(eventId: string, registrationId: string, version: string, operationId: string): Promise<RosterIssuance>;
  reactivate(eventId: string, registrationId: string, version: string, operationId: string): Promise<RosterEntry>;
}
