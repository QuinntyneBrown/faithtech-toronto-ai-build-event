import { ChangeDetectionStrategy, Component, computed, inject } from "@angular/core";
import { BoardGroup, BoardMember, BoardProject, CardComponent, CsButtonDirective, MemberMove, NewTeamRequest, ProjectAssignment, TeamBoardComponent } from "@quinntyne/cornerstone";
import { ADMINISTRATOR_SESSION_SERVICE, ENTRY_SERVICE, EVENT_FLOW_SERVICE, EVENT_SERVICE, TEAM_SERVICE } from "@faithtech/api";

@Component({
  selector: "event-teams",
  imports: [CardComponent, CsButtonDirective, TeamBoardComponent],
  templateUrl: "./teams.component.html",
  styleUrl: "./teams.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TeamsComponent {
  readonly event = inject(EVENT_SERVICE);
  readonly administrator = inject(ADMINISTRATOR_SESSION_SERVICE);
  readonly entry = inject(ENTRY_SERVICE);
  readonly teams = inject(TEAM_SERVICE);
  readonly flow = inject(EVENT_FLOW_SERVICE);
  readonly groups = computed<BoardGroup[]>(() => [
    ...(this.event.state()?.teams.map(team => ({ id: team.id, name: team.label, projectId: team.projectId ?? "" })) ?? []),
    { id: "", name: "Unassigned", projectId: "" }
  ]);
  readonly members = computed<BoardMember[]>(() => [
    ...(this.event.state()?.teams.flatMap(team => team.members.map(member => ({ id: member.id, name: member.label, label: "", groupId: team.id }))) ?? []),
    ...(this.event.state()?.unassignedMembers.map(member => ({ id: member.id, name: member.label, label: "", groupId: "" })) ?? [])
  ]);
  readonly projects = computed<BoardProject[]>(() => this.event.state()?.projects.map(project => ({ id: project.id, title: project.title })) ?? []);

  constructor() {
    this.event.load();
  }

  assignProject(assignment: ProjectAssignment): void {
    const version = this.event.state()?.version;
    if (version) this.teams.assignProject(assignment.groupId, assignment.projectId || null, version);
  }

  move(move: MemberMove): void {
    const version = this.event.state()?.version;
    if (version) this.teams.moveMember(move.memberId, move.groupId ? "existing" : "unassigned", move.groupId || null, version);
  }

  createTeam(request: NewTeamRequest): void {
    const version = this.event.state()?.version;
    if (version) this.teams.moveMember(request.memberId, "new", null, version);
  }

  advance(): void { const state = this.event.state(); if (state) this.flow.advance("teams", "raffle", state.version); }
}
