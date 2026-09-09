import type {
  AdministratorParticipant,
  ProjectCard,
  PublicEventState,
  PublicTeam,
  RaffleResult,
  RaffleSnapshot
} from "../../frontend/projects/api/src/public-api";
import type { BrowserSession } from "./browser-session";

export type Screen = PublicEventState["currentScreen"];

/** The passcode the mocked deployment has been provisioned with. */
export const ADMINISTRATOR_PASSCODE = "0042";

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export interface StoredParticipant {
  id: string;
  email: string;
  publicLabel: string;
  name: string | null;
  whatYouMake: string | null;
  onYourHeart: string | null;
  teamId: string | null;
  hasWonRaffle: boolean;
}

export interface StoredTeam {
  id: string;
  label: string;
  projectId: string | null;
}

export interface StoredProject {
  id: string;
  title: string;
  description: string;
  repositoryUrl: string | null;
  demoUrl: string | null;
}

export interface MockResponse {
  status: number;
  json?: unknown;
}

/** A one-shot outcome forced onto the next matching request. */
export interface ForcedOutcome {
  status?: number;
  json?: unknown;
  abort?: boolean;
  headers?: Record<string, string>;
}

/**
 * An in-memory stand-in for the API. It owns the version counter, so every
 * mutation behaves the way the real service does: commit, bump, and let the
 * client re-read the public state.
 */
export class EventStore {
  version = 1;
  currentScreen: Screen = "countdown";
  title = "RTR - Reconciliation Through Relationships";
  welcome = "Welcome to AI Build Night.";
  purpose = "Build something tonight that serves the Church.";
  venue = "FaithTech Toronto";
  eventTime = "Wednesday 9 September 2026, 5:20 PM";
  countdownTargetUtc = new Date(Date.now() + 90_000).toISOString();

  participants: StoredParticipant[] = [];
  teams: StoredTeam[] = [];
  projects: StoredProject[] = [];
  latestResult: RaffleResult | null = null;
  previousWinners: RaffleResult[] = [];

  teamsFormed = false;

  /** Set to null to model a deployment with no passcode provisioned at all. */
  provisionedPasscode: string | null = ADMINISTRATOR_PASSCODE;

  /** Milliseconds added to the wall clock for every server time answer. */
  serverClockOffsetMs = 0;

  /** How long a draw cycles names before the winner is revealed. */
  drawRevealDelayMs = 4_000;

  /** How long the celebration runs after the reveal. */
  drawEffectsDurationMs = 5_000;

  private nextParticipantLabel = 1;
  private nextTeamLabel = 1;
  private sequence = 0;
  private readonly forced = new Map<string, ForcedOutcome[]>();
  private readonly persistent = new Map<string, ForcedOutcome>();

  readonly requests: { method: string; path: string; body: unknown }[] = [];

  // ---------------------------------------------------------------- seeding

  identifier(prefix: string): string {
    this.sequence += 1;
    const tail = String(this.sequence).padStart(4, "0");
    return prefix + "-" + tail + "-4000-8000-000000000000";
  }

  addParticipant(email: string, overrides: Partial<StoredParticipant> = {}): StoredParticipant {
    const participant: StoredParticipant = {
      id: this.identifier("11111111"),
      email,
      publicLabel: "Participant " + String(this.nextParticipantLabel).padStart(3, "0"),
      name: null,
      whatYouMake: null,
      onYourHeart: null,
      teamId: null,
      hasWonRaffle: false,
      ...overrides
    };
    this.nextParticipantLabel += 1;
    this.participants.push(participant);
    return participant;
  }

  addProject(project: Omit<StoredProject, "id"> & { id?: string }): StoredProject {
    const stored: StoredProject = { ...project, id: project.id ?? this.identifier("22222222") };
    this.projects.push(stored);
    return stored;
  }

  addTeam(memberIds: string[] = [], projectId: string | null = null): StoredTeam {
    const team: StoredTeam = {
      id: this.identifier("33333333"),
      label: "Team " + String(this.nextTeamLabel).padStart(2, "0"),
      projectId
    };
    this.nextTeamLabel += 1;
    this.teams.push(team);
    for (const id of memberIds) {
      const participant = this.participants.find(candidate => candidate.id === id);
      if (participant) participant.teamId = team.id;
    }
    return team;
  }

