import { Component, DestroyRef, ElementRef, inject, input, OnInit, output, signal, viewChild } from '@angular/core';
import { ROSTER_SERVICE, RosterEntry, RosterFailure, RosterIssuance } from '@faithtech/api';

@Component({ selector: 'ft-roster-panel', templateUrl: './roster-panel.html', styleUrl: './roster-panel.css' })
export class RosterPanel implements OnInit {
  private readonly service = inject(ROSTER_SERVICE);
  private readonly destroy = inject(DestroyRef);
  readonly eventId = input.required<string>();
  readonly requestAdd = output<void>();
  readonly issued = output<RosterIssuance>();
  readonly denied = output<void>();
  readonly entries = signal<RosterEntry[] | null>(null);
  readonly loadError = signal('');
  readonly name = signal('');
  readonly error = signal('');
  readonly fieldError = signal('');
  readonly busy = signal(false);
  readonly uncertain = signal(false);
  private operationId = crypto.randomUUID();
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
}
