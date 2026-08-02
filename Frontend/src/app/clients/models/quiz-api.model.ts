/** Shapes returned by the backend (Backend/DTOs/QuizQuestionDto.cs, QuizAttemptDto.cs, QuizDto.cs). */

// Backend/DTOs/QuizDto.cs — StudentQuizDto, dùng cho trang "Quiz chung".
export interface StudentQuizApi {
  id: number;
  title: string;
  description?: string;
  questionCount: number;
  /** Người dùng hiện tại đã làm quiz này ít nhất 1 lần chưa. */
  hasAttempted: boolean;
  /** Điểm cao nhất của người dùng hiện tại — null nếu chưa từng làm. */
  bestScorePercent?: number | null;
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
