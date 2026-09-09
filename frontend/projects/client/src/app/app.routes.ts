import { Routes } from "@angular/router";
import { CountdownPageComponent } from "./pages/countdown-page.component";

export const routes: Routes = [
  { path: "countdown", component: CountdownPageComponent },
  { path: "", pathMatch: "full", redirectTo: "countdown" },
  { path: "**", redirectTo: "countdown" }
];
