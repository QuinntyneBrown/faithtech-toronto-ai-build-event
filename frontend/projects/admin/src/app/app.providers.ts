import { Provider } from '@angular/core';
import { SESSION_SERVICE, SessionService, EVENT_SERVICE, EventService, SCHEDULE_SERVICE, ScheduleService } from '@faithtech/api';

export const serviceProviders: Provider[] = [
  { provide: SESSION_SERVICE, useClass: SessionService }, { provide: EVENT_SERVICE, useClass: EventService },
  { provide: SCHEDULE_SERVICE, useClass: ScheduleService },
];
