import { HttpClient } from "@angular/common/http";
import { Injectable, inject, signal } from "@angular/core";
import { AdministratorParticipant, IRosterService } from "./roster-service.contract";
import { EVENT_SERVICE } from "../event/event-service.token";

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
}
