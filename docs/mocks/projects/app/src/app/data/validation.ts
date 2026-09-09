import { Participant } from '../models/participant';
import { Project } from '../models/project';
export function email(value: string): string {
  const normalized = value.trim().toLowerCase();
  if (normalized.length > 254 || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(normalized)) throw new Error('Enter a valid email address.');
  return normalized;
}
export function text(value: string, max: number, required = false): string {
  const clean = value.trim().replace(/\r\n?/g, '\n');
  if ((required && !clean) || [...clean].length > max) throw new Error(required ? 'Complete the required fields within their character limits.' : 'Your answer exceeds the character limit.');
  return clean;
}
export function participantFields(p: Participant): Participant { return { ...p, email: email(p.email), name: text(p.name, 200), makes: text(p.makes, 2000), heart: text(p.heart, 2000) }; }
function link(value: string): string {
  if (!value.trim()) return '';
  try { const url = new URL(value.trim()); if (url.protocol !== 'https:' || url.username || url.password || url.href.length > 2048) throw Error(); return url.href; }
  catch { throw new Error('Links must be complete https:// addresses without credentials.'); }
}
export function projectFields(p: Project): Project { return { ...p, title: text(p.title, 200, true), description: text(p.description, 2000, true), repository: link(p.repository), demo: link(p.demo) }; }
