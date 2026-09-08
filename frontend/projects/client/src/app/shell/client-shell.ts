import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink, RouterOutlet } from '@angular/router';
import { ParticipantSessionPanel } from '@faithtech/domain';

@Component({ selector: 'ft-client-shell', imports: [RouterLink, RouterOutlet, ParticipantSessionPanel],
  templateUrl: './client-shell.html', styleUrl: './client-shell.css' })
export class ClientShell {
  private readonly router = inject(Router);
  readonly eventId = inject(ActivatedRoute).snapshot.paramMap.get('eventId')!;
  signedOut() { void this.router.navigate(['/events', this.eventId, 'access'], { replaceUrl: true }); }
}
