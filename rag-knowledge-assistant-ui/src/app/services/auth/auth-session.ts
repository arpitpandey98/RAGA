import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MsalBroadcastService, MsalService } from '@azure/msal-angular';
import { AuthenticationResult, InteractionStatus } from '@azure/msal-browser';
import { filter } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { CurrentTenant } from '../../models/tenant.model';

@Injectable({ providedIn: 'root' })
export class AuthSession {
  private readonly msalService = inject(MsalService);
  private readonly msalBroadcastService = inject(MsalBroadcastService);
  private readonly http = inject(HttpClient);

  readonly isLoggedIn = signal(false);
  readonly displayName = signal('');
  readonly tenantName = signal('');
  // Person's display name as resolved by the backend from the validated token.
  readonly userDisplayName = signal('');

  readonly signedInName = computed(() => {
    const userName = this.userDisplayName().trim();
    if (userName) {
      return userName;
    }

    // Fallback while /api/Tenant/current is loading or has no name.
    // Never fall back to the tenant name: that is the workspace, not the person.
    const name = this.displayName().trim();
    if (name && name.toLowerCase() !== 'unknown') {
      return name;
    }

    return '';
  });

  private tenantRequestInFlight = false;

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

        if (accounts.length > 0) {
          this.loadTenant();
        } else {
          this.tenantName.set('');
          this.userDisplayName.set('');
          this.tenantRequestInFlight = false;
        }
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

  private loadTenant(): void {
    if (this.tenantName() || this.tenantRequestInFlight) {
      return;
    }

    this.tenantRequestInFlight = true;

    this.http
      .get<CurrentTenant>(`${environment.apiUrl}/api/Tenant/current`)
      .subscribe({
        next: (tenant) => {
          this.tenantName.set(tenant.name?.trim() ?? '');
          this.userDisplayName.set(tenant.displayName?.trim() ?? '');
          this.tenantRequestInFlight = false;
        },
        error: (error) => {
          console.error('Failed to load tenant:', error);
          this.tenantRequestInFlight = false;
        }
      });
  }
}
