import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {  ChatRequest,  ChatResponse} from '../../models/chat.model';

@Injectable({
  providedIn: 'root'
})
export class ChatService {

  private readonly http = inject(HttpClient);

  private readonly apiUrl =
    'https://localhost:44300/api/Chat';

  sendMessage(
    request: ChatRequest
  ): Observable<ChatResponse> {
    return this.http.post<ChatResponse>(
      this.apiUrl,
      request
    );
  }
}