import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Document } from '../../models/document.model';

@Injectable({
  providedIn: 'root'
})
export class DocumentsService {

  private readonly http = inject(HttpClient);

  private readonly apiUrl = 'https://localhost:44300/api/Document';

  getDocuments(): Observable<Document[]> {
    return this.http.get<Document[]>(this.apiUrl);
  }

  getDocument(id: number): Observable<Document> {
    return this.http.get<Document>(`${this.apiUrl}/${id}`);
  }

  uploadDocument(file: File): Observable<Document> {
    const formData = new FormData();

    formData.append('file', file);

    return this.http.post<Document>(
      this.apiUrl,
      formData
    );
  }

  deleteDocument(id: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${id}`
    );
  }
}