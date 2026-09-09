import { ChangeDetectionStrategy, Component, inject } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { CardComponent, CsButtonDirective, CsSelectDirective, EmptyStateComponent, FieldComponent } from "@quinntyne/cornerstone";
import { ADMINISTRATOR_SESSION_SERVICE, EVENT_FLOW_SERVICE, EVENT_SERVICE, PublicTeam, PublicTeamMember, TEAM_SERVICE } from "@faithtech/api";

@Component({
  selector: "event-teams",
  imports: [CardComponent, CsButtonDirective, CsSelectDirective, EmptyStateComponent, FieldComponent, FormsModule],
  templateUrl: "./teams.component.html",
  styleUrl: "./teams.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TeamsComponent {
  readonly event = inject(EVENT_SERVICE);
  readonly administrator = inject(ADMINISTRATOR_SESSION_SERVICE);
  readonly teams = inject(TEAM_SERVICE);
  readonly flow = inject(EVENT_FLOW_SERVICE);

  constructor() {
    this.event.load();
  }

  projectTitle(projectId: string | null): string | null {
    return projectId === null ? null : this.event.state()?.projects.find(project => project.id === projectId)?.title ?? null;
  }

  memberId(member: PublicTeamMember | string): string {
    return typeof member === "string" ? member : member.id;
  }

  memberLabel(member: PublicTeamMember | string): string {
    return typeof member === "string" ? member : member.label;
  }

  assignProject(team: PublicTeam, projectId: string): void {
    const version = this.event.state()?.version;
    if (version) this.teams.assignProject(team.id, projectId || null, version);
  }

  move(memberId: string, selection: string): void {
    const version = this.event.state()?.version;
    if (!version || !selection) return;
    if (selection === "unassigned" || selection === "new") {
      this.teams.moveMember(memberId, selection, null, version);
      return;
    }
    this.teams.moveMember(memberId, "existing", selection, version);
  }

  advance(): void { const state = this.event.state(); if (state) this.flow.advance("teams", "raffle", state.version); }
}
