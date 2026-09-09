import { HttpClient } from "@angular/common/http";
import { Injectable, signal } from "@angular/core";
import { IEventService } from "./event-service.contract";
import { PublicEventState } from "./public-event-state";

@Injectable()
export class EventService implements IEventService {
  readonly state = signal<PublicEventState | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  constructor(private readonly http: HttpClient) {}

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
