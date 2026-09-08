import { CanDeactivateFn } from '@angular/router';
import { EventEditorPage } from './event-editor-page';

export const eventEditorLeaveGuard: CanDeactivateFn<EventEditorPage> = (page, _route, _current, next) => page.canLeave(next.url);
