import type { Page, WebSocketRoute } from "@playwright/test";

/**
 * SignalR terminates every record with the ASCII record separator (0x1E),
 * not a newline. Built from its code point so the byte never has to survive
 * a round trip through an editor or a diff.
 */
const RECORD_SEPARATOR = String.fromCharCode(30);

const NEGOTIATION = {
  negotiateVersion: 1,
  connectionId: "e2e-connection",
  connectionToken: "e2e-token",
  availableTransports: [{ transport: "WebSockets", transferFormats: ["Text", "Binary"] }]
};

function frame(message: unknown): string {
  return JSON.stringify(message) + RECORD_SEPARATOR;
}

/**
 * Stands in for the `event-updates` hub.
 *
 * The client negotiates over HTTP and then speaks the JSON hub protocol over a
 * web socket, so both halves are intercepted. Nothing is forwarded upstream:
 * the socket is answered entirely from here, which is what lets the suite run
 * with no SignalR server.
 */
export class HubMock {
  private socket: WebSocketRoute | null = null;
  private handshakeComplete = false;
  private offline = false;

  async install(page: Page): Promise<void> {
    await page.route("**/hubs/event-updates/negotiate**", route => {
      if (this.offline) return route.abort("failed");
      return route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify(NEGOTIATION)
      });
    });

    await page.routeWebSocket(/\/hubs\/event-updates/, socket => {
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

  /** Pushes the server's "something changed" signal for the given version. */
  pushEventUpdate(version: number | string): void {
    this.socket?.send(frame({ type: 1, target: "eventUpdated", arguments: [String(version)] }));
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
