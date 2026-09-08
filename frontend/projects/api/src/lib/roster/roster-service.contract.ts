import { RegistrationInput } from './registration-input';
import { RosterEntry } from './roster-entry';
import { RosterIssuance } from './roster-issuance';

export interface IRosterService {
  list(eventId: string): Promise<RosterEntry[]>;
  add(eventId: string, input: RegistrationInput, operationId: string): Promise<RosterIssuance>;
}
