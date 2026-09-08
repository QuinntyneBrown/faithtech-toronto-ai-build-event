import { Provider } from '@angular/core';
import { ENTRY_SERVICE, PARTICIPANT_SESSION_SERVICE } from '@faithtech/api';
import { MockEntryService } from '../../../api/testing/mock-entry.service';
import { MockParticipantSessionService } from '../../../api/testing/mock-participant-session.service';

export const serviceProviders: Provider[] = [
  { provide: ENTRY_SERVICE, useClass: MockEntryService },
  { provide: PARTICIPANT_SESSION_SERVICE, useClass: MockParticipantSessionService },
];
