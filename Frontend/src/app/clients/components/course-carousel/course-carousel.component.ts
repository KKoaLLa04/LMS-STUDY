import { Component, DestroyRef, Input, OnChanges, OnInit, SimpleChanges, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Course } from '../../models/course.model';
import { OcIconComponent } from '../icon/icon.component';

const AUTOPLAY_MS = 5000;

@Component({
  selector: 'app-oc-course-carousel',
  standalone: true,
  imports: [OcIconComponent],
  templateUrl: './course-carousel.component.html',
  styleUrl: './course-carousel.component.scss',
})
export class CourseCarouselComponent implements OnInit, OnChanges {
  @Input({ required: true }) courses: Course[] = [];

  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly activeIndex = signal(0);

  get activeCourse(): Course | undefined {
    return this.courses[this.activeIndex()];
  }

  ngOnInit(): void {
    const timer = setInterval(() => {
      if (this.courses.length === 0) return;
      this.activeIndex.update((i) => (i + 1) % this.courses.length);
    }, AUTOPLAY_MS);
    this.destroyRef.onDestroy(() => clearInterval(timer));
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['courses'] && this.activeIndex() >= this.courses.length) {
      this.activeIndex.set(0);
    }
  }

  goToSlide(index: number): void {
    this.activeIndex.set(index);
  }

  openActiveCourse(): void {
    if (this.activeCourse) this.router.navigate(['/client', this.activeCourse.id]);
  }
}
