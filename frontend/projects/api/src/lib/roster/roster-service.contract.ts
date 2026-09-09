import { Signal } from "@angular/core";

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
}
