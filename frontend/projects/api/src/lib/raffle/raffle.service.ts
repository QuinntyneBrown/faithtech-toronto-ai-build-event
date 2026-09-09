import { HttpClient } from "@angular/common/http";
import { Injectable, inject, signal } from "@angular/core";
import { EVENT_SERVICE } from "../event/event-service.token";
import { IRaffleService } from "./raffle-service.contract";
import { RaffleSnapshot } from "./raffle-snapshot";

@Injectable()
export class RaffleService implements IRaffleService {
  readonly state = signal<RaffleSnapshot | null>(null);
  readonly loading = signal(false);
  readonly drawing = signal(false);
  readonly error = signal<string | null>(null);
  private readonly event = inject(EVENT_SERVICE);

  constructor(private readonly http: HttpClient) {}

  load(): void {
    this.loading.set(true);
    this.http.get<RaffleSnapshot>("/api/event/raffle").subscribe({
      next: state => {
        this.state.set(state);
        this.loading.set(false);
      },
      error: () => {
        this.error.set("We could not load the raffle. Please try again.");
        this.loading.set(false);
      }
    });
  }

  draw(expectedVersion: string): void {
    this.drawing.set(true);
    this.error.set(null);
    this.http.post("/api/admin/raffle/draws", { operationId: crypto.randomUUID(), expectedVersion }).subscribe({
      next: () => {
        this.drawing.set(false);
        this.load();
        this.event.load();
      },
      error: () => {
        this.drawing.set(false);
        this.error.set("The name could not be drawn. Refresh the raffle and try again.");
      }
    });
  }
}
