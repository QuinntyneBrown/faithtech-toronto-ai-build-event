import { Provider } from '@angular/core';
import { SESSION_SERVICE } from '@faithtech/api';
import { MockSessionService } from '../../../api/testing/mock-session.service';

export const serviceProviders: Provider[] = [{ provide: SESSION_SERVICE, useClass: MockSessionService }];
