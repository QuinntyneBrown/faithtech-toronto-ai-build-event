import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { ActivatedRoute, Router, RouterLink, RouterOutlet } from '@angular/router';
import { ParticipantSessionPanel } from '@faithtech/domain';

@Component({ selector: 'ft-client-shell', imports: [RouterLink, RouterOutlet, ParticipantSessionPanel],
  templateUrl: './client-shell.html', styleUrl: './client-shell.css' })
export class ClientShell {
  private readonly router = inject(Router);
  // A signal, not a one-time snapshot: the shell's own route config doesn't change between two different
  // events, so Angular reuses this component instance across such a switch, and the nav links and session
  // panel below must follow the current :eventId rather than staying pinned to whichever one loaded first.
  readonly eventId = toSignal(inject(ActivatedRoute).paramMap.pipe(map(params => params.get('eventId')!)), { requireSync: true });
  signedOut() { void this.router.navigate(['/events', this.eventId(), 'access'], { replaceUrl: true }); }
}
