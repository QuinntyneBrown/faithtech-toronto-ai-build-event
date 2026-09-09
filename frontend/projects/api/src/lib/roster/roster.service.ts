import { HttpClient } from "@angular/common/http";
import { Injectable, signal } from "@angular/core";
import { AdministratorParticipant, IRosterService } from "./roster-service.contract";

@Injectable()
export class RosterService implements IRosterService {
  readonly participants = signal<AdministratorParticipant[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  constructor(private readonly http: HttpClient) {}

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.http.get<AdministratorParticipant[]>("/api/admin/participants").subscribe({
      next: participants => { this.participants.set(participants); this.loading.set(false); },
      error: () => { this.error.set("We could not load the participant roster."); this.loading.set(false); }
    });
  }
}
