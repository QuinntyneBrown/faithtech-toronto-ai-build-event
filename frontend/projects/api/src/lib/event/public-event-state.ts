export interface PublicEventState {
  version: string;
  currentScreen: "countdown" | "projects" | "teams" | "raffle";
  title: string;
  welcome: string;
  purpose: string;
  venue: string;
  eventTime: string;
  countdownTargetUtc: string;
  projects: ProjectCard[];
}

export interface ProjectCard {
  id: string;
  title: string;
  description: string;
  repositoryUrl: string | null;
  demoUrl: string | null;
}
