import { HttpClient } from "@angular/common/http";
import { DestroyRef, Injectable, inject, signal } from "@angular/core";
import { HubConnection, HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";
import { firstValueFrom } from "rxjs";
import { IEventService } from "./event-service.contract";
import { PublicEventState } from "./public-event-state";
import { ServerTimeResponse } from "./server-time-response";

@Injectable()
export class EventService implements IEventService {
  readonly state = signal<PublicEventState | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly connected = signal(false);
  readonly connectionError = signal<string | null>(null);
  readonly clockSynchronized = signal(false);
  readonly clockError = signal<string | null>(null);
  private serverAnchorMilliseconds: number | null = null;
  private monotonicAnchorMilliseconds: number | null = null;
  private clockRefreshPending: Promise<void> | null = null;
  private recoveryRequired = false;
  private readonly destroyRef = inject(DestroyRef);
  private readonly connection: HubConnection;

  constructor(private readonly http: HttpClient) {
    this.connection = new HubConnectionBuilder().withUrl("/hubs/event-updates").build();
    this.connection.on("eventUpdated", (version: string) => {
      const current = this.state()?.version;
      if (current === undefined || BigInt(version) > BigInt(current)) {
        this.load();
      }
    });
    this.connection.onreconnecting(() => {
      this.recoveryRequired = true;
      this.connected.set(false);
      this.connectionError.set("Live updates are reconnecting. Changes are disabled until the event is synchronized.");
    });
    this.connection.onreconnected(() => {
      this.synchronize();
    });
    this.connection.onclose(() => {
      this.recoveryRequired = true;
      this.connected.set(false);
      this.connectionError.set("Live updates are unavailable. Changes are disabled until you reconnect.");
    });
    void this.startConnection();
    const onVisibilityChange = () => {
      if (document.visibilityState === "visible") {
        if (this.connection.state === HubConnectionState.Disconnected) this.retryLiveUpdates();
        else this.load();
      }
    };
    window.addEventListener("visibilitychange", onVisibilityChange);
    const clockTimer = window.setInterval(() => {
      if (document.visibilityState === "visible") void this.refreshServerTime();
    }, 30_000);
    this.destroyRef.onDestroy(() => {
      window.removeEventListener("visibilitychange", onVisibilityChange);
      window.clearInterval(clockTimer);
      void this.connection.stop();
    });
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.http.get<PublicEventState>("/api/event/state").subscribe({
      next: state => {
        const current = this.state();
        if (current === null || BigInt(state.version) >= BigInt(current.version)) {
          this.state.set(state);
        }
        this.loading.set(false);
        void this.refreshServerTime();
      },
      error: () => {
        this.error.set("We could not load the event. Please try again.");
        this.loading.set(false);
      }
    });
  }

  serverNow(): number {
    if (this.serverAnchorMilliseconds === null || this.monotonicAnchorMilliseconds === null) return Number.NaN;
    return this.serverAnchorMilliseconds + performance.now() - this.monotonicAnchorMilliseconds;
  }

  retryLiveUpdates(): void {
    this.recoveryRequired = false;
    if (this.connection.state === HubConnectionState.Disconnected) void this.startConnection();
    else this.synchronize();
  }

  private refreshServerTime(): Promise<void> {
    if (this.clockRefreshPending !== null) return this.clockRefreshPending;
    this.clockRefreshPending = this.sampleServerTime().finally(() => this.clockRefreshPending = null);
    return this.clockRefreshPending;
  }

  private async sampleServerTime(): Promise<void> {
    const samples: { roundTrip: number; serverAtReceipt: number; receivedAt: number }[] = [];
    for (let attempt = 0; attempt < 3; attempt++) {
      const startedAt = performance.now();
      try {
        const response = await firstValueFrom(this.http.get<ServerTimeResponse>("/api/event/time"));
        const receivedAt = performance.now();
        const serverTime = Date.parse(response.serverTimeUtc);
        if (!Number.isNaN(serverTime)) {
          const roundTrip = receivedAt - startedAt;
          samples.push({ roundTrip, serverAtReceipt: serverTime + roundTrip / 2, receivedAt });
        }
      } catch {
        // A later sample may still establish a trustworthy clock.
      }
    }

    const best = samples.sort((left, right) => left.roundTrip - right.roundTrip)[0];
    if (best === undefined || best.roundTrip > 2_000) {
      this.clockSynchronized.set(false);
      this.clockError.set("Current countdown time is unavailable. The scheduled welcome time is shown instead.");
      return;
    }

    this.serverAnchorMilliseconds = best.serverAtReceipt;
    this.monotonicAnchorMilliseconds = best.receivedAt;
    this.clockSynchronized.set(true);
    this.clockError.set(null);
  }

  private async startConnection(): Promise<void> {
    try {
      await this.connection.start();
      if (!this.recoveryRequired) this.synchronize();
    } catch {
      this.connected.set(false);
      this.connectionError.set("Live updates are unavailable. Changes are disabled until you reconnect.");
    }
  }

  private synchronize(): void {
    this.connected.set(false);
    this.connectionError.set("Live updates are synchronizing. Changes are disabled until the event is current.");
    this.http.get<PublicEventState>("/api/event/state").subscribe({
      next: state => {
        const current = this.state();
        if (current === null || BigInt(state.version) >= BigInt(current.version)) {
          this.state.set(state);
        }
        void this.refreshServerTime();
        if (this.connection.state === HubConnectionState.Connected) {
          this.connected.set(true);
          this.connectionError.set(null);
        }
      },
      error: () => {
        this.connected.set(false);
        this.connectionError.set("Live updates are unavailable. Changes are disabled until you reconnect.");
      }
    });
  }
}
