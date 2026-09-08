import { RosterEntry } from './roster-entry';

export interface RosterIssuance {
  entry: RosterEntry;
  code: string | null;
  previouslyCompleted: boolean;
  credentialVersion: string;
}
