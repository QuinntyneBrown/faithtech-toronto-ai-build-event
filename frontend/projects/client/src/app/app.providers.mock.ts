import { Provider } from '@angular/core';
import { ENTRY_SERVICE } from '@faithtech/api';
import { MockEntryService } from '../../../api/testing/mock-entry.service';

export const serviceProviders: Provider[] = [
  { provide: ENTRY_SERVICE, useClass: MockEntryService },
];
