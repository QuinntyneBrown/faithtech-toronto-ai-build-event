import { HttpClient } from "@angular/common/http";
import { Injectable, signal } from "@angular/core";
import { IAdministratorSessionService } from "./administrator-session-service.contract";

@Injectable()
export class AdministratorSessionService implements IAdministratorSessionService {
  readonly active = signal(false);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  constructor(private readonly http: HttpClient) {}

  load(): void {
    this.http.get<{ authenticated: boolean }>("/api/admin/session").subscribe({
      next: state => this.active.set(state.authenticated),
      error: () => this.active.set(false)
    });
  }

  signIn(passcode: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.http.post<{ authenticated: boolean }>("/api/admin/session", { passcode }).subscribe({
      next: state => {
        this.active.set(state.authenticated);
        this.loading.set(false);
      },
      error: () => {
        this.active.set(false);
        this.error.set("That passcode was not accepted.");
        this.loading.set(false);
      }
    });
  }
}
