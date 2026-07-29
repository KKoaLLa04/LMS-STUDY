/** Shapes returned by the backend (Backend/DTOs/QuizQuestionDto.cs, QuizAttemptDto.cs, QuizDto.cs). */

// Backend/DTOs/QuizDto.cs — StudentQuizDto, dùng cho trang "Quiz chung".
export interface StudentQuizApi {
  id: number;
  title: string;
  description?: string;
  questionCount: number;
}

export interface QuizOptionApi {
  id: number;
  text: string;
}

export interface QuizQuestionApi {
  id: number;
  text: string;
  orderNumber: number;
  allowMultipleAnswers: boolean;
  options: QuizOptionApi[];
}

export interface QuizAnswerSubmission {
  questionId: number;
  selectedOptionIds: number[];
}

export interface QuizAttemptApi {
  id: number;
  quizId?: number;
  scorePercent: number;
  attemptedAt: string;
}

export interface QuizQuestionResultApi {
  questionId: number;
  isCorrect: boolean;
  correctOptionIds: number[];
  selectedOptionIds: number[];
}

export interface QuizAttemptResultApi {
  attempt: QuizAttemptApi;
  bestScorePercent: number;
  pointsAwarded: number;
  questionResults: QuizQuestionResultApi[];
}
