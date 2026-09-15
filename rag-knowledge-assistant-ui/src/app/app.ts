import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { Documents } from './components/documents/documents';
import { Chat } from './components/chat/chat';


@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Documents, Chat],
  templateUrl: './app.html',
  styleUrl: './app.css'
})

export class App {

  protected readonly title = signal(
    'Enterprise Knowledge Assistant'
  );
}