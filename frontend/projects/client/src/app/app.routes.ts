import { Routes } from "@angular/router";
import { CountdownPageComponent } from "./pages/countdown-page.component";
import { ProjectsPageComponent } from "./pages/projects-page.component";
import { TeamsPageComponent } from "./pages/teams-page.component";

export const routes: Routes = [
  { path: "countdown", component: CountdownPageComponent },
  { path: "projects", component: ProjectsPageComponent },
  { path: "teams", component: TeamsPageComponent },
  { path: "", pathMatch: "full", redirectTo: "countdown" },
  { path: "**", redirectTo: "countdown" }
];
