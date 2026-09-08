import { Component, DestroyRef, ElementRef, inject, input, OnInit, output, signal, viewChild } from '@angular/core';
import { ROSTER_SERVICE, RosterEntry, RosterFailure, RosterIssuance } from '@faithtech/api';

@Component({ selector: 'ft-roster-panel', templateUrl: './roster-panel.html', styleUrl: './roster-panel.css' })
export class RosterPanel implements OnInit {
  private readonly service = inject(ROSTER_SERVICE);
  private readonly destroy = inject(DestroyRef);
  readonly eventId = input.required<string>();
  readonly requestAdd = output<void>();
  readonly requestRename = output<RosterEntry>();
  readonly requestDeactivate = output<RosterEntry>();
  readonly issued = output<RosterIssuance>();
  readonly renamed = output<RosterEntry>();
  readonly deactivated = output<RosterEntry>();
  readonly codeReplaced = output<RosterIssuance>();
  readonly reactivated = output<RosterEntry>();
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
  readonly deactivateTarget = signal<RosterEntry | null>(null);
  readonly deactivateError = signal('');
  readonly deactivateBusy = signal(false);
  private deactivateOperationId = crypto.randomUUID();
  readonly replaceCodeError = signal('');
  readonly replaceCodeBusy = signal(false);
  private replaceCodeOperationId = crypto.randomUUID();
  readonly reactivateError = signal('');
  readonly reactivateBusyId = signal<string | null>(null);
  private reactivateOperationId = crypto.randomUUID();
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
  beginDeactivate(entry: RosterEntry) {
    this.deactivateTarget.set(entry); this.deactivateError.set(''); this.deactivateOperationId = crypto.randomUUID();
  }
  cancelDeactivate() { if (!this.deactivateBusy()) this.deactivateTarget.set(null); }
  async deactivate() {
    const target = this.deactivateTarget();
    if (!target || this.deactivateBusy()) return;
    this.deactivateBusy.set(true); this.deactivateError.set('');
    try {
      const result = await this.service.deactivate(this.eventId(), target.id, target.version, this.deactivateOperationId);
      this.entries.update(entries => (entries ?? []).map(entry => entry.id === result.id ? result : entry));
      this.deactivateTarget.set(null); this.deactivated.emit(result);
    } catch (error) {
      if (error instanceof RosterFailure && error.status === 401) { this.denied.emit(); return; }
      if (error instanceof RosterFailure && (error.status === 409 || error.status === 428)) {
        this.deactivateError.set('This participant changed elsewhere. Close this dialog and reopen deactivate to try again.');
      } else { this.deactivateError.set('The deactivation could not be confirmed. Retry to continue.'); }
    } finally { this.deactivateBusy.set(false); }
  }
  async replaceCode(registrationId: string, version: string) {
    if (this.replaceCodeBusy()) return;
    this.replaceCodeBusy.set(true); this.replaceCodeError.set('');
    try {
      const result = await this.service.replaceCode(this.eventId(), registrationId, version, this.replaceCodeOperationId);
      this.entries.update(entries => (entries ?? []).map(entry => entry.id === result.entry.id ? result.entry : entry));
      this.replaceCodeOperationId = crypto.randomUUID();
      this.codeReplaced.emit(result);
    } catch (error) {
      if (error instanceof RosterFailure && error.status === 401) { this.denied.emit(); return; }
      if (error instanceof RosterFailure && (error.status === 409 || error.status === 428)) {
        this.replaceCodeError.set('This participant changed elsewhere. Close this dialog and try again.');
      } else { this.replaceCodeError.set('The code could not be replaced. Retry to continue.'); }
    } finally { this.replaceCodeBusy.set(false); }
  }
  async reactivate(entry: RosterEntry) {
    if (this.reactivateBusyId()) return;
    this.reactivateBusyId.set(entry.id); this.reactivateError.set('');
    try {
      const result = await this.service.reactivate(this.eventId(), entry.id, entry.version, this.reactivateOperationId);
      this.entries.update(entries => (entries ?? []).map(x => x.id === result.id ? result : x));
      this.reactivateOperationId = crypto.randomUUID();
      this.reactivated.emit(result);
    } catch (error) {
      if (error instanceof RosterFailure && error.status === 401) { this.denied.emit(); return; }
      this.reactivateOperationId = crypto.randomUUID();
      if (error instanceof RosterFailure && error.status === 422) {
        this.reactivateError.set(error.errors['email']?.join(' ') || 'This entry cannot be reactivated until its email binding is resolved.');
      } else if (error instanceof RosterFailure && (error.status === 409 || error.status === 428)) {
        this.reactivateError.set('This participant changed elsewhere. Retry to reactivate.');
      } else { this.reactivateError.set('The reactivation could not be confirmed. Retry to continue.'); }
    } finally { this.reactivateBusyId.set(null); }
  }
}
