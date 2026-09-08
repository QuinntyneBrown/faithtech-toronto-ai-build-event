import { Provider } from '@angular/core';
import { SESSION_SERVICE, EVENT_SERVICE, SCHEDULE_SERVICE } from '@faithtech/api';
import { MockSessionService } from '../../../api/testing/mock-session.service';
import { MockEventService } from '../../../api/testing/mock-event.service';
import { MockScheduleService } from '../../../api/testing/mock-schedule.service';

export const serviceProviders: Provider[] = [
  { provide: SESSION_SERVICE, useClass: MockSessionService }, { provide: EVENT_SERVICE, useClass: MockEventService },
  { provide: SCHEDULE_SERVICE, useClass: MockScheduleService },
];
