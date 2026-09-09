import { Participant } from './participant';
import { Project } from './project';
import { Team } from './team';
import { Draw } from './draw';
import { Screen } from './screen';
export interface EventState { schema: 1; revision: number; generation: string; stage: Screen; target: number; participants: Participant[]; projects: Project[]; teams: Team[]; draws: Draw[]; nextLabel: number; }
