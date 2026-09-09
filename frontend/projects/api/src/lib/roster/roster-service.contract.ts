import { Signal } from "@angular/core";
import { AdministratorParticipantInput } from "./administrator-participant-input";

export interface AdministratorParticipant {
  id: string;
  email: string;
  publicLabel: string;
  name: string | null;
  whatYouMake: string | null;
  onYourHeart: string | null;
  teamLabel: string | null;
  hasWonRaffle: boolean;
}

export interface IRosterService {
  readonly participants: Signal<AdministratorParticipant[]>;
  readonly loading: Signal<boolean>;
  readonly error: Signal<string | null>;
  load(): void;
  add(email: string, expectedVersion: string): void;
  update(participantId: string, input: AdministratorParticipantInput, expectedVersion: string): void;
  remove(participantId: string, expectedVersion: string): void;
}
