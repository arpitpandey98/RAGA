import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthSession } from '../../services/auth/auth-session';

@Component({
  selector: 'app-landing',
  imports: [RouterLink],
  templateUrl: './landing.html',
  styleUrl: './landing.css'
})
export class Landing {
  protected readonly auth = inject(AuthSession);
}
