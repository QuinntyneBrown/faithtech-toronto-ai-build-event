import { HttpClient } from "@angular/common/http";
import { Injectable, inject, signal } from "@angular/core";
import { EVENT_SERVICE } from "./event-service.token";
import { IEventFlowService } from "./event-flow-service.contract";

@Injectable()
export class EventFlowService implements IEventFlowService {
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  private readonly event = inject(EVENT_SERVICE);

  constructor(private readonly http: HttpClient) {}

  advance(fromScreen: string, toScreen: string, expectedVersion: string): void {
    this.loading.set(true); this.error.set(null);
    this.http.post<void>("/api/admin/event/advance", { operationId: crypto.randomUUID(), expectedVersion, fromScreen, toScreen }).subscribe({
      next: () => { this.loading.set(false); this.event.load(); },
      error: () => { this.loading.set(false); this.error.set("The screen did not advance. Refresh and try again."); }
    });
  }
}
