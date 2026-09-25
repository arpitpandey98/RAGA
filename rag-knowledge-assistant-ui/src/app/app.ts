import { Component, OnInit, inject, signal} from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {
  AuthenticationResult,
  InteractionStatus
} from '@azure/msal-browser';

import { MsalBroadcastService, MsalService } from '@azure/msal-angular';

import { filter } from 'rxjs/operators';

import { environment } from '../environments/environment';
import { Documents } from './components/documents/documents';
import { Chat } from './components/chat/chat';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Documents, Chat],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {

private readonly msalService = inject(MsalService);
private readonly msalBroadcastService = inject(MsalBroadcastService);

  isLoggedIn = signal(false);

  ngOnInit(): void {
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
      filter(
        (status: InteractionStatus) =>
          status === InteractionStatus.None
      )
    )
    .subscribe(() => {
      const accounts =
        this.msalService.instance.getAllAccounts();

      if (
        !this.msalService.instance.getActiveAccount() &&
        accounts.length > 0
      ) {
        this.msalService.instance.setActiveAccount(accounts[0]);
      }

      this.isLoggedIn.set(accounts.length > 0);
    });
}

  login(): void {
    this.msalService.loginRedirect({
      scopes: [
  `api://${environment.auth.apiClientId}/${environment.auth.apiScope}`
]
    });
  }

  logout(): void {
    this.msalService.logoutRedirect();
  }

  protected readonly title = signal(
  'Enterprise Knowledge Assistant'
);
}