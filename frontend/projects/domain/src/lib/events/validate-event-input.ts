import { EventFailure, EventInput } from '@faithtech/api';

export function validateEventInput(input: EventInput): EventInput {
  const normalized = { ...input };
  const errors: Record<string, string[]> = {};
  for (const [field, limit] of [ ['title', 200], ['venueName', 200], ['address', 5000], ['waitingContent', 5000],
    ['closingContent', 5000], ['directionsUrl', 2048], ['timezone', 200] ] as const) {
    const value = input[field]?.replace(/\r\n|[\r\f\u0085\u2028\u2029]/g, '\n').replace(/^\p{White_Space}+|\p{White_Space}+$/gu, '') || null;
    normalized[field] = value;
    if (value && [...value].length > limit) errors[field] = [`Use at most ${limit} characters.`];
  }
  if (normalized.directionsUrl) {
    try {
      const url = new URL(normalized.directionsUrl);
      if (!/^https:\/\//i.test(normalized.directionsUrl) || url.protocol !== 'https:' || url.username || url.password)
        errors['directionsUrl'] = ['Use an absolute HTTPS URL without credentials.'];
    } catch { errors['directionsUrl'] = ['Use an absolute HTTPS URL without credentials.']; }
  }
  for (const [field, limit] of [['latitude', 90], ['longitude', 180]] as const) {
    const value = input[field];
    if (value !== null && (!Number.isFinite(value) || value < -limit || value > limit))
      errors[field] = [`Use a ${field} between -${limit} and ${limit}.`];
  }
  if (Object.keys(errors).length) throw new EventFailure(422, 'validation-failed', errors);
  return normalized;
}
