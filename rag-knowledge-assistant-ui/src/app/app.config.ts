import {
  ApplicationConfig,
  inject,
  provideAppInitializer
} from '@angular/core';

import {
  provideHttpClient,
  withInterceptorsFromDi,
  HTTP_INTERCEPTORS
} from '@angular/common/http';

import { provideRouter } from '@angular/router';

import {
  IPublicClientApplication,
  PublicClientApplication,
  InteractionType,
  BrowserCacheLocation
} from '@azure/msal-browser';

import {
  MSAL_INSTANCE,
  MSAL_GUARD_CONFIG,
  MSAL_INTERCEPTOR_CONFIG,
  MsalGuardConfiguration,
  MsalInterceptorConfiguration,
  MsalInterceptor,
  MsalService,
  MsalGuard,
  MsalBroadcastService
} from '@azure/msal-angular';

import { routes } from './app.routes';
import { environment } from '../environments/environment';


export function MSALInstanceFactory(): IPublicClientApplication {
  return new PublicClientApplication({
    auth: {
      clientId: environment.auth.clientId,
      authority: `https://login.microsoftonline.com/${environment.auth.tenantId}`,
      redirectUri: window.location.origin,
      postLogoutRedirectUri: window.location.origin
    },
    cache: {
      cacheLocation: BrowserCacheLocation.LocalStorage
    }
  });
}


export function MSALGuardConfigFactory(): MsalGuardConfiguration {
  return {
    interactionType: InteractionType.Redirect,
    authRequest: {
      scopes: [
        `api://${environment.auth.apiClientId}/${environment.auth.apiScope}`
      ]
    }
  };
}


export function MSALInterceptorConfigFactory(): MsalInterceptorConfiguration {
  const protectedResourceMap = new Map<string, string[]>();

  protectedResourceMap.set(
    `${environment.apiUrl}/api/*`,
    [
      `api://${environment.auth.apiClientId}/${environment.auth.apiScope}`
    ]
  );

  return {
    interactionType: InteractionType.Redirect,
    protectedResourceMap
  };
}


function initializeMsal(): Promise<void> {
  const msalInstance = inject(MSAL_INSTANCE);

  return msalInstance.initialize();
}


export const appConfig: ApplicationConfig = {
  providers: [

    provideAppInitializer(initializeMsal),

    provideRouter(routes),

    provideHttpClient(
      withInterceptorsFromDi()
    ),

    {
      provide: MSAL_INSTANCE,
      useFactory: MSALInstanceFactory
    },

    {
      provide: MSAL_GUARD_CONFIG,
      useFactory: MSALGuardConfigFactory
    },

    {
      provide: MSAL_INTERCEPTOR_CONFIG,
      useFactory: MSALInterceptorConfigFactory
    },

    {
      provide: HTTP_INTERCEPTORS,
      useClass: MsalInterceptor,
      multi: true
    },

    MsalService,
    MsalGuard,
    MsalBroadcastService
  ]
};