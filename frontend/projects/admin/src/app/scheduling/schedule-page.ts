import { afterNextRender, Component, ElementRef, HostListener, inject, Injector, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { StageInput } from '@faithtech/api';
import { AdministratorSessionPanel, ScheduleEditor } from '@faithtech/domain';

@Component({ selector: 'ft-schedule-page', imports: [FormsModule, RouterLink, AdministratorSessionPanel, ScheduleEditor], templateUrl: './schedule-page.html', styleUrl: './schedule-page.css' })
export class SchedulePage {
  private readonly router = inject(Router);
  private readonly injector = inject(Injector);
  readonly eventId = inject(ActivatedRoute).snapshot.paramMap.get('eventId')!;
  readonly editor = viewChild.required(ScheduleEditor);
  readonly editing = signal<StageInput | null>(null);
  private readonly stageDialog = viewChild.required<ElementRef<HTMLDialogElement>>('stageDialog');
  private readonly stageForm = viewChild<ElementRef<HTMLFormElement>>('stageForm');
  private readonly leaveDialog = viewChild.required<ElementRef<HTMLDialogElement>>('leaveDialog');
  private originalStage = '';
  private discardStage = false;
  private resolveLeave?: (leave: boolean) => void;
  stageError(field: string) { const stage = this.editing(); return stage ? this.editor().stageError(stage.id, field) : undefined; }
  openStage(stage: StageInput, field = 'name') {
    this.originalStage = JSON.stringify(stage); this.editing.set(structuredClone(stage));
    afterNextRender(() => {
      if (this.editing()) { this.stageDialog().nativeElement.showModal(); this.stageForm()?.nativeElement.querySelector<HTMLElement>('#stage-' + field)?.focus(); }
    }, { injector: this.injector });
  }
  change<K extends keyof StageInput>(field: K, value: StageInput[K]) { this.editing.update(stage => stage ? { ...stage, [field]: value } : null); }
  time(field: 'start' | 'end', local: string) { this.change(field, local ? { local, offsetMinutes: null } : null); }
  offset(field: 'start' | 'end', offsetMinutes: number | null) { const time = this.editing()?.[field]; if (time) this.change(field, { ...time, offsetMinutes }); }
  applyStage() { const stage = this.editing(); if (stage) this.editor().applyStage(stage); this.closeStage(); }
  closeStage() { this.stageDialog().nativeElement.close(); this.editing.set(null); }
  cancelStage() {
    if (JSON.stringify(this.editing()) === this.originalStage) this.closeStage();
    else { this.discardStage = true; this.leaveDialog().nativeElement.showModal(); }
  }
  canLeave(nextUrl: string): boolean | Promise<boolean> {
    if (nextUrl === '/sign-in') { this.closeStage(); this.finishLeave(false); return true; }
    if (!this.editor().hasUnsavedChanges() && !this.editing()) return true;
    this.resolveLeave?.(false); this.discardStage = false; this.leaveDialog().nativeElement.showModal();
    return new Promise(resolve => { this.resolveLeave = resolve; });
  }
  finishLeave(leave: boolean) { this.leaveDialog().nativeElement.close(); if (leave && this.discardStage) this.closeStage(); this.resolveLeave?.(leave); this.resolveLeave = undefined; }
  signedOut() { void this.router.navigateByUrl('/sign-in', { replaceUrl: true }); }
  @HostListener('window:beforeunload', ['$event'])
  beforeUnload(event: BeforeUnloadEvent) { if (this.editor().hasUnsavedChanges() || this.editing()) event.preventDefault(); }
}
