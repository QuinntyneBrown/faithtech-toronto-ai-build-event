import { Component, DestroyRef, inject, input, OnInit, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { EVENT_SERVICE, EventDetail, EventInput, EventFailure, ServiceFailure } from '@faithtech/api';

@Component({ selector: 'ft-event-editor', imports: [FormsModule], templateUrl: './event-editor.html', styleUrl: './event-editor.css' })
export class EventEditor implements OnInit {
  private readonly service = inject(EVENT_SERVICE);
  private readonly destroy = inject(DestroyRef);
  readonly eventId = input.required<string>();
  readonly denied = output<void>();
  readonly detail = signal<EventDetail | null>(null);
  readonly error = signal('');
  readonly draft = signal<EventInput | null>(null);
  readonly busy = signal(false);
  readonly uncertain = signal(false);
  readonly saved = signal(false);
  readonly errors = signal<Record<string, string[]>>({});
  private operationId = crypto.randomUUID();
  change(field: keyof EventInput, value: string | number | null) {
    this.draft.update(draft => draft ? { ...draft, [field]: value } : null); this.saved.set(false);
  }
  async save() {
    const draft = this.draft(), current = this.detail();
    if (!draft || !current || this.busy()) return;
    this.busy.set(true); this.error.set(''); this.errors.set({}); this.saved.set(false);
    try {
      const result = await this.service.saveDraft(current.id, draft, current.version, this.operationId);
      if (this.destroy.destroyed) return;
      this.detail.set(result); this.draft.set(result); this.saved.set(true); this.uncertain.set(false); this.operationId = crypto.randomUUID();
    } catch (error) {
      if (error instanceof ServiceFailure && error.status === 401) this.denied.emit();
      else if (error instanceof EventFailure && error.status === 422) {
        this.errors.set(error.errors); this.error.set('Check the highlighted fields. Your changes are retained.'); this.operationId = crypto.randomUUID();
      } else { this.uncertain.set(true); this.error.set('The save could not be confirmed. Retry this save to check its outcome.'); }
    } finally { this.busy.set(false); }
  }
  ngOnInit() { void this.load(); }
  async load() {
    this.error.set('');
    try { const detail = await this.service.get(this.eventId()); if (!this.destroy.destroyed) { this.detail.set(detail); this.draft.set(detail); } }
    catch (error) {
      if (error instanceof ServiceFailure && error.status === 401) this.denied.emit();
      else this.error.set(error instanceof ServiceFailure && error.status === 404 ? 'This event was not found.' : 'The event could not be loaded. Retry to continue.');
    }
  }
}