  /** Groups every participant into teams of three, remainder last. */
  formTeams(): void {
    if (this.teamsFormed) return;
    this.teamsFormed = true;
    const eligible = [...this.participants];
    for (let index = 0; index < eligible.length; index += 3) {
      this.addTeam(eligible.slice(index, index + 3).map(participant => participant.id));
    }
  }

  // ------------------------------------------------------- failure control

  /** Forces the next request matching "METHOD /path" to produce this outcome. */
  failNext(key: string, outcome: ForcedOutcome): void {
    const queued = this.forced.get(key) ?? [];
    queued.push(outcome);
    this.forced.set(key, queued);
  }

  /**
   * Forces every request matching "METHOD /path" until cleared. Needed where
   * the client retries on its own — server time is sampled three times per
   * refresh, so a single queued failure would not keep the clock unsynchronized.
   */
  failAlways(key: string, outcome: ForcedOutcome): void {
    this.persistent.set(key, outcome);
  }

  clearFailure(key: string): void {
    this.persistent.delete(key);
    this.forced.delete(key);
  }

  consumeForced(key: string): ForcedOutcome | undefined {
    const queued = this.forced.get(key);
    if (queued?.length) {
      const outcome = queued.shift();
      if (!queued.length) this.forced.delete(key);
      return outcome;
    }
    return this.persistent.get(key);
  }

  // ------------------------------------------------------------ projections

  publicState(): PublicEventState {
    const teams: PublicTeam[] = this.teams.map(team => ({
      id: team.id,
      label: team.label,
      projectId: team.projectId,
      members: this.participants
        .filter(participant => participant.teamId === team.id)
        .sort((left, right) => left.publicLabel.localeCompare(right.publicLabel))
        .map(participant => participant.name ?? participant.publicLabel)
    }));

    const projects: ProjectCard[] = this.projects.map(project => ({ ...project }));

    return {
      version: String(this.version),
      currentScreen: this.currentScreen,
      title: this.title,
      welcome: this.welcome,
      purpose: this.purpose,
      venue: this.venue,
      eventTime: this.eventTime,
      countdownTargetUtc: this.countdownTargetUtc,
      projects,
      teams,
      raffle: this.raffleSnapshot()
    };
  }

  raffleSnapshot(): RaffleSnapshot {
    return {
      eligibleCount: this.participants.filter(participant => !participant.hasWonRaffle).length,
      latestResult: this.latestResult,
      previousWinners: this.previousWinners
    };
  }

  administratorParticipants(): AdministratorParticipant[] {
    return this.participants.map(participant => ({
      id: participant.id,
      email: participant.email,
      publicLabel: participant.publicLabel,
      name: participant.name,
      whatYouMake: participant.whatYouMake,
      onYourHeart: participant.onYourHeart,
      teamLabel: this.teams.find(team => team.id === participant.teamId)?.label ?? null,
      hasWonRaffle: participant.hasWonRaffle
    }));
  }

  serverTimeUtc(): string {
    return new Date(Date.now() + this.serverClockOffsetMs).toISOString();
  }

  // ---------------------------------------------------------------- routing

