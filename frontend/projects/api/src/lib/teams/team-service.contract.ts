export interface ITeamService {
  assignProject(teamId: string, projectId: string | null, expectedVersion: string): void;
}
