import { Provider } from '@angular/core';
import { ENTRY_SERVICE, EntryService, PARTICIPANT_SESSION_SERVICE, ParticipantSessionService } from '@faithtech/api';

export const serviceProviders: Provider[] = [
  { provide: ENTRY_SERVICE, useClass: EntryService },
  { provide: PARTICIPANT_SESSION_SERVICE, useClass: ParticipantSessionService },
];
