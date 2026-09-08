import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AdministratorSessionPanel, EventEditor } from '@faithtech/domain';

@Component({ selector: 'ft-event-editor-page', imports: [AdministratorSessionPanel, EventEditor, RouterLink],
  templateUrl: './event-editor-page.html', styleUrl: './event-editor-page.css' })
export class EventEditorPage {
  private readonly router = inject(Router);
  readonly eventId = inject(ActivatedRoute).snapshot.paramMap.get('eventId')!;
  signedOut() { void this.router.navigateByUrl('/sign-in', { replaceUrl: true }); }
}
