import { InjectionToken } from '@angular/core';
import { IEntryService } from './entry-service.contract';

export const ENTRY_SERVICE = new InjectionToken<IEntryService>('ENTRY_SERVICE');
