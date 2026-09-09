import { Signal } from "@angular/core";

export interface ParticipantProfile {
  name: string | null;
  whatYouMake: string | null;
  onYourHeart: string | null;
}

export interface IProfileService {
  readonly profile: Signal<ParticipantProfile | null>;
  readonly loading: Signal<boolean>;
  readonly error: Signal<string | null>;
  readonly notice: Signal<string | null>;
  load(): void;
  save(input: ParticipantProfile, expectedVersion: string): void;
  clear(): void;
  discardUnsaved(): void;
}
