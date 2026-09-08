import { Component, DestroyRef, effect, inject, input, output, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { SESSION_SERVICE, SessionState } from '@faithtech/api';

@Component({ selector: 'ft-administrator-session-panel', imports: [DatePipe],
  templateUrl: './administrator-session-panel.html', styleUrl: './administrator-session-panel.css' })
export class AdministratorSessionPanel {
  private readonly service = inject(SESSION_SERVICE);
  private readonly destroy = inject(DestroyRef);
  private readonly channel = new BroadcastChannel('faithtech-admin-session');
  private timer?: ReturnType<typeof setTimeout>;
  private revision = 0;
  readonly signedOut = output<void>();
  readonly showDetails = input(true);
  readonly state = signal<SessionState | null>(null);
  readonly error = signal('');
  readonly pendingSignOut = signal(false);

  constructor() {
    effect(() => {
      if (this.service.interactionRevision() > 0 && !this.pendingSignOut()) void this.refresh(false);
    });
    const refresh = () => { if (!document.hidden && !this.pendingSignOut()) void this.refresh(); };
    const conceal = () => { this.state.set(null); refresh(); };
    window.addEventListener('pageshow', conceal);
    window.addEventListener('focus', refresh);
    document.addEventListener('visibilitychange', conceal);
    this.channel.onmessage = () => this.expire();
    this.destroy.onDestroy(() => {
      this.revision++;
      clearTimeout(this.timer);
      this.channel.close();
      window.removeEventListener('pageshow', conceal);
      window.removeEventListener('focus', refresh);
      document.removeEventListener('visibilitychange', conceal);
    });
    void this.refresh();
  }
  async refresh(conceal = true) {
    const revision = ++this.revision;
    if (conceal) this.state.set(null);
    this.error.set('');
    try {
      const state = await this.service.read();
      if (revision !== this.revision || this.destroy.destroyed) return;
      if (!state) { this.expire(); return; }
      const remaining = Math.min(Date.parse(state.idleExpiresAtUtc), Date.parse(state.absoluteExpiresAtUtc)) - Date.parse(state.serverNow);
      if (remaining <= 0) { this.expire(); return; }
      this.state.set(state);
      clearTimeout(this.timer);
      this.timer = setTimeout(() => this.expire(), remaining);
    } catch {
      if (revision === this.revision) this.error.set('Session verification is unavailable. Retry to continue.');
    }
  }
  async keepActive() {
    try { await this.service.interact(); }
    catch { await this.refresh(); }
  }
  async signOut() {
    this.revision++;
    this.state.set(null);
    this.pendingSignOut.set(true);
    this.error.set('');
    try { await this.service.signOut(); this.channel.postMessage('signed-out'); this.expire(); }
    catch { this.error.set('Sign-out is pending. Reconnect and retry to revoke the server session.'); }
  }
  private expire() {
    this.revision++;
    this.state.set(null);
    clearTimeout(this.timer);
    this.signedOut.emit();
  }
}
