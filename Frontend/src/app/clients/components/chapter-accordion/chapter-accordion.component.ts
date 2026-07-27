import { animate, state, style, transition, trigger } from '@angular/animations';
import { Component, Input, signal } from '@angular/core';
import { Chapter } from '../../models/course.model';
import { formatChapterMeta } from '../../utils/format.util';
import { findNextLesson } from '../../utils/chapters.util';
import { OcIconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-oc-chapter-accordion',
  standalone: true,
  imports: [OcIconComponent],
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
export class ChapterAccordionComponent {
  @Input({ required: true }) chapters: Chapter[] = [];

  /** First chapter open by default, matching the reference design. */
  readonly expandedIds = signal<Set<number>>(new Set([0]));

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

  get nextLessonId(): number | undefined {
    return findNextLesson(this.chapters)?.id;
  }
}
