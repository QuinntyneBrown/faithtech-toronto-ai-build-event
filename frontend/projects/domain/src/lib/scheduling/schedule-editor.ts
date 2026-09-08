import { afterNextRender, Component, computed, DestroyRef, ElementRef, inject, Injector, input, OnInit, output, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SCHEDULE_SERVICE, ScheduleDetail, ScheduleFailure, ScheduleInput, StageInput } from '@faithtech/api';
import { WindowEditor } from './window-editor';

@Component({ selector: 'ft-schedule-editor', imports: [FormsModule, WindowEditor], templateUrl: './schedule-editor.html', styleUrl: './schedule-editor.css' })
export class ScheduleEditor implements OnInit {
  private readonly service = inject(SCHEDULE_SERVICE);
  private readonly destroy = inject(DestroyRef);
  private readonly injector = inject(Injector);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly summary = viewChild<ElementRef<HTMLElement>>('summary');
  readonly eventId = input.required<string>();
  readonly editStage = output<StageInput>();
  readonly editStageField = output<{ stage: StageInput; field: string }>();
  readonly denied = output<void>();
  readonly detail = signal<ScheduleDetail | null>(null);
  readonly draft = signal<ScheduleInput | null>(null);
  readonly error = signal('');
  readonly errors = signal<Record<string, string[]>>({});
  readonly issues = computed(() => Object.entries(this.errors()).map(([field, messages]) => ({ field, message: messages.join(' '), label: this.fieldLabel(field) })));
  readonly stageLabels: Record<string, string> = { name: 'Stage name', phase: 'Phase', start: 'Stage start', end: 'Stage end', screenType: 'Participant screen', content: 'Stage instructions', resourceUrl: 'Resource URL' };
  stageIssue(key: string) {
    const match = /^stage\.([0-9a-f-]+)\.(\w+)$/i.exec(key);
    const stage = match && this.draft()?.stages.find(x => x.id === match[1]);
    return stage && match ? { stage, field: match[2] in this.stageLabels ? match[2] : 'name' } : null;
  }
  stageError(id: string, field: string) { return this.errors()[`stage.${id}.${field}`]?.join(' '); }
  private retainErrors(errors: Record<string, string[]>, draft: ScheduleInput) {
    return Object.fromEntries(Object.entries(errors).map(([key, messages]) => {
      const match = /^stages\[(\d+)\](?:\.(\w+))?$/.exec(key), stage = match && draft.stages[Number(match[1])];
      return [stage && match ? `stage.${stage.id}.${match[2] && match[2] in this.stageLabels ? match[2] : 'name'}` : key, messages];
    }));
  }
  fieldLabel(field: string) {
    const issue = this.stageIssue(field);
    if (issue) return (issue.stage.name || 'Unnamed stage') + ' — ' + this.stageLabels[issue.field];
    const window = /^(selection|presentation)\.(start|end)$/.exec(field);
    if (window) return (window[1] === 'selection' ? 'Selection' : 'Demo presentation') + ' ' + window[2];
    return ({ timezone: 'Timezone', start: 'Event start', end: 'Event end', stages: 'Stages and content' } as Record<string, string>)[field] ?? 'Schedule configuration';
  }
  fieldTarget(field: string) {
    const issue = this.stageIssue(field); if (issue) return 'stage-' + issue.field;
    if (/^(selection|presentation)\.(start|end)$/.test(field)) return 'schedule-' + field.replace('.', '-');
    return ['timezone', 'start', 'end', 'stages'].includes(field) ? 'schedule-' + field : 'schedule-configuration';
  }
  focusField(event: Event, field: string) {
    event.preventDefault(); const issue = this.stageIssue(field);
    if (issue) this.editStageField.emit(issue);
    else this.host.nativeElement.querySelector<HTMLElement>('#' + this.fieldTarget(field))?.focus();
  }
  readonly busy = signal(false);
  readonly uncertain = signal(false);
  readonly saved = signal(false);
  readonly conflict = signal<ScheduleDetail | null>(null);
  private operationId = crypto.randomUUID();
  hasUnsavedChanges() { return this.busy() || this.uncertain() || JSON.stringify(this.draft()) !== JSON.stringify(this.detail()); }
  change<K extends keyof ScheduleInput>(field: K, value: ScheduleInput[K]) {
    this.draft.update(draft => draft ? { ...draft, [field]: value } : null); this.saved.set(false);
    if ((field === 'selection' || field === 'presentation') && value === null) this.clearErrors(field + '.');
  }
  private clearErrors(prefix: string) {
    if (!Object.keys(this.errors()).some(key => key.startsWith(prefix))) return;
    this.errors.update(errors => Object.fromEntries(Object.entries(errors).filter(([key]) => !key.startsWith(prefix))));
    if (!Object.keys(this.errors()).length) this.error.set('');
  }
  time(field: 'start' | 'end', local: string) { this.change(field, local ? { local, offsetMinutes: null } : null); }
  offset(field: 'start' | 'end', offsetMinutes: number | null) { const time = this.draft()?.[field]; if (time) this.change(field, { ...time, offsetMinutes }); }
  add() { this.editStage.emit({ id: crypto.randomUUID(), name: '', phase: '', screenType: 'information', content: '', resourceUrl: null, start: this.draft()?.start ?? null, end: this.draft()?.end ?? null }); }
  applyStage(stage: StageInput) { const stages = this.draft()?.stages ?? []; this.change('stages', stages.some(x => x.id === stage.id) ? stages.map(x => x.id === stage.id ? stage : x) : [...stages, stage]); }
  remove(id: string) {
    this.change('stages', this.draft()!.stages.filter(x => x.id !== id)); this.clearErrors(`stage.${id}.`);
    afterNextRender(() => this.host.nativeElement.querySelector<HTMLElement>('#schedule-stages')?.focus(), { injector: this.injector });
  }
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
    this.busy.set(true); this.error.set(''); this.errors.set({}); this.saved.set(false);
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
        this.errors.set(this.retainErrors(error.errors, draft));
        this.error.set(Object.keys(error.errors).length ? 'Check the fields below. Your changes are retained.' : 'The schedule was rejected. Check your access before retrying.');
        afterNextRender(() => this.summary()?.nativeElement.focus(), { injector: this.injector });
      } else { this.uncertain.set(true); this.error.set('The save could not be confirmed. Retry to check its outcome.'); }
    } finally { this.busy.set(false); }
  }
}
