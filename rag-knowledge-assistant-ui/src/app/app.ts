import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthSession } from './services/auth/auth-session';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private readonly auth = inject(AuthSession);

  ngOnInit(): void {
    this.auth.initialize();
  }
}
