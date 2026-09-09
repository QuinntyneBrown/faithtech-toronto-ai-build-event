import { RaffleSnapshot } from "../raffle/raffle-snapshot";

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
  teams: PublicTeam[];
  unassignedMembers: PublicTeamMember[];
  raffle: RaffleSnapshot;
}

export interface ProjectCard {
  id: string;
  title: string;
  description: string;
  repositoryUrl: string | null;
  demoUrl: string | null;
}

export interface PublicTeam {
  id: string;
  label: string;
  projectId: string | null;
  members: PublicTeamMember[];
}

export interface PublicTeamMember {
  id: string;
  label: string;
}