  handle(method: string, path: string, body: unknown, session: BrowserSession): MockResponse {
    this.requests.push({ method, path, body });
    const payload = (body ?? {}) as Record<string, any>;

    switch (method + " " + path) {
      case "GET /api/event/state":
        return { status: 200, json: this.publicState() };
      case "GET /api/event/time":
        return { status: 200, json: { serverTimeUtc: this.serverTimeUtc() } };
      case "GET /api/event/raffle":
        return { status: 200, json: this.raffleSnapshot() };

      case "POST /api/participant/entry-receipt":
        return { status: 200, json: { operationId: this.identifier("44444444") } };
      case "POST /api/participant/entries":
        return this.enter(String(payload.input?.email ?? ""), session);
      case "GET /api/participant/session":
        return this.participantSession(session);
      case "DELETE /api/participant/session":
        session.participantId = null;
        return { status: 204 };
      case "GET /api/participant/profile":
        return this.readProfile(session);
      case "PUT /api/participant/profile":
        return this.saveProfile(payload.input ?? {}, session);

      case "GET /api/admin/session":
        return { status: 200, json: { authenticated: session.administratorAuthenticated } };
      case "POST /api/admin/session":
        return this.signIn(String(payload.passcode ?? ""), session);
      case "DELETE /api/admin/session":
        session.administratorAuthenticated = false;
        return { status: 204 };
      case "POST /api/admin/session/interaction":
        return session.administratorAuthenticated ? { status: 204 } : { status: 401 };

      case "GET /api/admin/participants":
        return session.administratorAuthenticated
          ? { status: 200, json: this.administratorParticipants() }
          : { status: 401 };
      case "POST /api/admin/participants":
        return this.addParticipantAsAdministrator(String(payload.email ?? ""), session);

      case "POST /api/admin/projects":
        return this.createProject(payload.input ?? {}, session);
      case "POST /api/admin/event/advance":
        return this.advance(String(payload.toScreen ?? ""), String(payload.expectedVersion ?? ""), session);
      case "POST /api/admin/raffle/draws":
        return this.draw(session);
      case "POST /api/admin/teams/moves":
        return this.moveMember(payload, session);
    }

    const participantMatch = /^\/api\/admin\/participants\/([^/]+)$/.exec(path);
    if (participantMatch) {
      if (method === "PUT") return this.updateParticipant(participantMatch[1], payload.input ?? {}, session);
      if (method === "DELETE") return this.removeParticipant(participantMatch[1], session);
    }

    const projectMatch = /^\/api\/admin\/projects\/([^/]+)$/.exec(path);
    if (projectMatch) {
      if (method === "PUT") return this.updateProject(projectMatch[1], payload.input ?? {}, session);
      if (method === "DELETE") return this.removeProject(projectMatch[1], session);
    }

    const teamProjectMatch = /^\/api\/admin\/teams\/([^/]+)\/project$/.exec(path);
    if (teamProjectMatch && method === "PUT") {
      return this.assignProject(teamProjectMatch[1], payload.projectId ?? null, session);
    }

    return { status: 404 };
  }

  // --------------------------------------------------------------- handlers

  private requireAdministrator(session: BrowserSession): MockResponse | null {
    return session.administratorAuthenticated ? null : { status: 401 };
  }

  private enter(rawEmail: string, session: BrowserSession): MockResponse {
    const email = rawEmail.trim();
    if (!EMAIL_PATTERN.test(email) || email.length > 254) return { status: 422 };
    if (this.currentScreen !== "countdown") return { status: 409 };

    const normalized = email.toUpperCase();
    const existing = this.participants.find(participant => participant.email.toUpperCase() === normalized);
    if (existing) {
      if (session.participantId === existing.id) {
        return { status: 200, json: { participantId: existing.id, publicLabel: existing.publicLabel } };
      }
      return { status: 409 };
    }

    const participant = this.addParticipant(email);
    session.participantId = participant.id;
    this.version += 1;
    return { status: 200, json: { participantId: participant.id, publicLabel: participant.publicLabel } };
  }

  private participantSession(session: BrowserSession): MockResponse {
    const participant = this.participants.find(candidate => candidate.id === session.participantId);
    return participant
      ? { status: 200, json: { participantId: participant.id, publicLabel: participant.publicLabel } }
      : { status: 401 };
  }

  private readProfile(session: BrowserSession): MockResponse {
    const participant = this.participants.find(candidate => candidate.id === session.participantId);
    if (!participant) return { status: 401 };
    return {
      status: 200,
      json: {
        name: participant.name,
        whatYouMake: participant.whatYouMake,
        onYourHeart: participant.onYourHeart
      }
    };
  }

