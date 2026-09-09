import type { Page, WebSocketRoute } from "@playwright/test";

/**
 * SignalR terminates every record with the ASCII record separator (0x1E),
 * not a newline. Built from its code point so the byte never has to survive
 * a round trip through an editor or a diff.
 */
const RECORD_SEPARATOR = String.fromCharCode(30);

function negotiation(connection: string) {
  return {
    negotiateVersion: 1,
    connectionId: connection,
    connectionToken: connection,
    availableTransports: [{ transport: "WebSockets", transferFormats: ["Text", "Binary"] }]
  };
}

function frame(message: unknown): string {
  return JSON.stringify(message) + RECORD_SEPARATOR;
}

/**
 * One mocked SignalR hub.
 *
 * The client negotiates over HTTP and then speaks the JSON hub protocol over a
 * web socket, so both halves are intercepted. Nothing is forwarded upstream:
 * the socket is answered entirely from here, which is what lets the suite run
 * with no SignalR server.
 */
export class HubChannel {
  private socket: WebSocketRoute | null = null;
  private handshakeComplete = false;
  private offline = false;

  constructor(
    private readonly name: string,
    private readonly negotiateGlob: string,
    private readonly socketPattern: RegExp
  ) {}

  async install(page: Page): Promise<void> {
    await page.route(this.negotiateGlob, route => {
      if (this.offline) return route.abort("failed");
      return route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify(negotiation(this.name))
      });
    });

    await page.routeWebSocket(this.socketPattern, socket => {
      this.socket = socket;
      this.handshakeComplete = false;

      socket.onMessage(message => {
        const text = typeof message === "string" ? message : message.toString();
        for (const record of text.split(RECORD_SEPARATOR)) {
          if (!record) continue;

          let parsed: Record<string, unknown>;
          try {
            parsed = JSON.parse(record) as Record<string, unknown>;
          } catch {
            continue;
          }

          // The handshake request names a protocol and carries no message type.
          if (typeof parsed.protocol === "string") {
            this.handshakeComplete = true;
            socket.send(frame({}));
            continue;
          }

          // Ping. Answering keeps the client from timing the connection out.
          if (parsed.type === 6) socket.send(frame({ type: 6 }));
        }
      });

      socket.onClose(() => {
        this.socket = null;
        this.handshakeComplete = false;
      });
    });
  }

  get connected(): boolean {
    return this.socket !== null && this.handshakeComplete;
  }

  /** Invokes a hub method on the client. */
  send(target: string, args: unknown[] = []): void {
    this.socket?.send(frame({ type: 1, target, arguments: args }));
  }

  /**
   * Drops the transport the way a lost network would, and refuses the
   * negotiation the client immediately retries, so the disconnected state is
   * stable instead of racing SignalR's automatic reconnect.
   */
  goOffline(): void {
    this.offline = true;
    this.socket?.close({ code: 1006, reason: "transport lost" });
    this.socket = null;
    this.handshakeComplete = false;
  }

  /** Lets the client negotiate again, so a reconnection can succeed. */
  goOnline(): void {
    this.offline = false;
  }
}

/** Paths the API mock must leave alone, because a hub answers them. */
export const HUB_PATHS = ["/hubs/event-updates", "/api/participant/updates", "/api/admin/updates"];

/**
 * The three hubs the companion connects to. Two of them live under `/api`, so
 * they are installed after the REST mock and take precedence over it.
 */
export class HubMock {
  readonly event = new HubChannel(
    "event-updates",
    "**/hubs/event-updates/negotiate**",
    /\/hubs\/event-updates/
  );

  readonly participant = new HubChannel(
    "participant-updates",
    "**/api/participant/updates/negotiate**",
    /\/api\/participant\/updates/
  );

  readonly administrator = new HubChannel(
    "administrator-updates",
    "**/api/admin/updates/negotiate**",
    /\/api\/admin\/updates/
  );

  private get channels(): HubChannel[] {
    return [this.event, this.participant, this.administrator];
  }

  async install(page: Page): Promise<void> {
    for (const channel of this.channels) await channel.install(page);
  }

  get connected(): boolean {
    return this.event.connected;
  }

  /** Pushes the server's "something changed" signal for the given version. */
  pushEventUpdate(version: number | string): void {
    this.event.send("eventUpdated", [String(version)]);
  }

  /** Tells this browser its participant entry session is no longer valid. */
  invalidateParticipantSession(): void {
    this.participant.send("sessionInvalidated");
  }

  /** Tells this browser its administrator session is no longer valid. */
  invalidateAdministratorSession(): void {
    this.administrator.send("sessionInvalidated");
  }

  /** Takes the event hub down. The private hubs stay up unless asked. */
  goOffline(): void {
    this.event.goOffline();
  }

  goOnline(): void {
    this.event.goOnline();
  }

  /** Takes every hub down, as a total loss of connectivity would. */
  goFullyOffline(): void {
    for (const channel of this.channels) channel.goOffline();
  }

  goFullyOnline(): void {
    for (const channel of this.channels) channel.goOnline();
  }
}
