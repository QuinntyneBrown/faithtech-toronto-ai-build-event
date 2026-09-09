import {
  Component,
  ChangeDetectionStrategy,
  computed,
  inject,
} from "@angular/core";
import { BadgeComponent, CardComponent } from "@quinntyne/cornerstone";
import {
  TeamBoardComponent,
  MemberMove,
  ProjectAssignment,
} from "@mock/components";
import { EVENT_SERVICE } from "../../data/event-service.token";
@Component({
  selector: "mock-teams-page",
  imports: [TeamBoardComponent, BadgeComponent, CardComponent],
  templateUrl: "./teams-page.component.html",
  styleUrl: "./teams-page.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TeamsPageComponent {
  readonly event = inject(EVENT_SERVICE);
  readonly groups = computed(() => [
    ...this.event
      .state()
      .teams.map((t) => ({
        id: t.id,
        name: t.name,
        projectId: t.projectId || "",
      })),
    { id: "", name: "Unassigned", projectId: "" },
  ]);
  readonly members = computed(() =>
    this.event
      .state()
      .participants.map((p) => ({
        id: p.id,
        name: p.name,
        label: p.label,
        groupId: p.teamId || "",
      })),
  );
  readonly ownTeam = computed(() =>
    this.event
      .state()
      .teams.find((t) => t.id === this.event.participant()?.teamId),
  );
  move(event: MemberMove): void {
    void this.event.dispatch({
      type: "move",
      participantId: event.memberId,
      teamId: event.groupId || null,
    });
  }
  assign(event: ProjectAssignment): void {
    void this.event.dispatch({
      type: "assign",
      teamId: event.groupId,
      projectId: event.projectId || null,
    });
  }
}
