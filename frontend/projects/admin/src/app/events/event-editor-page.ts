import { Component, ElementRef, HostListener, inject, viewChild } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AdministratorSessionPanel, EventEditor } from '@faithtech/domain';

@Component({ selector: 'ft-event-editor-page', imports: [AdministratorSessionPanel, EventEditor, RouterLink],
  templateUrl: './event-editor-page.html', styleUrl: './event-editor-page.css' })
export class EventEditorPage {
  private readonly router = inject(Router);
  readonly eventId = inject(ActivatedRoute).snapshot.paramMap.get('eventId')!;
  private readonly editor = viewChild(EventEditor);
  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('leaveDialog');
  private resolveLeave?: (leave: boolean) => void;
  canLeave(nextUrl: string): boolean | Promise<boolean> {
    if (nextUrl === '/sign-in') { this.finishLeave(false); return true; }
    if (!this.editor()?.hasUnsavedChanges()) return true;
    this.resolveLeave?.(false);
    this.dialog().nativeElement.showModal();
    return new Promise(resolve => { this.resolveLeave = resolve; });
  }
  finishLeave(leave: boolean) {
    this.dialog().nativeElement.close(); this.resolveLeave?.(leave); this.resolveLeave = undefined;
  }
  @HostListener('window:beforeunload', ['$event'])
  beforeUnload(event: BeforeUnloadEvent) { if (this.editor()?.hasUnsavedChanges()) event.preventDefault(); }
  signedOut() { void this.router.navigateByUrl('/sign-in', { replaceUrl: true }); }
}
