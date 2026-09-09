import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { DestroyRef, Injectable, inject, signal } from "@angular/core";
import { HubConnection, HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";
import { IAdministratorSessionService } from "./administrator-session-service.contract";

@Injectable()
export class AdministratorSessionService implements IAdministratorSessionService {
  readonly active = signal(false);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  private lastInteractionReportAt = Number.NEGATIVE_INFINITY;
  private interactionReportPending = false;
  private connectionStarting = false;
  private stoppingConnection = false;
  private readonly connection: HubConnection;
  private readonly destroyRef = inject(DestroyRef);

  constructor(private readonly http: HttpClient) {
    this.connection = new HubConnectionBuilder()
      .withUrl("/api/admin/updates")
      .withAutomaticReconnect([0, 2_000, 5_000, 10_000])
      .build();
    this.connection.on("sessionInvalidated", () => this.invalidate());
    this.connection.onreconnecting(() => this.active.set(false));
    this.connection.onreconnected(() => this.load());
    this.connection.onclose(() => {
      if (!this.stoppingConnection) this.invalidate("Administrator updates disconnected. Sign in again to continue.");
    });
    this.destroyRef.onDestroy(() => void this.connection.stop());
  }

  load(): void {
    this.http.get<{ authenticated: boolean }>("/api/admin/session").subscribe({
      next: state => {
        this.active.set(state.authenticated);
        if (state.authenticated) void this.connect();
      },
      error: () => this.invalidate()
    });
  }

  recordInteraction(): void {
    const now = performance.now();
    if (!this.active() || this.interactionReportPending || now - this.lastInteractionReportAt < 30_000) return;

    this.interactionReportPending = true;
    this.lastInteractionReportAt = now;
    this.http.post<void>("/api/admin/session/interaction", null).subscribe({
      next: () => this.interactionReportPending = false,
      error: () => {
        this.interactionReportPending = false;
        this.invalidate("Your administrator session has ended.");
      }
    });
  }

  signIn(passcode: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.http.post<{ authenticated: boolean }>("/api/admin/session", { passcode }).subscribe({
      next: state => {
        this.active.set(state.authenticated);
        this.lastInteractionReportAt = performance.now();
        this.loading.set(false);
        if (state.authenticated) void this.connect();
      },
      error: (response: HttpErrorResponse) => {
        this.active.set(false);
        const retryAfter = response.headers.get("Retry-After");
        this.error.set(response.status === 429 && retryAfter !== null
          ? `Too many attempts. Try again in ${retryAfter} seconds.`
          : "That passcode was not accepted.");
        this.loading.set(false);
      }
    });
  }

  signOut(): void {
    this.http.delete<void>("/api/admin/session").subscribe({
      next: () => { this.invalidate(); this.error.set(null); },
      error: () => this.error.set("We could not sign out. Try again.")
    });
  }

  private async connect(): Promise<void> {
    if (this.connectionStarting || this.connection.state !== HubConnectionState.Disconnected) return;
    this.connectionStarting = true;
    try {
      await this.connection.start();
    } catch {
      this.invalidate("Administrator updates are unavailable. Sign in again to continue.");
    } finally {
      this.connectionStarting = false;
    }
  }

  private invalidate(message: string | null = null): void {
    this.active.set(false);
    this.error.set(message);
    if (this.connection.state !== HubConnectionState.Disconnected) {
      this.stoppingConnection = true;
      void this.connection.stop().finally(() => this.stoppingConnection = false);
    }
  }
}
