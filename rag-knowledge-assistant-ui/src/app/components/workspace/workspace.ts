import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Documents } from '../documents/documents';
import { Chat } from '../chat/chat';

@Component({
  selector: 'app-workspace',
  imports: [RouterLink, Documents, Chat],
  templateUrl: './workspace.html',
  styleUrl: './workspace.css'
})
export class Workspace {}
