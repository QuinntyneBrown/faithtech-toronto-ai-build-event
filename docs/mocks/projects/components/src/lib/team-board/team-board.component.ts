import { NativeControlStateDirective } from '../native-control-state.directive';
import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';
import { CdkDropList, CdkDropListGroup, CdkDrag, CdkDragHandle, CdkDragDrop } from '@angular/cdk/drag-drop';
import { CardComponent, BadgeComponent, CsButtonDirective, CsSelectDirective } from '@quinntyne/cornerstone';
import { BoardMember } from './board-member'; import { BoardGroup } from './board-group'; import { BoardProject } from './board-project';
import { MemberMove } from './member-move'; import { ProjectAssignment } from './project-assignment';
@Component({ selector: 'mock-team-board', imports: [NativeControlStateDirective, CdkDropList, CdkDropListGroup, CdkDrag, CdkDragHandle, CardComponent, BadgeComponent, CsButtonDirective, CsSelectDirective], templateUrl: './team-board.component.html', styleUrl: './team-board.component.scss', changeDetection: ChangeDetectionStrategy.OnPush })
export class TeamBoardComponent {
  readonly groups = input.required<BoardGroup[]>(); readonly members = input.required<BoardMember[]>(); readonly projects = input<BoardProject[]>([]);
  readonly editable = input(false); readonly disabled = input(false); readonly currentMember = input('');
  readonly moved = output<MemberMove>(); readonly assigned = output<ProjectAssignment>();
  people(id: string): BoardMember[] { return this.members().filter(p => p.groupId === id); }
  project(id: string): string { return this.projects().find(p => p.id === id)?.title || 'No project assigned'; }
  drop(event: CdkDragDrop<string, string, string>): void { if (this.editable() && !this.disabled() && event.container.data !== event.previousContainer.data) this.moved.emit({ memberId: event.item.data, groupId: event.container.data }); }
  move(member: BoardMember, event: Event): void { const select = event.target as HTMLSelectElement; this.moved.emit({ memberId: member.id, groupId: select.value }); select.value = member.groupId; }
  assign(group: BoardGroup, event: Event): void { const select = event.target as HTMLSelectElement; this.assigned.emit({ groupId: group.id, projectId: select.value }); select.value = group.projectId; }
}

