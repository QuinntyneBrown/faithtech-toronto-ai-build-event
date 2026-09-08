import { Provider } from '@angular/core';
import { SESSION_SERVICE, SessionService, EVENT_SERVICE, EventService } from '@faithtech/api';

export const serviceProviders: Provider[] = [
  { provide: SESSION_SERVICE, useClass: SessionService }, { provide: EVENT_SERVICE, useClass: EventService },
];
