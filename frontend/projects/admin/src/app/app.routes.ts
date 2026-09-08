import { Routes } from '@angular/router';
import { SignInPage } from './access/sign-in-page';
import { SessionPage } from './access/session-page';
import { EventsPage } from './events/events-page';
import { EventEditorPage } from './events/event-editor-page';
import { eventEditorLeaveGuard } from './events/event-editor-leave.guard';

export const routes: Routes = [
  { path: 'sign-in', component: SignInPage },
  { path: 'session', component: SessionPage },
  { path: 'events', component: EventsPage },
  { path: 'events/:eventId', component: EventEditorPage, canDeactivate: [eventEditorLeaveGuard] },
  { path: '', pathMatch: 'full', redirectTo: 'sign-in' },
];
