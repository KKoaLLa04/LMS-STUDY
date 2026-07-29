// Backend/DTOs/DocumentDto.cs

export interface DocumentItem {
  id: number;
  title: string;
  content?: string;
  fileUrl?: string;
  createdAt: string;
  updatedAt?: string;
  linkedLessonCount: number;
}

export interface CreateDocumentRequest {
  title: string;
  content?: string;
  fileUrl?: string;
}

export type UpdateDocumentRequest = CreateDocumentRequest;
