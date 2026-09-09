export interface ITeamService {
  assignProject(teamId: string, projectId: string | null, expectedVersion: string): void;
  moveMember(participantId: string, destination: "unassigned" | "new" | "existing", teamId: string | null, expectedVersion: string): void;
}
