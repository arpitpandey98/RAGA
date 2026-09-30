import { Routes } from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';
import { Landing } from './components/landing/landing';
import { Workspace } from './components/workspace/workspace';

export const routes: Routes = [
  { path: '', component: Landing },
  { path: 'app', component: Workspace },
  { path: '**', redirectTo: '' }
];