  private saveProfile(input: Record<string, any>, session: BrowserSession): MockResponse {
    const participant = this.participants.find(candidate => candidate.id === session.participantId);
    if (!participant) return { status: 401 };
    if (String(input.name ?? "").length > 200) return { status: 422 };
    participant.name = blankToNull(input.name);
    participant.whatYouMake = blankToNull(input.whatYouMake);
    participant.onYourHeart = blankToNull(input.onYourHeart);
    this.version += 1;
    return {
      status: 200,
      json: {
        name: participant.name,
        whatYouMake: participant.whatYouMake,
        onYourHeart: participant.onYourHeart
      }
    };
  }

  private signIn(passcode: string, session: BrowserSession): MockResponse {
    if (this.provisionedPasscode === null || passcode !== this.provisionedPasscode) {
      return { status: 401 };
    }
    session.administratorAuthenticated = true;
    return { status: 200, json: { authenticated: true } };
  }

  private addParticipantAsAdministrator(rawEmail: string, session: BrowserSession): MockResponse {
    const denied = this.requireAdministrator(session);
    if (denied) return denied;

    const email = rawEmail.trim();
    if (!EMAIL_PATTERN.test(email) || email.length > 254) return { status: 422 };
    const normalized = email.toUpperCase();
    if (this.participants.some(participant => participant.email.toUpperCase() === normalized)) {
      return { status: 409 };
    }

    const participant = this.addParticipant(email);
    this.version += 1;
    return {
      status: 200,
      json: this.administratorParticipants().find(candidate => candidate.id === participant.id)
    };
  }

  private updateParticipant(id: string, input: Record<string, any>, session: BrowserSession): MockResponse {
    const denied = this.requireAdministrator(session);
    if (denied) return denied;

    const participant = this.participants.find(candidate => candidate.id === id);
    if (!participant) return { status: 404 };

    const email = String(input.email ?? "").trim();
    if (!EMAIL_PATTERN.test(email) || email.length > 254) return { status: 422 };
    const normalized = email.toUpperCase();
    if (this.participants.some(other => other.id !== id && other.email.toUpperCase() === normalized)) {
      return { status: 409 };
    }

    participant.email = email;
    participant.name = blankToNull(input.name);
    participant.whatYouMake = blankToNull(input.whatYouMake);
    participant.onYourHeart = blankToNull(input.onYourHeart);
    this.version += 1;
    return { status: 200, json: this.administratorParticipants().find(candidate => candidate.id === id) };
  }

  private removeParticipant(id: string, session: BrowserSession): MockResponse {
    const denied = this.requireAdministrator(session);
    if (denied) return denied;

    const index = this.participants.findIndex(candidate => candidate.id === id);
    if (index < 0) return { status: 404 };

    const removed = this.participants.splice(index, 1)[0];
    if (session.participantId === removed.id) session.participantId = null;
    for (const result of [this.latestResult, ...this.previousWinners]) {
      if (result && result.winnerLabel === (removed.name ?? removed.publicLabel)) {
        result.winnerLabel = "Removed participant";
      }
    }
    this.version += 1;
    return { status: 204 };
  }

  private createProject(input: Record<string, any>, session: BrowserSession): MockResponse {
    const denied = this.requireAdministrator(session);
    if (denied) return denied;
    const invalid = validateProject(input);
    if (invalid) return invalid;

    const project = this.addProject({
      title: String(input.title).trim(),
      description: String(input.description).trim(),
      repositoryUrl: blankToNull(input.repositoryUrl),
      demoUrl: blankToNull(input.demoUrl)
    });
    this.version += 1;
    return { status: 200, json: project.id };
  }

  private updateProject(id: string, input: Record<string, any>, session: BrowserSession): MockResponse {
    const denied = this.requireAdministrator(session);
    if (denied) return denied;
    const project = this.projects.find(candidate => candidate.id === id);
    if (!project) return { status: 404 };
    const invalid = validateProject(input);
    if (invalid) return invalid;

    project.title = String(input.title).trim();
    project.description = String(input.description).trim();
    project.repositoryUrl = blankToNull(input.repositoryUrl);
    project.demoUrl = blankToNull(input.demoUrl);
    this.version += 1;
    return { status: 204 };
  }

  private removeProject(id: string, session: BrowserSession): MockResponse {
    const denied = this.requireAdministrator(session);
    if (denied) return denied;
    const index = this.projects.findIndex(candidate => candidate.id === id);
    if (index < 0) return { status: 404 };

    this.projects.splice(index, 1);
    for (const team of this.teams) if (team.projectId === id) team.projectId = null;
    this.version += 1;
    return { status: 204 };
  }

