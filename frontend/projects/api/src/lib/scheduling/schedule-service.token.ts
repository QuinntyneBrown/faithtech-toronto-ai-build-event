import { InjectionToken } from '@angular/core';
import { IScheduleService } from './schedule-service.contract';

export const SCHEDULE_SERVICE = new InjectionToken<IScheduleService>('SCHEDULE_SERVICE');
