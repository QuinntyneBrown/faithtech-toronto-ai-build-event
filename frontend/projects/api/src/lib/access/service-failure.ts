export class ServiceFailure extends Error {
  constructor(public readonly status: number, public readonly retryAfterSeconds = 0) {
    super('The request could not be completed.');
  }
}
