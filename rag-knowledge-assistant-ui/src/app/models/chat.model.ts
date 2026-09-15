export interface ChatRequest {
  conversationId?: string;
  message: string;
}

export interface ChatResponse {
  conversationId: number;
  answer: string;
  sources: ChatSource[];
}

export interface ChatSource {
  documentId: number;
  documentName: string;
  pageNumber: number;
}