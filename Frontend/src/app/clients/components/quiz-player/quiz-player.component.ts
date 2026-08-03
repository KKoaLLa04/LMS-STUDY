import {
  Component,
  ElementRef,
  Input,
  NgZone,
  OnChanges,
  OnDestroy,
  OnInit,
  SimpleChanges,
  inject,
  signal,
} from '@angular/core';
import { forkJoin } from 'rxjs';
import { RouterLink } from '@angular/router';
import { QuizService } from '../../services/quiz.service';
import { AuthService } from '../../../core/auth/auth.service';
import { QuizAnswerSubmission, QuizAttemptApi, QuizAttemptResultApi, QuizQuestionApi } from '../../models/quiz-api.model';
import { OcIconComponent } from '../icon/icon.component';
import { RelativeDatePipe } from '../../pipes/relative-date.pipe';

/** Ưu tiên câu hỏi gần đỉnh vùng nhìn nhất khi nhiều câu cùng "intersect" — mượt hơn so với
 * chỉ lấy entry đầu tiên trình duyệt trả về (thứ tự không đảm bảo theo vị trí trên trang). */
function topmostEntry(entries: IntersectionObserverEntry[]): IntersectionObserverEntry {
  return entries.reduce((a, b) => (a.boundingClientRect.top <= b.boundingClientRect.top ? a : b));
}

@Component({
  selector: 'app-oc-quiz-player',
  standalone: true,
  imports: [OcIconComponent, RelativeDatePipe, RouterLink],
  templateUrl: './quiz-player.component.html',
  styleUrl: './quiz-player.component.scss',
})
export class QuizPlayerComponent implements OnInit, OnChanges, OnDestroy {
  // Dùng khi quiz gắn trong 1 bài học của khóa học.
  @Input() lessonId?: number;
  // Dùng khi làm Quiz chung trực tiếp (trang "Quiz chung", không qua bài học nào). Có ưu tiên
  // cao hơn lessonId nếu cả hai đều được set.
  @Input() quizId?: number;
  // 'page' (mặc định): trang "Quiz chung" — đủ rộng để hiện bảng điều hướng câu hỏi bám theo
  // scroll. 'inline': quiz gắn trong 1 bài học của khóa (accordion) — vùng hiển thị hẹp, không
  // phù hợp cho bảng điều hướng riêng nên giữ layout 1 cột như cũ.
  @Input() layout: 'page' | 'inline' = 'page';

  private readonly quizService = inject(QuizService);
  private readonly authService = inject(AuthService);
  private readonly elementRef: ElementRef<HTMLElement> = inject(ElementRef);
  private readonly ngZone = inject(NgZone);

  /** Câu hỏi/kết quả chỉ tồn tại cho học sinh đã đăng nhập — API standalone/lesson quiz đều
   * yêu cầu token nên khách chưa đăng nhập luôn nhận mảng rỗng, dễ hiểu lầm là "quiz chưa có
   * câu hỏi". Chặn từ FE để hiện đúng thông báo "cần đăng nhập". */
  readonly isLoggedIn = signal(this.authService.isLoggedIn());

  readonly questions = signal<QuizQuestionApi[]>([]);
  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly result = signal<QuizAttemptResultApi | null>(null);
  /** Câu hỏi đang ở gần đỉnh vùng nhìn nhất — dùng để làm nổi ô tương ứng trên bảng điều hướng. */
  readonly activeQuestionId = signal<number | null>(null);
  /** Lịch sử các lần làm trước — tải lại mỗi khi vào trang để không mất dấu vết dù kết quả lần
   * làm hiện tại (result) chỉ tồn tại trong bộ nhớ component và biến mất khi rời trang. */
  readonly attempts = signal<QuizAttemptApi[]>([]);

  /** questionId -> Set of selected optionIds. */
  private selections = new Map<number, Set<number>>();
  private scrollSpyObserver?: IntersectionObserver;

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

  ngOnDestroy(): void {
    this.scrollSpyObserver?.disconnect();
  }

