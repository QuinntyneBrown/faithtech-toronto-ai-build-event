import { HttpClient } from "@angular/common/http";
import { Injectable, inject, signal } from "@angular/core";
import { EVENT_SERVICE } from "../event/event-service.token";
import { ITeamService } from "./team-service.contract";

@Injectable()
export class TeamService implements ITeamService {
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  private readonly event = inject(EVENT_SERVICE);

  constructor(private readonly http: HttpClient) {}

  assignProject(teamId: string, projectId: string | null, expectedVersion: string): void {
    this.mutate(this.http.put<void>(`/api/admin/teams/${teamId}/project`, { operationId: crypto.randomUUID(), expectedVersion, projectId }));
  }

  moveMember(participantId: string, destination: "unassigned" | "new" | "existing", teamId: string | null, expectedVersion: string): void {
    this.mutate(this.http.post<void>("/api/admin/teams/moves", { operationId: crypto.randomUUID(), expectedVersion, participantId, destination, teamId }));
  }

  private mutate(request: ReturnType<HttpClient["post"]>): void {
    this.loading.set(true);
    this.error.set(null);
    request.subscribe({
      next: () => { this.loading.set(false); this.event.load(); },
      error: () => { this.loading.set(false); this.error.set("The team change was not saved. Refresh and try again."); }
    });
  }
}
