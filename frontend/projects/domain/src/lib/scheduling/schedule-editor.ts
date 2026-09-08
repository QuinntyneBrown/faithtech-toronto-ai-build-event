import { Component, DestroyRef, inject, input, OnInit, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SCHEDULE_SERVICE, ScheduleDetail, ScheduleFailure, ScheduleInput, StageInput } from '@faithtech/api';
import { WindowEditor } from './window-editor';

@Component({ selector: 'ft-schedule-editor', imports: [FormsModule, WindowEditor], templateUrl: './schedule-editor.html', styleUrl: './schedule-editor.css' })
export class ScheduleEditor implements OnInit {
  private readonly service = inject(SCHEDULE_SERVICE);
  private readonly destroy = inject(DestroyRef);
  readonly eventId = input.required<string>();
  readonly editStage = output<StageInput>();
  readonly denied = output<void>();
  readonly detail = signal<ScheduleDetail | null>(null);
  readonly draft = signal<ScheduleInput | null>(null);
  readonly error = signal('');
  readonly busy = signal(false);
  readonly uncertain = signal(false);
  readonly saved = signal(false);
  readonly conflict = signal<ScheduleDetail | null>(null);
  private operationId = crypto.randomUUID();
  hasUnsavedChanges() { return this.busy() || this.uncertain() || JSON.stringify(this.draft()) !== JSON.stringify(this.detail()); }
  change<K extends keyof ScheduleInput>(field: K, value: ScheduleInput[K]) { this.draft.update(draft => draft ? { ...draft, [field]: value } : null); this.saved.set(false); }
  time(field: 'start' | 'end', local: string) { this.change(field, local ? { local, offsetMinutes: null } : null); }
  offset(field: 'start' | 'end', offsetMinutes: number | null) { const time = this.draft()?.[field]; if (time) this.change(field, { ...time, offsetMinutes }); }
  add() { this.editStage.emit({ id: crypto.randomUUID(), name: '', phase: '', screenType: 'information', content: '', resourceUrl: null, start: this.draft()?.start ?? null, end: this.draft()?.end ?? null }); }
  applyStage(stage: StageInput) { const stages = this.draft()?.stages ?? []; this.change('stages', stages.some(x => x.id === stage.id) ? stages.map(x => x.id === stage.id ? stage : x) : [...stages, stage]); }
  remove(id: string) { this.change('stages', this.draft()!.stages.filter(x => x.id !== id)); }
  reapply() { const current = this.conflict(); if (current) this.detail.set(current); this.conflict.set(null); this.error.set(''); this.operationId = crypto.randomUUID(); }
  useCurrent() { const current = this.conflict(); this.reapply(); if (current) this.draft.set(current); }
  ngOnInit() { void this.load(); }
  async load() {
    this.error.set('');
    try { const value = await this.service.get(this.eventId()); if (!this.destroy.destroyed) { this.detail.set(value); this.draft.set(value); } }
    catch (error) { if (error instanceof ScheduleFailure && error.status === 401) this.denied.emit(); else this.error.set('The schedule could not be loaded. Retry to continue.'); }
  }
  async save() {
    const draft = this.draft(), current = this.detail(); if (!draft || !current || this.busy() || this.conflict()) return;
    this.busy.set(true); this.error.set(''); this.saved.set(false);
    try {
      const value = await this.service.save(current.id, draft, current.version, this.operationId);
      if (this.destroy.destroyed) return;
      this.detail.set(value); this.draft.set(value); this.uncertain.set(false); this.saved.set(true); this.operationId = crypto.randomUUID();
    } catch (error) {
      if (error instanceof ScheduleFailure && error.status === 401) this.denied.emit();
      else if (error instanceof ScheduleFailure && error.code === 'stale-version' && error.current) {
        this.uncertain.set(false); this.conflict.set(error.current); this.error.set('Another administrator changed this event. Compare the latest schedule before reapplying your edits.');
      } else if (error instanceof ScheduleFailure && error.status >= 400 && error.status < 500) {
        this.uncertain.set(false); this.operationId = crypto.randomUUID();
        this.error.set(Object.entries(error.errors).map(([field, messages]) => `${field}: ${messages.join(' ')}`).join(' ') || 'The schedule was rejected. Check your access before retrying.');
      } else { this.uncertain.set(true); this.error.set('The save could not be confirmed. Retry to check its outcome.'); }
    } finally { this.busy.set(false); }
  }
}
