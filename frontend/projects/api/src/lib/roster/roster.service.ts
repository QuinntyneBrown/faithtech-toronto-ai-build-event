import { HttpClient } from "@angular/common/http";
import { Injectable, inject, signal } from "@angular/core";
import { AdministratorParticipant, IRosterService } from "./roster-service.contract";
import { EVENT_SERVICE } from "../event/event-service.token";
import { AdministratorParticipantInput } from "./administrator-participant-input";

@Injectable()
export class RosterService implements IRosterService {
  readonly participants = signal<AdministratorParticipant[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  private readonly event = inject(EVENT_SERVICE);

  constructor(private readonly http: HttpClient) {}

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.http.get<AdministratorParticipant[]>("/api/admin/participants").subscribe({
      next: participants => { this.participants.set(participants); this.loading.set(false); },
      error: () => { this.error.set("We could not load the participant roster."); this.loading.set(false); }
    });
  }

  add(email: string, expectedVersion: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.http.post<AdministratorParticipant>("/api/admin/participants", { operationId: crypto.randomUUID(), expectedVersion, email }).subscribe({
      next: () => { this.loading.set(false); this.load(); this.event.load(); },
      error: () => { this.loading.set(false); this.error.set("The participant could not be added. Check the email and refresh before trying again."); }
    });
  }

  update(participantId: string, input: AdministratorParticipantInput, expectedVersion: string): void {
    this.mutate(
      this.http.put<AdministratorParticipant>(`/api/admin/participants/${participantId}`, { operationId: crypto.randomUUID(), expectedVersion, input }),
      "The participant could not be updated. Refresh before trying again."
    );
  }

  remove(participantId: string, expectedVersion: string): void {
    this.mutate(
      this.http.delete<void>(`/api/admin/participants/${participantId}`, { body: { operationId: crypto.randomUUID(), expectedVersion } }),
      "The participant could not be removed. Refresh before trying again."
    );
  }

  private mutate(request: ReturnType<HttpClient["post"]>, message: string): void {
    this.loading.set(true);
    this.error.set(null);
    request.subscribe({
      next: () => { this.loading.set(false); this.load(); this.event.load(); },
      error: () => { this.loading.set(false); this.error.set(message); }
    });
  }
}
