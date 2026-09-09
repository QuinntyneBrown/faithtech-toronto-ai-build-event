export interface RaffleSnapshot {
  eligibleCount: number;
  latestResult: RaffleResult | null;
  previousWinners: RaffleResult[];
}

export interface RaffleResult {
  drawId: string;
  winnerLabel: string;
  candidateLabels: string[];
  startedAtUtc: string;
  revealAtUtc: string;
  effectsEndAtUtc: string;
}
