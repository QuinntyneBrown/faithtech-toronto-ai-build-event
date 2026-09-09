import { HttpClient } from "@angular/common/http";
import { Injectable, inject, signal } from "@angular/core";
import { EVENT_SERVICE } from "../event/event-service.token";
import { ProjectInput } from "./project-input";
import { IProjectService } from "./project-service.contract";

@Injectable()
export class ProjectService implements IProjectService {
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  private readonly event = inject(EVENT_SERVICE);

  constructor(private readonly http: HttpClient) {}

  add(input: ProjectInput, expectedVersion: string): void { this.mutate(this.http.post<string>("/api/admin/projects", { operationId: crypto.randomUUID(), expectedVersion, input })); }
  update(projectId: string, input: ProjectInput, expectedVersion: string): void { this.mutate(this.http.put<void>(`/api/admin/projects/${projectId}`, { operationId: crypto.randomUUID(), expectedVersion, input })); }
  remove(projectId: string, expectedVersion: string): void { this.mutate(this.http.delete<void>(`/api/admin/projects/${projectId}`, { body: { operationId: crypto.randomUUID(), expectedVersion } })); }

  private mutate(request: ReturnType<HttpClient["post"]>): void {
    this.loading.set(true); this.error.set(null);
    request.subscribe({ next: () => { this.loading.set(false); this.event.load(); }, error: () => { this.loading.set(false); this.error.set("The project change was not saved. Refresh and try again."); } });
  }
}
