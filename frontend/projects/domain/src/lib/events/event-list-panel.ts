import { Component, inject, output, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { EVENT_SERVICE, EventSummary } from '@faithtech/api';

@Component({ selector: 'ft-event-list-panel', imports: [RouterLink], templateUrl: './event-list-panel.html', styleUrl: './event-list-panel.css' })
export class EventListPanel {
  private readonly service = inject(EVENT_SERVICE);
  readonly requestCopy = output<EventSummary>();
  readonly events = signal<EventSummary[] | null>(null);
  readonly error = signal('');
  constructor() { void this.refresh(); }
  async refresh() {
    this.error.set('');
    try { this.events.set(await this.service.list()); }
    catch { this.events.set(null); this.error.set('Events could not be loaded. Please retry.'); }
  }
}
