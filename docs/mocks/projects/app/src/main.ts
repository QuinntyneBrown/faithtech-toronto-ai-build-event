import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideCsTheme } from '@quinntyne/cornerstone';
import { EVENT_SERVICE } from './app/data/event-service.token';
import { MockEventService } from './app/data/mock-event.service';
import { provideRouter } from '@angular/router';
import { routes } from './app/app.routes';
bootstrapApplication(AppComponent, { providers: [provideCsTheme('light'), provideRouter(routes), { provide: EVENT_SERVICE, useClass: MockEventService }] }).catch(console.error);