  private advance(toScreen: string, expectedVersion: string, session: BrowserSession): MockResponse {
    const denied = this.requireAdministrator(session);
    if (denied) return denied;
    if (expectedVersion !== String(this.version)) return { status: 409 };

    const order: Screen[] = ["countdown", "projects", "teams", "raffle"];
    if (!order.includes(toScreen as Screen)) return { status: 422 };

    this.currentScreen = toScreen as Screen;
    if (this.currentScreen === "teams") this.formTeams();
    this.version += 1;
    return { status: 204 };
  }

  private draw(session: BrowserSession): MockResponse {
    const denied = this.requireAdministrator(session);
    if (denied) return denied;
    return this.commitDraw() ? { status: 204 } : { status: 409 };
  }

  /**
   * Commits a draw the way the server would, and returns the saved result.
   * `startedAgoMs` places the draw in the past, so a test can seed a browser
   * arriving part way through a draw, or after one has already been revealed.
   */
  seedDraw(startedAgoMs = 0): RaffleResult | null {
    return this.commitDraw(startedAgoMs);
  }

  private commitDraw(startedAgoMs = 0): RaffleResult | null {
    const eligible = this.participants.filter(participant => !participant.hasWonRaffle);
    if (!eligible.length) return null;

    const winner = eligible[0];
    winner.hasWonRaffle = true;
    const startedAt = Date.now() + this.serverClockOffsetMs - startedAgoMs;
    const revealAt = startedAt + this.drawRevealDelayMs;
    const result: RaffleResult = {
      drawId: this.identifier("55555555"),
      winnerLabel: winner.name ?? winner.publicLabel,
      candidateLabels: eligible.map(participant => participant.name ?? participant.publicLabel),
      startedAtUtc: new Date(startedAt).toISOString(),
      revealAtUtc: new Date(revealAt).toISOString(),
      effectsEndAtUtc: new Date(revealAt + this.drawEffectsDurationMs).toISOString()
    };
    if (this.latestResult) this.previousWinners = [this.latestResult, ...this.previousWinners];
    this.latestResult = result;
    this.version += 1;
    return result;
  }

  private assignProject(teamId: string, projectId: string | null, session: BrowserSession): MockResponse {
    const denied = this.requireAdministrator(session);
    if (denied) return denied;
    const team = this.teams.find(candidate => candidate.id === teamId);
    if (!team) return { status: 404 };
    if (projectId && !this.projects.some(project => project.id === projectId)) return { status: 409 };

    team.projectId = projectId || null;
    this.version += 1;
    return { status: 204 };
  }

  private moveMember(payload: Record<string, any>, session: BrowserSession): MockResponse {
    const denied = this.requireAdministrator(session);
    if (denied) return denied;

    const participant = this.participants.find(candidate => candidate.id === payload.participantId);
    if (!participant) return { status: 404 };

    const destination = String(payload.destination ?? "");
    if (destination === "unassigned") participant.teamId = null;
    else if (destination === "new") participant.teamId = this.addTeam([]).id;
    else {
      const team = this.teams.find(candidate => candidate.id === payload.teamId);
      if (!team) return { status: 404 };
      participant.teamId = team.id;
    }
    this.version += 1;
    return { status: 204 };
  }
}

function blankToNull(value: unknown): string | null {
  const text = typeof value === "string" ? value.trim() : "";
  return text.length ? text : null;
}

function validateProject(input: Record<string, any>): MockResponse | null {
  const title = String(input.title ?? "").trim();
  const description = String(input.description ?? "").trim();
  if (!title || !description || title.length > 200 || description.length > 2_000) return { status: 422 };
  for (const link of [input.repositoryUrl, input.demoUrl]) {
    const value = typeof link === "string" ? link.trim() : "";
    if (value && !/^https:\/\/[^\s@/]+(\/|$)/.test(value)) return { status: 422 };
    if (value.length > 2_048) return { status: 422 };
  }
  return null;
}
