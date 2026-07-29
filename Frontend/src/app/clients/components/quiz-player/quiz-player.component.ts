import { Component, Input, OnChanges, OnInit, SimpleChanges, inject, signal } from '@angular/core';
import { QuizService } from '../../services/quiz.service';
import { QuizAnswerSubmission, QuizAttemptResultApi, QuizQuestionApi } from '../../models/quiz-api.model';
import { OcIconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-oc-quiz-player',
  standalone: true,
  imports: [OcIconComponent],
  templateUrl: './quiz-player.component.html',
  styleUrl: './quiz-player.component.scss',
})
export class QuizPlayerComponent implements OnInit, OnChanges {
  // Dùng khi quiz gắn trong 1 bài học của khóa học.
  @Input() lessonId?: number;
  // Dùng khi làm Quiz chung trực tiếp (trang "Quiz chung", không qua bài học nào). Có ưu tiên
  // cao hơn lessonId nếu cả hai đều được set.
  @Input() quizId?: number;

  private readonly quizService = inject(QuizService);

  readonly questions = signal<QuizQuestionApi[]>([]);
  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly result = signal<QuizAttemptResultApi | null>(null);

  /** questionId -> Set of selected optionIds. */
  private selections = new Map<number, Set<number>>();

  ngOnInit(): void {
    this.loadQuestions();
  }

  ngOnChanges(changes: SimpleChanges): void {
    const lessonChanged = changes['lessonId'] && !changes['lessonId'].firstChange;
    const quizChanged = changes['quizId'] && !changes['quizId'].firstChange;
    if (lessonChanged || quizChanged) {
      this.loadQuestions();
    }
  }

  private loadQuestions(): void {
    this.loading.set(true);
    this.result.set(null);
    this.selections.clear();
    const questions$ = this.quizId != null
      ? this.quizService.getQuestionsStandalone(this.quizId)
      : this.quizService.getQuestions(this.lessonId!);
    questions$.subscribe((questions) => {
      this.questions.set(questions);
      this.loading.set(false);
    });
  }

  isSelected(questionId: number, optionId: number): boolean {
    return this.selections.get(questionId)?.has(optionId) ?? false;
  }

  toggleOption(question: QuizQuestionApi, optionId: number): void {
    if (this.result()) return; // đã nộp bài, khóa lựa chọn cho tới khi làm lại

    const current = this.selections.get(question.id) ?? new Set<number>();
    if (question.allowMultipleAnswers) {
      if (current.has(optionId)) current.delete(optionId);
      else current.add(optionId);
    } else {
      current.clear();
      current.add(optionId);
    }
    this.selections.set(question.id, current);
  }

  get allAnswered(): boolean {
    return this.questions().every((q) => (this.selections.get(q.id)?.size ?? 0) > 0);
  }

  submit(): void {
    if (this.submitting() || !this.allAnswered) return;

    const answers: QuizAnswerSubmission[] = this.questions().map((q) => ({
      questionId: q.id,
      selectedOptionIds: Array.from(this.selections.get(q.id) ?? []),
    }));

    this.submitting.set(true);
    const submit$ = this.quizId != null
      ? this.quizService.submitAttemptStandalone(this.quizId, answers)
      : this.quizService.submitAttempt(this.lessonId!, answers);
    submit$.subscribe((res) => {
      this.submitting.set(false);
      this.result.set(res);
    });
  }

  retry(): void {
    this.result.set(null);
    this.selections.clear();
  }

  optionState(questionId: number, optionId: number): 'correct' | 'incorrect' | 'neutral' {
    const r = this.result();
    if (!r) return 'neutral';
    const qResult = r.questionResults.find((qr) => qr.questionId === questionId);
    if (!qResult) return 'neutral';
    const isCorrectOption = qResult.correctOptionIds.includes(optionId);
    const wasSelected = qResult.selectedOptionIds.includes(optionId);
    if (isCorrectOption) return 'correct';
    if (wasSelected && !isCorrectOption) return 'incorrect';
    return 'neutral';
  }
}
