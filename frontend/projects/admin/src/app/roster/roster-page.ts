import { afterNextRender, Component, effect, ElementRef, inject, Injector, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { RosterIssuance } from '@faithtech/api';
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
  private readonly summary = viewChild<ElementRef<HTMLElement>>('summary');
  constructor() {
    effect(() => { if (!this.session()?.state()) { this.clearCode(); this.addDialog()?.nativeElement.close(); } });
    effect(() => { if (this.panel()?.error()) afterNextRender(() => this.summary()?.nativeElement.focus(), { injector: this.injector }); });
  }
  openAdd() { this.panel()?.beginAdd(); this.addDialog()?.nativeElement.showModal(); }
  cancelAdd() { if (!this.panel()?.busy()) this.addDialog()?.nativeElement.close(); }
  showCode(result: RosterIssuance) {
    this.addDialog()?.nativeElement.close();
    if (!this.session()?.state()) return;
    this.issuance.set(result);
    afterNextRender(() => { if (this.issuance() && this.session()?.state()) this.codeDialog()?.nativeElement.showModal(); }, { injector: this.injector });
  }
  clearCode() { this.codeDialog()?.nativeElement.close(); this.issuance.set(null); }
  signedOut() { this.clearCode(); this.addDialog()?.nativeElement.close(); void this.router.navigateByUrl('/sign-in', { replaceUrl: true }); }
}
