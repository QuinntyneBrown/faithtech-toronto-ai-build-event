import { Provider } from '@angular/core';
import { SESSION_SERVICE, EVENT_SERVICE } from '@faithtech/api';
import { MockSessionService } from '../../../api/testing/mock-session.service';
import { MockEventService } from '../../../api/testing/mock-event.service';

export const serviceProviders: Provider[] = [
  { provide: SESSION_SERVICE, useClass: MockSessionService }, { provide: EVENT_SERVICE, useClass: MockEventService },
];
