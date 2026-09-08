import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({ selector: 'ft-stub-page', templateUrl: './stub-page.html', styleUrl: './stub-page.css' })
export class StubPage {
  readonly feature = (inject(ActivatedRoute).snapshot.data['feature'] as string | undefined) ?? 'This area';
}
