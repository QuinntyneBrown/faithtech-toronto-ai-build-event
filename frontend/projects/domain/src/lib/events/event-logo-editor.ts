import { Component, DestroyRef, effect, inject, input, output, signal } from '@angular/core';
import { EVENT_SERVICE, EventDetail, EventFailure, ServiceFailure } from '@faithtech/api';

@Component({ selector: 'ft-event-logo-editor', templateUrl: './event-logo-editor.html', styleUrl: './event-logo-editor.css' })
export class EventLogoEditor {
  private readonly service = inject(EVENT_SERVICE);
  private readonly destroy = inject(DestroyRef);
  readonly event = input.required<EventDetail>();
  readonly blocked = input(false);
  readonly saved = output<EventDetail>();
  readonly denied = output<void>();
  readonly file = signal<File | null>(null);
  readonly busy = signal(false);
  readonly uncertain = signal(false);
  readonly error = signal('');
  readonly success = signal(false);
  readonly preview = signal('');
  readonly previewError = signal(false);
  readonly conflict = signal<EventDetail | null>(null);
  private operationId = crypto.randomUUID();
  private version = '';
  constructor() {
    effect(onCleanup => {
      const event = this.event(); let active = true; let url = '';
      this.preview.set(''); this.previewError.set(false);
      if (event.logo) void this.service.getLogo(event.id).then(blob => {
        if (active) { url = URL.createObjectURL(blob); this.preview.set(url); }
      }).catch(error => {
        if (!active) return;
        if (error instanceof ServiceFailure && error.status === 401) this.denied.emit();
        else this.previewError.set(true);
      });
      onCleanup(() => { active = false; if (url) URL.revokeObjectURL(url); });
    });
  }
  choose(event: Event) {
    this.file.set((event.target as HTMLInputElement).files?.[0] ?? null);
    this.error.set(''); this.success.set(false); this.operationId = crypto.randomUUID();
  }
  hasUnsavedChanges() { return !!this.file() || this.busy() || this.uncertain(); }
  useCurrent() {
    const current = this.conflict(); if (!current) return;
    this.saved.emit(current); this.conflict.set(null); this.error.set(''); this.operationId = crypto.randomUUID();
  }
  async upload() {
    const file = this.file();
    if (!file || this.busy() || this.blocked() || this.conflict()) return;
    if (!this.uncertain()) this.version = this.event().version;
    this.busy.set(true); this.error.set(''); this.success.set(false);
    try {
      const result = await this.service.uploadLogo(this.event().id, file, this.version, this.operationId);
      if (this.destroy.destroyed) return;
      this.saved.emit(result); this.file.set(null); this.uncertain.set(false); this.success.set(true);
      this.operationId = crypto.randomUUID();
    } catch (error) {
      if (error instanceof ServiceFailure && error.status === 401) this.denied.emit();
      else if (error instanceof EventFailure && error.code === 'stale-version' && error.current) {
        this.uncertain.set(false); this.conflict.set(error.current);
        this.error.set('Another administrator changed this event. Load the latest saved values before uploading your selected logo.');
      } else if (error instanceof ServiceFailure && error.status >= 400 && error.status < 500) {
        this.uncertain.set(false); this.operationId = crypto.randomUUID();
        this.error.set(error instanceof EventFailure && error.errors['logo'] ? error.errors['logo'].join(' ') : 'The upload was rejected. Check the file and your access before retrying.');
      } else { this.uncertain.set(true); this.error.set('The upload could not be confirmed. Retry to check its outcome.'); }
    } finally { this.busy.set(false); }
  }
}
