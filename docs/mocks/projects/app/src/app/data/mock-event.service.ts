import { Injectable, computed, signal } from '@angular/core';
import { IEventService } from './event-service.contract';
import { EventState } from '../models/event-state';
import { EventCommand } from './event-command';
import { seed } from './seed';
import { applyCommand } from './apply-command';

const STORAGE = 'faithtech-companion-mock-v1';
@Injectable()
export class MockEventService implements IEventService {
  readonly state = signal(seed()); readonly ready = signal(false); readonly admin = signal(false);
  readonly offline = signal(false); readonly busy = signal(false); readonly failNext = signal(false);
  readonly error = signal(''); readonly notice = signal('');
  private readonly identity = signal<string | null>(null);
  readonly participant = computed(() => this.state().participants.find(p => p.id === this.identity()));
  private readonly channel = new BroadcastChannel(STORAGE);
  constructor() {
    try { this.admin.set(sessionStorage.getItem(STORAGE + '-admin') === 'yes'); this.identity.set(sessionStorage.getItem(STORAGE + '-person')); }
    catch { this.error.set('Browser session storage is unavailable. Enable storage and reload this mock.'); }
    this.channel.onmessage = () => this.refresh();
    window.addEventListener('storage', e => { if (e.key === STORAGE) this.refresh(); });
    window.addEventListener('focus', () => this.refresh());
    if (!navigator.locks) { this.error.set('Open the mock on localhost or HTTPS in Chrome to enable synchronized tabs.'); return; }
    void navigator.locks.request(STORAGE, () => {
      try { const saved = this.read(); if (saved) this.state.set(saved); else localStorage.setItem(STORAGE, JSON.stringify(this.state())); this.ready.set(true); }
      catch { this.error.set('Mock storage could not be loaded. Use Reset mock to restore the fixtures.'); }
    });
  }
  private read(): EventState | null {
    const value = localStorage.getItem(STORAGE); if (!value) return null;
    const state: EventState = JSON.parse(value);
    if (state.schema !== 1 || !Array.isArray(state.participants) || !Array.isArray(state.draws)) throw new Error('Stored mock data is incompatible. Reset the mock.');
    return state;
  }
  private refresh(): void {
    if (this.offline()) return;
    try {
      const next = this.read(); if (!next || (next.generation === this.state().generation && next.revision < this.state().revision)) return;
      if (next.generation !== this.state().generation) this.clearEntry();
      this.state.set(next);
    } catch { this.error.set('Could not synchronize mock data. Reset the mock or allow browser storage.'); }
  }
  async dispatch(command: EventCommand): Promise<boolean> {
    if (this.busy()) return false;
    this.error.set(''); this.notice.set('');
    if (this.offline()) { this.error.set('Reconnect this tab before making changes.'); return false; }
    const version = this.state().revision; this.busy.set(true);
    try {
      if (!navigator.locks) throw new Error('Synchronized changes require localhost or HTTPS in Chrome.');
      if (this.failNext()) { this.failNext.set(false); throw new Error('Simulated save failure. Your changes were not saved; try again.'); }
      await navigator.locks.request(STORAGE, () => {
        let current = this.state();
        try { current = this.read() ?? current; } catch (error) { if (command.type !== 'reset') throw error; }
        if (command.type !== 'reset' && current.revision !== version) { this.state.set(current); throw new Error('Another tab updated the event. Review the latest values and try again.'); }
        if (!['enter', 'profile', 'reset', 'restartCountdown'].includes(command.type) && !this.admin()) throw new Error('Sign in as an administrator first.');
        if (command.type === 'profile' && command.id !== this.identity()) throw new Error('This entry belongs to another session.');
        const next = applyCommand(current, command);
        localStorage.setItem(STORAGE, JSON.stringify(next)); this.state.set(next); this.ready.set(true);
        if (command.type === 'enter') { this.identity.set(command.id); try { sessionStorage.setItem(STORAGE + '-person', command.id); } catch { /* Entry committed; current-tab ownership still works. */ } }
        if (command.type === 'reset') this.clearEntry();
        this.channel.postMessage(next.revision);
      });
      this.notice.set(command.type === 'enter' ? 'You have been entered into the raffle.' : command.type === 'reset' ? 'Mock reset. All tabs now share a fresh event.' : 'Saved. Connected tabs are up to date.');
      return true;
    } catch (error) { this.error.set(error instanceof Error ? error.message : 'Could not save this change.'); return false; }
    finally { this.busy.set(false); }
  }
  login(passcode: string): boolean {
    this.error.set('');
    if (passcode !== '0042') { this.error.set('Use the four-digit mock passcode 0042.'); return false; }
    if (this.offline()) { this.error.set('Reconnect this tab before signing in.'); return false; }
    this.admin.set(true); try { sessionStorage.setItem(STORAGE + '-admin', 'yes'); } catch { /* Current tab can still demonstrate login. */ }
    this.notice.set('Administrator controls are available in this tab.'); return true;
  }
  logout(): void { this.admin.set(false); try { sessionStorage.removeItem(STORAGE + '-admin'); } catch { /* In-memory session is cleared. */ } this.dismiss(); }
  clearEntry(): void { this.identity.set(null); try { sessionStorage.removeItem(STORAGE + '-person'); } catch { /* In-memory session is cleared. */ } }
  setOffline(value: boolean): void { this.offline.set(value); if (!value) this.refresh(); }
  simulateFailure(): void { this.failNext.set(!this.failNext()); }
  dismiss(): void { this.error.set(''); this.notice.set(''); }
}
