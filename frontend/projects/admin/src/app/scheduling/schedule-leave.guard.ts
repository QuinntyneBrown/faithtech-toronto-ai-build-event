import { CanDeactivateFn } from '@angular/router';
import { SchedulePage } from './schedule-page';

export const scheduleLeaveGuard: CanDeactivateFn<SchedulePage> = (page, _route, _state, next) => page.canLeave(next.url);
