import { Routes } from '@angular/router';
import { CountdownPageComponent } from './pages/countdown/countdown-page.component';
import { ProjectsPageComponent } from './pages/projects/projects-page.component';
import { TeamsPageComponent } from './pages/teams/teams-page.component';
import { RafflePageComponent } from './pages/raffle/raffle-page.component';
export const routes: Routes = [{ path: 'countdown', component: CountdownPageComponent }, { path: 'projects', component: ProjectsPageComponent }, { path: 'teams', component: TeamsPageComponent }, { path: 'raffle', component: RafflePageComponent }, { path: '**', redirectTo: 'countdown' }];


