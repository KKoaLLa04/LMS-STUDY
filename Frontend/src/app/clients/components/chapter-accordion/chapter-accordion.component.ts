import { animate, state, style, transition, trigger } from '@angular/animations';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject, signal } from '@angular/core';
import { Chapter, Lesson } from '../../models/course.model';
import { formatChapterMeta } from '../../utils/format.util';
import { findNextLesson } from '../../utils/chapters.util';
import { OcIconComponent } from '../icon/icon.component';
import { QuizPlayerComponent } from '../quiz-player/quiz-player.component';
import { AuthService } from '../../../core/auth/auth.service';
import { LessonProgressService } from '../../services/lesson-progress.service';

/** Chỉ gửi PUT lên server tối đa mỗi N giây xem thêm được (qua sự kiện timeupdate) — tránh
 * gọi API dồn dập, pause/ended vẫn luôn gửi ngay để không mất tiến độ dở dang. */
const PROGRESS_REPORT_INTERVAL_SECONDS = 5;

@Component({
  selector: 'app-oc-chapter-accordion',
  standalone: true,
  imports: [OcIconComponent, QuizPlayerComponent],
  templateUrl: './chapter-accordion.component.html',
  styleUrl: './chapter-accordion.component.scss',
  animations: [
    trigger('expandCollapse', [
      state('collapsed', style({ height: '0', opacity: 0, overflow: 'hidden' })),
      state('expanded', style({ height: '*', opacity: 1, overflow: 'hidden' })),
      transition('collapsed <=> expanded', animate('250ms ease')),
    ]),
  ],
})
export class ChapterAccordionComponent implements OnChanges {
  @Input({ required: true }) chapters: Chapter[] = [];

  /** Bài học video bị chặn vì chưa đăng nhập — parent (course-detail) xử lý toast/điều hướng login. */
  @Output() videoBlocked = new EventEmitter<void>();

  /** Bài học vừa được backend xác nhận hoàn thành (>= 90% thời lượng) — parent tải lại khóa học
   * để cập nhật % tiến độ, trạng thái "đã hoàn thành" và nút "Tiếp tục: bài kế tiếp". */
  @Output() lessonCompleted = new EventEmitter<number>();

  private readonly authService = inject(AuthService);
  private readonly lessonProgressService = inject(LessonProgressService);

  /** First chapter open by default, matching the reference design. */
  readonly expandedIds = signal<Set<number>>(new Set());

  /** Which lesson's inline video player is currently open, if any. */
  readonly playingLessonId = signal<number | null>(null);

  private hasInitializedDefault = false;
  private readonly lastReportedSecond = new Map<number, number>();

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.hasInitializedDefault && changes['chapters'] && this.chapters.length > 0) {
      this.hasInitializedDefault = true;
      this.expandedIds.set(new Set([this.chapters[0].id]));
    }
  }

  isExpanded(chapterId: number): boolean {
    return this.expandedIds().has(chapterId);
  }

  toggle(chapterId: number): void {
    this.expandedIds.update((current) => {
      const next = new Set(current);
      if (next.has(chapterId)) next.delete(chapterId);
      else next.add(chapterId);
      return next;
    });
  }

  chapterMeta(chapter: Chapter): string {
    return formatChapterMeta(chapter.lessons.length);
  }

  isLessonPlayable(lesson: Lesson): boolean {
    if (lesson.status === 'locked') return false;
    switch (lesson.lessonType) {
      case 'Document':
        return !!lesson.content || !!lesson.documentUrl;
      case 'Quiz':
        return true;
      default:
        return !!lesson.videoUrl;
    }
  }

  toggleLessonVideo(lesson: Lesson): void {
    if (!this.isLessonPlayable(lesson)) return;
    if (lesson.lessonType === 'Video' && !this.authService.isLoggedIn()) {
      this.videoBlocked.emit();
      return;
    }
    this.playingLessonId.update((current) => (current === lesson.id ? null : lesson.id));
  }

  /** Mở (mở rộng chương + phát) một bài học cụ thể theo id — dùng cho nút "Tiếp tục: bài ..."
   * ở trang chi tiết khóa học, khác với toggleLessonVideo() vì luôn MỞ chứ không đóng lại. */
  openLesson(lessonId: number): void {
    const chapter = this.chapters.find((c) => c.lessons.some((l) => l.id === lessonId));
    const lesson = chapter?.lessons.find((l) => l.id === lessonId);
    if (!chapter || !lesson || !this.isLessonPlayable(lesson)) return;

    if (lesson.lessonType === 'Video' && !this.authService.isLoggedIn()) {
      this.videoBlocked.emit();
      return;
    }

    this.expandedIds.update((current) => new Set(current).add(chapter.id));
    this.playingLessonId.set(lessonId);

    setTimeout(() => {
      document.getElementById(`lesson-${lessonId}`)?.scrollIntoView({ behavior: 'smooth', block: 'center' });
    });
  }

  onLessonTimeUpdate(lesson: Lesson, event: Event): void {
    const video = event.target as HTMLVideoElement;
    const current = Math.floor(video.currentTime);
    const last = this.lastReportedSecond.get(lesson.id) ?? 0;
    if (current - last < PROGRESS_REPORT_INTERVAL_SECONDS) return;
    this.reportLessonProgress(lesson, current);
  }

  onLessonPaused(lesson: Lesson, event: Event): void {
    const video = event.target as HTMLVideoElement;
    this.reportLessonProgress(lesson, Math.floor(video.currentTime));
  }

  onLessonEnded(lesson: Lesson, event: Event): void {
    const video = event.target as HTMLVideoElement;
    const watchedSeconds = Math.floor(video.duration || lesson.durationMinutes * 60);
    this.reportLessonProgress(lesson, watchedSeconds);
  }

  private reportLessonProgress(lesson: Lesson, watchedSeconds: number): void {
    this.lastReportedSecond.set(lesson.id, watchedSeconds);
    this.lessonProgressService.updateProgress(lesson.id, watchedSeconds).subscribe((res) => {
      if (res.data?.isCompleted) this.lessonCompleted.emit(lesson.id);
    });
  }

  get nextLessonId(): number | undefined {
    return findNextLesson(this.chapters)?.id;
  }
}
