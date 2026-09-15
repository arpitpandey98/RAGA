import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ChatService } from '../../services/chat/chat';
import { marked } from 'marked';
import { ChatResponse, ChatSource } from '../../models/chat.model';

interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
  sources: ChatSource[];
}

@Component({
  selector: 'app-chat',
  imports: [FormsModule],
  templateUrl: './chat.html',
  styleUrl: './chat.css'
})
export class Chat {

  protected readonly messages = signal<ChatMessage[]>([]);

  protected readonly messageInput = signal('');

  protected readonly loading = signal(false);

  protected readonly error = signal<string | null>(null);

  protected readonly conversationId =
    signal<number | null>(null);

  constructor(
    private readonly chatService: ChatService
  ) { }

  protected renderMarkdown(content: string): string {
    return marked.parse(content) as string;
  }

  protected sendMessage(): void {
    const message = this.messageInput().trim();

    if (!message || this.loading()) {
      return;
    }

    this.error.set(null);

    this.messages.update(messages => [
      ...messages,
      {
        role: 'user',
        content: message,
        sources: []
      }
    ]);

    this.messageInput.set('');
    this.loading.set(true);

    this.chatService.sendMessage({
      conversationId:
        this.conversationId() !== null
          ? this.conversationId()!.toString()
          : undefined,

      message
    }).subscribe({
      next: (response: ChatResponse) => {

        this.conversationId.set(
          response.conversationId
        );

        this.messages.update(messages => [
          ...messages,
          {
            role: 'assistant',
            content: response.answer,
            sources: response.sources
          }
        ]);

        this.loading.set(false);
      },

      error: (error: unknown) => {
        console.error(
          'Failed to send chat message:',
          error
        );

        this.error.set(
          'Unable to get an answer. Please try again.'
        );

        this.loading.set(false);
      }
    });
  }

  protected onInputChange(value: string): void {
    this.messageInput.set(value);
  }
}