import { Provider } from '@angular/core';
import { ENTRY_SERVICE, EntryService } from '@faithtech/api';

export const serviceProviders: Provider[] = [
  { provide: ENTRY_SERVICE, useClass: EntryService },
];
