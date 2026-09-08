import { Component, DestroyRef, inject, input, OnInit, output, signal } from '@angular/core';
import { PARTICIPANT_SESSION_SERVICE, ParticipantSessionState } from '@faithtech/api';

@Component({ selector: 'ft-participant-session-panel',
  templateUrl: './participant-session-panel.html', styleUrl: './participant-session-panel.css' })
export class ParticipantSessionPanel implements OnInit {
  private readonly service = inject(PARTICIPANT_SESSION_SERVICE);
  private readonly destroy = inject(DestroyRef);
  readonly eventId = input.required<string>();
  readonly showDetails = input(true);
  readonly signedOut = output<void>();
  readonly state = signal<ParticipantSessionState | null>(null);
  readonly error = signal('');
  readonly pendingSignOut = signal(false);
  private channel!: BroadcastChannel;
  private timer?: ReturnType<typeof setTimeout>;
  private revision = 0;

  ngOnInit() {
    this.channel = new BroadcastChannel(`faithtech-participant-session-${this.eventId()}`);
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
      const state = await this.service.read(this.eventId());
      if (revision !== this.revision || this.destroy.destroyed) return;
      if (!state) { this.expire(); return; }
      // Absolute expiry only: unlike an administrator session, nothing here extends it early.
      const remaining = Date.parse(state.absoluteExpiresAtUtc) - Date.parse(state.serverNow);
      if (remaining <= 0) { this.expire(); return; }
      this.state.set(state);
      clearTimeout(this.timer);
      this.timer = setTimeout(() => this.expire(), remaining);
    } catch {
      if (revision === this.revision) this.error.set('Session verification is unavailable. Retry to continue.');
    }
  }

  async signOut() {
    this.revision++;
    this.state.set(null);
    this.pendingSignOut.set(true);
    this.error.set('');
    try { await this.service.signOut(this.eventId()); this.channel.postMessage('signed-out'); this.expire(); }
    catch { this.error.set('Sign-out is pending. Reconnect and retry to revoke the server session.'); }
  }

  private expire() {
    this.revision++;
    this.state.set(null);
    clearTimeout(this.timer);
    this.signedOut.emit();
  }
}
