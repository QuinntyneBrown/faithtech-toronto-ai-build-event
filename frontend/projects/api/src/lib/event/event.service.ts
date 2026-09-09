import { HttpClient } from "@angular/common/http";
import { DestroyRef, Injectable, inject, signal } from "@angular/core";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { IEventService } from "./event-service.contract";
import { PublicEventState } from "./public-event-state";

@Injectable()
export class EventService implements IEventService {
  readonly state = signal<PublicEventState | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  private readonly destroyRef = inject(DestroyRef);
  private readonly connection: HubConnection;

  constructor(private readonly http: HttpClient) {
    this.connection = new HubConnectionBuilder().withUrl("/hubs/event-updates").withAutomaticReconnect().build();
    this.connection.on("eventUpdated", (version: string) => {
      const current = this.state()?.version;
      if (current === undefined || BigInt(version) > BigInt(current)) {
        this.load();
      }
    });
    this.connection.start().catch(() => this.error.set("Live updates are unavailable; refresh to see the latest event state."));
    this.destroyRef.onDestroy(() => { void this.connection.stop(); });
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.http.get<PublicEventState>("/api/event/state").subscribe({
      next: state => {
        this.state.set(state);
        this.loading.set(false);
      },
      error: () => {
        this.error.set("We could not load the event. Please try again.");
        this.loading.set(false);
      }
    });
  }
}
