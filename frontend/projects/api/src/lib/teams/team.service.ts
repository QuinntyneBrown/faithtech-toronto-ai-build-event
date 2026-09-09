import { HttpClient } from "@angular/common/http";
import { Injectable, inject } from "@angular/core";
import { EVENT_SERVICE } from "../event/event-service.token";
import { ITeamService } from "./team-service.contract";

@Injectable()
export class TeamService implements ITeamService {
  private readonly event = inject(EVENT_SERVICE);

  constructor(private readonly http: HttpClient) {}

  assignProject(teamId: string, projectId: string | null, expectedVersion: string): void {
    this.http.put<void>(`/api/admin/teams/${teamId}/project`, { operationId: crypto.randomUUID(), expectedVersion, projectId }).subscribe({
      next: () => this.event.load()
    });
  }

  moveMember(participantId: string, destination: "unassigned" | "new" | "existing", teamId: string | null, expectedVersion: string): void {
    this.http.post<void>("/api/admin/teams/moves", { operationId: crypto.randomUUID(), expectedVersion, participantId, destination, teamId }).subscribe({
      next: () => this.event.load()
    });
  }
}
