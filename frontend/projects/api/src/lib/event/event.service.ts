import { HttpClient } from "@angular/common/http";
import { DestroyRef, Injectable, inject, signal } from "@angular/core";
import { HubConnection, HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";
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
  private readonly serverOffsetMilliseconds = signal(0);
  private readonly destroyRef = inject(DestroyRef);
  private readonly connection: HubConnection;

  constructor(private readonly http: HttpClient) {
    this.connection = new HubConnectionBuilder().withUrl("/hubs/event-updates").withAutomaticReconnect([0, 2000, 5000, 10000]).build();
    this.connection.on("eventUpdated", (version: string) => {
      const current = this.state()?.version;
      if (current === undefined || BigInt(version) > BigInt(current)) {
        this.load();
      }
    });
    this.connection.onreconnecting(() => {
      this.connected.set(false);
      this.connectionError.set("Live updates are reconnecting. Changes are disabled until the event is synchronized.");
    });
    this.connection.onreconnected(() => {
      this.connected.set(true);
      this.connectionError.set(null);
      this.load();
    });
    this.connection.onclose(() => {
      this.connected.set(false);
      this.connectionError.set("Live updates are unavailable. Changes are disabled until you reconnect.");
    });
    void this.startConnection();
    this.destroyRef.onDestroy(() => { void this.connection.stop(); });
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.http.get<PublicEventState>("/api/event/state").subscribe({
      next: state => {
        this.state.set(state);
        this.loading.set(false);
        this.refreshServerTime();
      },
      error: () => {
        this.error.set("We could not load the event. Please try again.");
        this.loading.set(false);
      }
    });
  }

  serverNow(): number {
    return Date.now() + this.serverOffsetMilliseconds();
  }

  retryLiveUpdates(): void {
    if (this.connection.state === HubConnectionState.Disconnected) void this.startConnection();
  }

  private refreshServerTime(): void {
    const requestStartedAt = Date.now();
    this.http.get<ServerTimeResponse>("/api/event/time").subscribe({
      next: response => {
        const requestCompletedAt = Date.now();
        const serverTime = Date.parse(response.serverTimeUtc);
        if (!Number.isNaN(serverTime)) {
          this.serverOffsetMilliseconds.set(serverTime - ((requestStartedAt + requestCompletedAt) / 2));
        }
      }
    });
  }

  private async startConnection(): Promise<void> {
    try {
      await this.connection.start();
      this.connected.set(true);
      this.connectionError.set(null);
      this.load();
    } catch {
      this.connected.set(false);
      this.connectionError.set("Live updates are unavailable. Changes are disabled until you reconnect.");
    }
  }
}
