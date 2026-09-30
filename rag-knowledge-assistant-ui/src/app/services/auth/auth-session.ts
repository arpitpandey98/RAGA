import { Injectable, inject, signal } from '@angular/core';
import { MsalBroadcastService, MsalService } from '@azure/msal-angular';
import { AuthenticationResult, InteractionStatus } from '@azure/msal-browser';
import { filter } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthSession {
  private readonly msalService = inject(MsalService);
  private readonly msalBroadcastService = inject(MsalBroadcastService);

  readonly isLoggedIn = signal(false);
  readonly displayName = signal('');

  initialize(): void {
    this.msalService.handleRedirectObservable().subscribe({
      next: (result: AuthenticationResult | null) => {
        if (result?.account) {
          this.msalService.instance.setActiveAccount(result.account);
        }
      },
      error: (error) => {
        console.error('MSAL redirect error:', error);
      }
    });

    this.msalBroadcastService.inProgress$
      .pipe(
        filter((status: InteractionStatus) => status === InteractionStatus.None)
      )
      .subscribe(() => {
        const accounts = this.msalService.instance.getAllAccounts();

        if (!this.msalService.instance.getActiveAccount() && accounts.length > 0) {
          this.msalService.instance.setActiveAccount(accounts[0]);
        }

        const account = this.msalService.instance.getActiveAccount() ?? accounts[0];
        this.isLoggedIn.set(accounts.length > 0);
        this.displayName.set(account?.name ?? account?.username ?? '');
      });
  }

  login(): void {
    this.msalService.loginRedirect({
      scopes: [`api://${environment.auth.apiClientId}/${environment.auth.apiScope}`]
    });
  }

  logout(): void {
    this.msalService.logoutRedirect();
  }
}
