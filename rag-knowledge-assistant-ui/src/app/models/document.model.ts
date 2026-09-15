export interface Document {
  id: number;
  fileType: number;
  fileName: string;
  blobPath: string;
  uploadedBy: string;
  uploadedAt: string;
  status: number;
}