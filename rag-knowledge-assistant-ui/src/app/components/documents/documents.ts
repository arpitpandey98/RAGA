import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { interval, Subscription } from 'rxjs';
import { DocumentsService } from '../../services/document/documents';
import { Document } from '../../models/document.model';

@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [],
  templateUrl: './documents.html',
  styleUrl: './documents.css'
})
export class Documents implements OnInit, OnDestroy {

  protected readonly documents = signal<Document[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  private pollingSubscription?: Subscription;


  constructor(
    private readonly documentsService: DocumentsService
  ) { }

  ngOnInit(): void {
    this.loadDocuments();
  }

  private loadDocuments(): void {
    this.loading.set(true);
    this.error.set(null);

    this.documentsService.getDocuments().subscribe({
      next: (documents) => {
        this.documents.set(documents);
        this.loading.set(false);
        this.startPollingIfNeeded();
      },

      error: (error) => {
        console.error('Failed to load documents:', error);

        this.error.set(
          'Unable to load documents.'
        );

        this.loading.set(false);
      }
    });
  }

  private startPollingIfNeeded(): void {
    if (!this.hasProcessingDocuments()) {
      this.stopPolling();
      return;
    }

    if (this.pollingSubscription) {
      return;
    }

    this.pollingSubscription = interval(5000).subscribe(() => {
      this.refreshDocuments();
    });
  }

  private refreshDocuments(): void {
    this.documentsService.getDocuments().subscribe({
      next: (documents) => {
        this.documents.set(documents);

        if (!this.hasProcessingDocuments()) {
          this.stopPolling();
        }
      },
      error: (error: unknown) => {
        console.error(
          'Failed to refresh documents:',
          error
        );
      }
    });
  }

  private stopPolling(): void {
    this.pollingSubscription?.unsubscribe();
    this.pollingSubscription = undefined;
  }
  private hasProcessingDocuments(): boolean {
    return this.documents().some(
      document =>
        document.status === 0 ||
        document.status === 1
    );
  }

  protected uploadDocument(file: File): void {
    this.loading.set(true);
    this.error.set(null);

    this.documentsService.uploadDocument(file).subscribe({
      next: () => {
        this.loading.set(false);

        // Refresh the document list after upload.
        this.loadDocuments();
      },

      error: (error) => {
        console.error('Failed to upload document:', error);

        this.error.set(
          'Unable to upload document.'
        );

        this.loading.set(false);
      }
    });
  }

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    const file = input.files?.[0];

    if (!file) {
      return;
    }

    this.uploadDocument(file);

    // Allow selecting the same file again later.
    input.value = '';
  }

  ngOnDestroy(): void {
    this.stopPolling();
  }
}