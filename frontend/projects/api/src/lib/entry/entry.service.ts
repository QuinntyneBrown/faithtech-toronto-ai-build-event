import { HttpClient } from "@angular/common/http";
import { Injectable, signal } from "@angular/core";
import { switchMap } from "rxjs";
import { EntryConfirmation, IEntryService } from "./entry-service.contract";

interface EntryReceiptResponse {
  operationId: string;
}

@Injectable()
export class EntryService implements IEntryService {
  readonly confirmation = signal<EntryConfirmation | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  constructor(private readonly http: HttpClient) {}

  enter(email: string, expectedVersion: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.http.post<EntryReceiptResponse>("/api/participant/entry-receipt", {}).pipe(
      switchMap(receipt => this.http.post<EntryConfirmation>("/api/participant/entries", {
        operationId: receipt.operationId,
        expectedVersion,
        input: { email }
      }))
    ).subscribe({
      next: confirmation => {
        this.confirmation.set(confirmation);
        this.loading.set(false);
      },
      error: () => {
        this.error.set("We could not enter you into the raffle. Please check your email and try again.");
        this.loading.set(false);
      }
    });
  }

  load(): void {
    this.http.get<EntryConfirmation>("/api/participant/session").subscribe({
      next: confirmation => this.confirmation.set(confirmation),
      error: () => this.confirmation.set(null)
    });
  }

  leave(): void {
    this.http.delete<void>("/api/participant/session").subscribe({
      next: () => { this.confirmation.set(null); this.error.set(null); },
      error: () => this.error.set("We could not clear this browser session. Try again.")
    });
  }
}
