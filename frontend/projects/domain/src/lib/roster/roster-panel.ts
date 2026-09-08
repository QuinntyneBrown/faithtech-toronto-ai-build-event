import { Component, DestroyRef, ElementRef, inject, input, OnInit, output, signal, viewChild } from '@angular/core';
import { ROSTER_SERVICE, RosterEntry, RosterFailure, RosterIssuance } from '@faithtech/api';

@Component({ selector: 'ft-roster-panel', templateUrl: './roster-panel.html', styleUrl: './roster-panel.css' })
export class RosterPanel implements OnInit {
  private readonly service = inject(ROSTER_SERVICE);
  private readonly destroy = inject(DestroyRef);
  readonly eventId = input.required<string>();
  readonly requestAdd = output<void>();
  readonly requestRename = output<RosterEntry>();
  readonly issued = output<RosterIssuance>();
  readonly renamed = output<RosterEntry>();
  readonly denied = output<void>();
  readonly entries = signal<RosterEntry[] | null>(null);
  readonly loadError = signal('');
  readonly name = signal('');
  readonly error = signal('');
  readonly fieldError = signal('');
  readonly busy = signal(false);
  readonly uncertain = signal(false);
  private operationId = crypto.randomUUID();
  readonly renameTarget = signal<RosterEntry | null>(null);
  readonly renameName = signal('');
  readonly renameError = signal('');
  readonly renameFieldError = signal('');
  readonly renameBusy = signal(false);
  private renameOperationId = crypto.randomUUID();
  private readonly addButton = viewChild<ElementRef<HTMLButtonElement>>('addButton');
  focusAdd() { this.addButton()?.nativeElement.focus(); }
  discardDraft() { if (!this.uncertain()) { this.name.set(''); this.error.set(''); this.fieldError.set(''); this.operationId = crypto.randomUUID(); } }
  beginAdd() { if (!this.name() && !this.uncertain()) this.discardDraft(); }
  ngOnInit() { void this.load(); }
  async load() {
    this.loadError.set('');
    try { const entries = await this.service.list(this.eventId()); if (!this.destroy.destroyed) this.entries.set(entries); }
    catch (error) { if (this.destroy.destroyed) return; if (error instanceof RosterFailure && error.status === 401) this.denied.emit(); else this.loadError.set('The roster could not be loaded. Retry to continue.'); }
  }
  async add() {
    if (this.busy()) return;
    this.busy.set(true); this.error.set(''); this.fieldError.set('');
    try {
      const result = await this.service.add(this.eventId(), { displayName: this.name() }, this.operationId);
      if (this.destroy.destroyed) return;
      this.entries.update(entries => [...(entries ?? []).filter(entry => entry.id !== result.entry.id), result.entry]);
      this.uncertain.set(false); this.name.set(''); this.operationId = crypto.randomUUID(); this.issued.emit(result);
    } catch (error) {
      if (this.destroy.destroyed) return;
      if (error instanceof RosterFailure && error.status === 401) this.denied.emit();
      else if (error instanceof RosterFailure && error.status >= 400 && error.status < 500) {
        this.uncertain.set(false); this.operationId = crypto.randomUUID();
        this.fieldError.set(error.errors['displayName']?.join(' ') ?? '');
        this.error.set(this.fieldError() ? 'Check the participant name. Your entry is retained.' : 'The addition was rejected. Check your access before retrying.');
      } else { this.uncertain.set(true); this.error.set('The addition could not be confirmed. Check its outcome before adding another participant.'); }
    } finally { this.busy.set(false); }
  }
  beginRename(entry: RosterEntry) {
    this.renameTarget.set(entry); this.renameName.set(entry.displayName);
    this.renameError.set(''); this.renameFieldError.set(''); this.renameOperationId = crypto.randomUUID();
  }
  cancelRename() { if (!this.renameBusy()) this.renameTarget.set(null); }
  async rename() {
    const target = this.renameTarget();
    if (!target || this.renameBusy()) return;
    this.renameBusy.set(true); this.renameError.set(''); this.renameFieldError.set('');
    try {
      const result = await this.service.rename(this.eventId(), target.id, this.renameName(), target.version, this.renameOperationId);
      this.entries.update(entries => (entries ?? []).map(entry => entry.id === result.id ? result : entry));
      this.renameTarget.set(null); this.renamed.emit(result);
    } catch (error) {
      if (error instanceof RosterFailure && error.status === 401) { this.denied.emit(); return; }
      if (error instanceof RosterFailure && (error.status === 409 || error.status === 428)) {
        this.renameError.set('This participant changed elsewhere. Close this dialog and reopen rename to try again.');
      } else if (error instanceof RosterFailure && error.status >= 400 && error.status < 500) {
        this.renameFieldError.set(error.errors['displayName']?.join(' ') ?? '');
        this.renameError.set(this.renameFieldError() ? 'Check the participant name. Your entry is retained.' : 'The rename was rejected. Check your access before retrying.');
      } else { this.renameError.set('The rename could not be confirmed. Retry to continue.'); }
    } finally { this.renameBusy.set(false); }
  }
}