  private loadQuestions(): void {
    this.isLoggedIn.set(this.authService.isLoggedIn());
    if (!this.isLoggedIn()) {
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    this.result.set(null);
    this.selections.clear();
    this.activeQuestionId.set(null);
    this.attempts.set([]);
    this.scrollSpyObserver?.disconnect();

    const questions$ = this.quizId != null
      ? this.quizService.getQuestionsStandalone(this.quizId)
      : this.quizService.getQuestions(this.lessonId!);
    const attempts$ = this.quizId != null
      ? this.quizService.getMyAttemptsStandalone(this.quizId)
      : this.quizService.getMyAttempts(this.lessonId!);

    forkJoin({ questions: questions$, attempts: attempts$ }).subscribe(({ questions, attempts }) => {
      this.questions.set(questions);
      this.attempts.set(attempts);
      this.loading.set(false);
      if (this.layout === 'page' && questions.length > 1) {
        // Chờ Angular render xong các thẻ câu hỏi (đợi hết CD hiện tại) rồi mới gắn observer.
        setTimeout(() => this.setupScrollSpy());
      }
      // attempts đã sắp xếp mới nhất trước (GetMyAttemptsAsync) — khôi phục đúng màn hình kết
      // quả của lần làm gần nhất thay vì chỉ hiện điểm số tóm tắt, để không mất dấu vết khi
      // học sinh tải lại trang sau khi đã làm bài.
      if (attempts.length > 0) this.restoreAttempt(attempts[0].id);
    });
  }

  private restoreAttempt(attemptId: number): void {
    const detail$ = this.quizId != null
      ? this.quizService.getAttemptDetailStandalone(this.quizId, attemptId)
      : this.quizService.getAttemptDetail(this.lessonId!, attemptId);
    detail$.subscribe((res) => {
      if (!res) return;
      for (const qr of res.questionResults) {
        this.selections.set(qr.questionId, new Set(qr.selectedOptionIds));
      }
      this.result.set(res);
    });
  }

  private setupScrollSpy(): void {
    const questionEls = this.elementRef.nativeElement.querySelectorAll<HTMLElement>('[data-question-id]');
    if (questionEls.length === 0) return;

    this.ngZone.runOutsideAngular(() => {
      this.scrollSpyObserver = new IntersectionObserver(
        (entries) => {
          const visible = entries.filter((e) => e.isIntersecting);
          if (visible.length === 0) return;
          const id = Number(topmostEntry(visible).target.getAttribute('data-question-id'));
          this.ngZone.run(() => this.activeQuestionId.set(id));
        },
        // Dải quan sát nằm ngay dưới header sticky (78px) tới khoảng giữa màn hình — coi một
        // câu là "đang xem" ngay khi nó chạm dải này, không cần cuộn hết cả câu qua khỏi màn hình.
        { rootMargin: '-84px 0px -55% 0px', threshold: 0 }
      );
      questionEls.forEach((el) => this.scrollSpyObserver!.observe(el));
    });
  }

  scrollToQuestion(questionId: number): void {
    this.elementRef.nativeElement
      .querySelector(`[data-question-id="${questionId}"]`)
      ?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  isSelected(questionId: number, optionId: number): boolean {
    return this.selections.get(questionId)?.has(optionId) ?? false;
  }

  isAnswered(questionId: number): boolean {
    return (this.selections.get(questionId)?.size ?? 0) > 0;
  }

  get answeredCount(): number {
    return this.questions().filter((q) => this.isAnswered(q.id)).length;
  }

  get latestAttempt(): QuizAttemptApi | null {
    const list = this.attempts();
    if (list.length === 0) return null;
    return list.reduce((latest, a) => (new Date(a.attemptedAt) > new Date(latest.attemptedAt) ? a : latest));
  }

  get bestScorePercent(): number {
    return this.attempts().reduce((best, a) => Math.max(best, a.scorePercent), 0);
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
      if (res) this.attempts.update((list) => [...list, res.attempt]);
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
