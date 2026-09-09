import { HttpClient, HttpErrorResponse } from "@angular/common/http";
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
  private pendingDraw: { operationId: string; expectedVersion: string } | null = null;

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
    const request = this.pendingDraw ?? { operationId: crypto.randomUUID(), expectedVersion };
    this.http.post("/api/admin/raffle/draws", request).subscribe({
      next: () => {
        this.pendingDraw = null;
        this.drawing.set(false);
        this.load();
        this.event.load();
      },
      error: (response: HttpErrorResponse) => {
        this.pendingDraw = response.status === 0 ? request : null;
        this.drawing.set(false);
        this.error.set(response.status === 0
          ? "The draw result may already be saved. Select DRAW NAME again to retry safely."
          : "The name could not be drawn. Refresh the raffle and try again.");
      }
    });
  }
}
