import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CardComponent, CsButtonDirective } from '@quinntyne/cornerstone';
@Component({selector: 'mock-app', imports: [CardComponent, CsButtonDirective], templateUrl: './app.component.html', styleUrl: './app.component.scss', changeDetection: ChangeDetectionStrategy.OnPush})
export class AppComponent {}
