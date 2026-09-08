import { Component, DestroyRef, inject, input, OnInit, output, signal } from '@angular/core';
import { EVENT_SERVICE, EventDetail, ServiceFailure } from '@faithtech/api';

@Component({ selector: 'ft-event-editor', templateUrl: './event-editor.html', styleUrl: './event-editor.css' })
export class EventEditor implements OnInit {
  private readonly service = inject(EVENT_SERVICE);
  private readonly destroy = inject(DestroyRef);
  readonly eventId = input.required<string>();
  readonly denied = output<void>();
  readonly detail = signal<EventDetail | null>(null);
  readonly error = signal('');
  ngOnInit() { void this.load(); }
  async load() {
    this.error.set('');
    try { const detail = await this.service.get(this.eventId()); if (!this.destroy.destroyed) this.detail.set(detail); }
    catch (error) {
      if (error instanceof ServiceFailure && error.status === 401) this.denied.emit();
      else this.error.set(error instanceof ServiceFailure && error.status === 404 ? 'This event was not found.' : 'The event could not be loaded. Retry to continue.');
    }
  }
}
