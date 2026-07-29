import { animate, state, style, transition, trigger } from '@angular/animations';
import { Component, Input, OnChanges, SimpleChanges, signal } from '@angular/core';
import { Chapter, Lesson } from '../../models/course.model';
import { formatChapterMeta } from '../../utils/format.util';
import { findNextLesson } from '../../utils/chapters.util';
import { OcIconComponent } from '../icon/icon.component';
import { QuizPlayerComponent } from '../quiz-player/quiz-player.component';

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

  /** First chapter open by default, matching the reference design. */
  readonly expandedIds = signal<Set<number>>(new Set());

  /** Which lesson's inline video player is currently open, if any. */
  readonly playingLessonId = signal<number | null>(null);

  private hasInitializedDefault = false;

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
    this.playingLessonId.update((current) => (current === lesson.id ? null : lesson.id));
  }

  get nextLessonId(): number | undefined {
    return findNextLesson(this.chapters)?.id;
  }
}
