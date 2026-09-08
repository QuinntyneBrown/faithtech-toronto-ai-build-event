import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AdministratorSessionPanel } from '@faithtech/domain';

@Component({ selector: 'ft-session-page', imports: [AdministratorSessionPanel, RouterLink],
  templateUrl: './session-page.html', styleUrl: './session-page.css' })
export class SessionPage {
  private readonly router = inject(Router);
  signedOut() { void this.router.navigateByUrl('/sign-in', { replaceUrl: true }); }
}
