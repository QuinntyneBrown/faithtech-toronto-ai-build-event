import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideCsTheme } from '@quinntyne/cornerstone';
import { EVENT_SERVICE } from './app/data/event-service.token';
import { MockEventService } from './app/data/mock-event.service';
bootstrapApplication(AppComponent, { providers: [provideCsTheme('light'), { provide: EVENT_SERVICE, useClass: MockEventService }] }).catch(console.error);
