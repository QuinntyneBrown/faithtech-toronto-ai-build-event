import { CanDeactivateFn } from '@angular/router';
import { RosterPage } from './roster-page';

export const rosterLeaveGuard: CanDeactivateFn<RosterPage> = (component, _route, _current, next) => component.canLeave(next.url);
