export type Screen = "countdown" | "projects" | "teams" | "raffle";
export const SCREENS: Screen[] = ["countdown", "projects", "teams", "raffle"];
export const SCREEN_LABELS: Record<Screen, string> = {
  countdown: "Countdown",
  projects: "Projects",
  teams: "Team selection",
  raffle: "Raffle",
};
