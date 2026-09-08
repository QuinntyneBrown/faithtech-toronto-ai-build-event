import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AdministratorAccessForm } from '@faithtech/domain';

@Component({ selector: 'ft-sign-in-page', imports: [AdministratorAccessForm],
  templateUrl: './sign-in-page.html', styleUrl: './sign-in-page.css' })
export class SignInPage {
  private readonly router = inject(Router);
  signedIn() { void this.router.navigateByUrl('/session'); }
}
