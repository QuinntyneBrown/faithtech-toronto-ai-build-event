export interface RosterEntry {
  id: string;
  displayName: string;
  active: boolean;
  emailBound: boolean;
  firstAccessAtUtc: string | null;
  version: string;
}
