import { Provider } from '@angular/core';
import { SESSION_SERVICE, SessionService } from '@faithtech/api';

export const serviceProviders: Provider[] = [{ provide: SESSION_SERVICE, useClass: SessionService }];
