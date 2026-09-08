import { afterNextRender, Component, effect, ElementRef, HostListener, inject, Injector, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { RosterEntry, RosterIssuance } from '@faithtech/api';
import { AdministratorSessionPanel, RosterPanel } from '@faithtech/domain';

@Component({ selector: 'ft-roster-page', imports: [FormsModule, RouterLink, AdministratorSessionPanel, RosterPanel], templateUrl: './roster-page.html', styleUrl: './roster-page.css' })
export class RosterPage {
  private readonly router = inject(Router);
  private readonly injector = inject(Injector);
  readonly eventId = inject(ActivatedRoute).snapshot.paramMap.get('eventId')!;
  readonly panel = viewChild(RosterPanel);
  readonly session = viewChild(AdministratorSessionPanel);
  readonly issuance = signal<RosterIssuance | null>(null);
  private readonly addDialog = viewChild<ElementRef<HTMLDialogElement>>('addDialog');
  private readonly codeDialog = viewChild<ElementRef<HTMLDialogElement>>('codeDialog');
  private readonly codeInput = viewChild<ElementRef<HTMLInputElement>>('codeInput');
  private readonly summary = viewChild<ElementRef<HTMLElement>>('summary');
  private readonly leaveDialog = viewChild<ElementRef<HTMLDialogElement>>('leaveDialog');
  private readonly renameDialog = viewChild<ElementRef<HTMLDialogElement>>('renameDialog');
  private readonly deactivateDialog = viewChild<ElementRef<HTMLDialogElement>>('deactivateDialog');
  private closeOnly = false;
  private resolveLeave?: (leave: boolean) => void;
  constructor() {
    effect(() => { if (!this.session()?.state()) { this.clearCode(); this.addDialog()?.nativeElement.close(); this.renameDialog()?.nativeElement.close(); this.deactivateDialog()?.nativeElement.close(); } });
    effect(() => { if (this.panel()?.error()) afterNextRender(() => this.summary()?.nativeElement.focus(), { injector: this.injector }); });
  }
  openAdd() { this.panel()?.beginAdd(); this.addDialog()?.nativeElement.showModal(); }
  cancelAdd() {
    if (this.panel()?.busy()) return;
    if (this.panel()?.name() || this.panel()?.uncertain()) { this.closeOnly = true; this.leaveDialog()?.nativeElement.showModal(); }
    else this.addDialog()?.nativeElement.close();
  }
  hasPendingChanges() { return !!this.panel()?.name() || this.panel()?.busy() || this.panel()?.uncertain() || !!this.issuance()?.code; }
  canLeave(nextUrl: string): boolean | Promise<boolean> {
    if (nextUrl === '/sign-in') { this.finishLeave(false); return true; }
    if (!this.hasPendingChanges()) return true;
    this.closeOnly = false; this.resolveLeave?.(false); this.leaveDialog()?.nativeElement.showModal();
    return new Promise(resolve => { this.resolveLeave = resolve; });
  }
  finishLeave(leave: boolean) {
    this.leaveDialog()?.nativeElement.close();
    if (leave && this.closeOnly) { this.panel()?.discardDraft(); this.addDialog()?.nativeElement.close(); this.restoreFocus(); }
    this.resolveLeave?.(leave); this.resolveLeave = undefined;
  }
  private restoreFocus() { afterNextRender(() => { if (this.session()?.state()) this.panel()?.focusAdd(); }, { injector: this.injector }); }
  showCode(result: RosterIssuance) {
    this.addDialog()?.nativeElement.close();
    if (!this.session()?.state()) return;
    this.issuance.set(result);
    afterNextRender(() => { if (this.issuance() && this.session()?.state()) this.codeDialog()?.nativeElement.showModal(); }, { injector: this.injector });
  }
  clearCode() { const visible = !!this.issuance(); this.codeDialog()?.nativeElement.close(); this.issuance.set(null); if (visible) this.restoreFocus(); }
  codeReplaced(result: RosterIssuance) {
    this.issuance.set(result);
    // The dialog is already open, so the native autofocus algorithm (which only runs when a <dialog> is shown) never
    // applies to this freshly rendered input; focus it explicitly once the view reflects the new issuance.
    afterNextRender(() => this.codeInput()?.nativeElement.focus(), { injector: this.injector });
  }
  openRename(entry: RosterEntry) { this.panel()?.beginRename(entry); this.renameDialog()?.nativeElement.showModal(); }
  cancelRename() { if (this.panel()?.renameBusy()) return; this.panel()?.cancelRename(); this.renameDialog()?.nativeElement.close(); }
  renamed(_entry: RosterEntry) { this.renameDialog()?.nativeElement.close(); }
  openDeactivate(entry: RosterEntry) { this.panel()?.beginDeactivate(entry); this.deactivateDialog()?.nativeElement.showModal(); }
  cancelDeactivate() { if (this.panel()?.deactivateBusy()) return; this.panel()?.cancelDeactivate(); this.deactivateDialog()?.nativeElement.close(); }
  deactivated(_entry: RosterEntry) { this.deactivateDialog()?.nativeElement.close(); }
  signedOut() {
    this.clearCode(); this.addDialog()?.nativeElement.close(); this.renameDialog()?.nativeElement.close(); this.deactivateDialog()?.nativeElement.close();
    void this.router.navigateByUrl('/sign-in', { replaceUrl: true });
  }
  @HostListener('window:beforeunload', ['$event'])
  beforeUnload(event: BeforeUnloadEvent) { if (this.hasPendingChanges()) event.preventDefault(); }
}
