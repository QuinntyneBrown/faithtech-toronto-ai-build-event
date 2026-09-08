import { Routes } from '@angular/router';
import { StubPage } from '@faithtech/components';
import { EnterEventPage } from './access/enter-event-page';
import { sessionGuard } from './access/session-guard';
import { ClientShell } from './shell/client-shell';
import { CurrentEventPage } from './event/current-event-page';

export const routes: Routes = [
  { path: 'events/:eventId/access', component: EnterEventPage },
  {
    path: 'events/:eventId', component: ClientShell, canActivate: [sessionGuard],
    children: [
      { path: '', component: CurrentEventPage },
      { path: 'schedule', component: StubPage, data: { feature: 'Schedule' } },
      { path: 'teams', component: StubPage, data: { feature: 'Teams and projects' } },
      { path: 'people', component: StubPage, data: { feature: 'People' } },
      { path: 'messages', component: StubPage, data: { feature: 'Messages' } },
      { path: 'quiz', component: StubPage, data: { feature: 'Quiz' } },
      { path: 'raffle', component: StubPage, data: { feature: 'Raffle' } },
      { path: 'showcase', component: StubPage, data: { feature: 'Showcase' } },
    ],
  },
];
