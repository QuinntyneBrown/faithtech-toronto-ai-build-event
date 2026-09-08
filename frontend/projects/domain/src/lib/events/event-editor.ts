import { Component, DestroyRef, inject, input, OnInit, output, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { EVENT_SERVICE, EventDetail, EventInput, EventFailure, ServiceFailure } from '@faithtech/api';
import { validateEventInput } from './validate-event-input';
import { EventLogoEditor } from './event-logo-editor';

@Component({ selector: 'ft-event-editor', imports: [FormsModule, EventLogoEditor], templateUrl: './event-editor.html', styleUrl: './event-editor.css' })
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
  readonly conflict = signal<EventDetail | null>(null);
  readonly logoEditor = viewChild(EventLogoEditor);
  logoPending() { return this.logoEditor()?.busy() || this.logoEditor()?.uncertain() || !!this.logoEditor()?.conflict(); }
  draftChanged() { return JSON.stringify(this.draft()) !== JSON.stringify(this.detail()); }
  hasUnsavedChanges() { return this.busy() || this.uncertain() || this.draftChanged() || this.logoEditor()?.hasUnsavedChanges(); }
  logoSaved(event: EventDetail) { this.detail.set(event); this.draft.set(event); this.saved.set(false); }
  reapply() {
    const current = this.conflict();
    if (current) this.detail.set(current);
    this.conflict.set(null); this.error.set(''); this.operationId = crypto.randomUUID();
  }
  reloadCurrent() { const current = this.conflict(); this.reapply(); if (current) this.draft.set(current); }
  private operationId = crypto.randomUUID();
  change<K extends keyof EventInput>(field: K, value: EventInput[K]) {
    this.draft.update(draft => draft ? { ...draft, [field]: value } : null); this.saved.set(false);
  }
  changeTime(field: 'start' | 'end', local: string) { this.change(field, local ? { local, offsetMinutes: null } : null); }
  changeOffset(field: 'start' | 'end', offsetMinutes: number | null) {
    const value = this.draft()?.[field]; if (value) this.change(field, { ...value, offsetMinutes });
  }
  async save() {
    const draft = this.draft(), current = this.detail();
    if (!draft || !current || this.busy() || this.conflict() || this.logoPending()) return;
    this.busy.set(true); this.error.set(''); this.errors.set({}); this.saved.set(false);
    try {
      const result = await this.service.saveDraft(current.id, validateEventInput(draft), current.version, this.operationId);
      if (this.destroy.destroyed) return;
      this.detail.set(result); this.draft.set(result); this.saved.set(true); this.uncertain.set(false); this.operationId = crypto.randomUUID();
    } catch (error) {
      if (error instanceof ServiceFailure && error.status === 401) this.denied.emit();
      else if (error instanceof EventFailure && error.status === 422) {
        this.uncertain.set(false); this.errors.set(error.errors); this.error.set('Check the highlighted fields. Your changes are retained.'); this.operationId = crypto.randomUUID();
      } else if (error instanceof EventFailure && error.code === 'stale-version' && error.current) {
        this.uncertain.set(false); this.conflict.set(error.current); this.error.set('Another administrator saved changes. Compare the current values before reapplying yours.');
      } else if (error instanceof ServiceFailure && error.status >= 400 && error.status < 500) {
        this.uncertain.set(false); this.operationId = crypto.randomUUID(); this.error.set('The save was rejected. Your changes are retained; check your access and reload before retrying.');
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
