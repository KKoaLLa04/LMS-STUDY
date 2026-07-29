import { QuizQuestionAdmin } from '../../courses/models/course.model';

// Backend/DTOs/QuizDto.cs

export interface QuizItem {
  id: number;
  title: string;
  description?: string;
  questionCount: number;
  createdAt: string;
  updatedAt?: string;
  linkedLessonCount: number;
}

export interface CreateQuizRequest {
  title: string;
  description?: string;
}

export type UpdateQuizRequest = CreateQuizRequest;

// Backend/DTOs/QuizImportDto.cs
export interface QuizImportResult {
  questions: QuizQuestionAdmin[];
  warnings: string[];
}
