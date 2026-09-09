import { HttpClient } from "@angular/common/http";
import { Injectable, signal } from "@angular/core";
import { IProfileService, ParticipantProfile } from "./profile-service.contract";

@Injectable()
export class ProfileService implements IProfileService {
  readonly profile = signal<ParticipantProfile | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly notice = signal<string | null>(null);

  constructor(private readonly http: HttpClient) {}

  load(): void {
    this.notice.set(null);
    this.http.get<ParticipantProfile>("/api/participant/profile").subscribe({
      next: profile => this.profile.set(profile),
      error: () => this.profile.set(null)
    });
  }

  save(input: ParticipantProfile, expectedVersion: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.http.put<ParticipantProfile>("/api/participant/profile", {
      operationId: crypto.randomUUID(),
      expectedVersion,
      input
    }).subscribe({
      next: profile => {
        this.profile.set(profile);
        this.loading.set(false);
      },
      error: () => {
        this.error.set("We could not save those details. Your raffle entry is still confirmed.");
        this.loading.set(false);
      }
    });
  }

  clear(): void {
    this.profile.set(null);
    this.error.set(null);
  }

  discardUnsaved(): void {
    this.notice.set("Your optional details were not submitted.");
  }
}
