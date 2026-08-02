// Backend/DTOs/DocumentDto.cs

export enum DocumentStatus {
  Shared = 'Shared',
  ForLesson = 'ForLesson',
  SharedAndForLesson = 'SharedAndForLesson'
}

export const DOCUMENT_STATUS_LABELS: Record<DocumentStatus, string> = {
  [DocumentStatus.Shared]: 'Chia sẻ',
  [DocumentStatus.ForLesson]: 'Dùng cho bài giảng',
  [DocumentStatus.SharedAndForLesson]: 'Vừa Chia sẻ, vừa dùng cho bài giảng'
};

export interface DocumentItem {
  id: number;
  title: string;
  content?: string;
  fileUrl?: string;
  status: DocumentStatus;
  createdAt: string;
  updatedAt?: string;
  linkedLessonCount: number;
}

export interface CreateDocumentRequest {
  title: string;
  content?: string;
  fileUrl?: string;
  status: DocumentStatus;
}

export type UpdateDocumentRequest = CreateDocumentRequest;
