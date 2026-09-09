import { Signal } from "@angular/core";

export interface ITeamService {
  readonly loading: Signal<boolean>;
  readonly error: Signal<string | null>;
  assignProject(teamId: string, projectId: string | null, expectedVersion: string): void;
  moveMember(participantId: string, destination: "unassigned" | "new" | "existing", teamId: string | null, expectedVersion: string): void;
}
