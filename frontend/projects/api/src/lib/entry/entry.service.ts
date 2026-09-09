import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { DestroyRef, Injectable, inject, signal } from "@angular/core";
import { HubConnection, HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";
import { switchMap } from "rxjs";
import { EntryConfirmation, IEntryService } from "./entry-service.contract";
import { PROFILE_SERVICE } from "../profile/profile-service.token";
import { PublicEventState } from "../event/public-event-state";

interface EntryReceiptResponse {
  operationId: string;
}

@Injectable()
export class EntryService implements IEntryService {
  readonly confirmation = signal<EntryConfirmation | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  private readonly profile = inject(PROFILE_SERVICE);
  private readonly destroyRef = inject(DestroyRef);
  private readonly connection: HubConnection;
  private connectionStarting = false;
  private stoppingConnection = false;

  constructor(private readonly http: HttpClient) {
    this.connection = new HubConnectionBuilder()
      .withUrl("/api/participant/updates")
      .withAutomaticReconnect([0, 2_000, 5_000, 10_000])
      .build();
    this.connection.on("sessionInvalidated", () => this.invalidate());
    this.connection.onreconnecting(() => this.clearPrivateState());
    this.connection.onreconnected(() => this.load());
    this.connection.onclose(() => {
      if (!this.stoppingConnection) this.invalidate("Participant updates disconnected. Enter again to continue.");
    });
    this.destroyRef.onDestroy(() => void this.connection.stop());
  }

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
        void this.connect();
      },
      error: (response: HttpErrorResponse) => this.handleEntryError(response)
    });
  }

  private handleEntryError(response: HttpErrorResponse): void {
    const retryAfter = response.headers.get("Retry-After");
    if (response.status === 429 && retryAfter !== null) {
      this.finishWithError(`Too many entry attempts. Try again in ${retryAfter} seconds.`);
      return;
    }

    const detail = typeof response.error?.detail === "string" ? response.error.detail : "";
    const expectedMessages = [
      "This email is already entered; ask an administrator to update your details.",
      "Entry is closed; ask an administrator for help."
    ];
    if (expectedMessages.includes(detail)) {
      this.finishWithError(detail);
      return;
    }

    if (response.status === 409) {
      this.http.get<PublicEventState>("/api/event/state").subscribe({
        next: state => this.finishWithError(state.currentScreen === "countdown"
          ? expectedMessages[0]
          : expectedMessages[1]),
        error: () => this.finishWithError("We could not enter you into the raffle. Please check your email and try again.")
      });
      return;
    }

    this.finishWithError("We could not enter you into the raffle. Please check your email and try again.");
  }

  private finishWithError(message: string): void {
    this.error.set(message);
    this.loading.set(false);
  }

  load(): void {
    this.http.get<EntryConfirmation>("/api/participant/session").subscribe({
      next: confirmation => {
        this.confirmation.set(confirmation);
        void this.connect();
      },
      error: () => this.invalidate()
    });
  }

  leave(): void {
    this.http.delete<void>("/api/participant/session").subscribe({
      next: () => { this.invalidate(); this.error.set(null); },
      error: () => this.error.set("We could not clear this browser session. Try again.")
    });
  }

  private async connect(): Promise<void> {
    if (this.connectionStarting || this.connection.state !== HubConnectionState.Disconnected) return;
    this.connectionStarting = true;
    try {
      await this.connection.start();
    } catch {
      this.invalidate("Participant updates are unavailable. Enter again to continue.");
    } finally {
      this.connectionStarting = false;
    }
  }

  private clearPrivateState(): void {
    this.confirmation.set(null);
    this.profile.clear();
  }

  private invalidate(message: string | null = null): void {
    this.clearPrivateState();
    this.error.set(message);
    if (this.connection.state !== HubConnectionState.Disconnected) {
      this.stoppingConnection = true;
      void this.connection.stop().finally(() => this.stoppingConnection = false);
    }
  }
}
