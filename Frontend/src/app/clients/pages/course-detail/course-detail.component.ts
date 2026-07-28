import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Course } from '../../models/course.model';
import { ClientCourseService } from '../../services/client-course.service';
import { courseCtaLabel } from '../../utils/course-progress.util';
import { OcIconComponent } from '../../components/icon/icon.component';
import { ChapterAccordionComponent } from '../../components/chapter-accordion/chapter-accordion.component';
import { CoursePricePipe } from '../../pipes/course-price.pipe';
import { CourseDurationPipe } from '../../pipes/course-duration.pipe';
import { StudentsCountPipe } from '../../pipes/students-count.pipe';
import { RelativeDatePipe } from '../../pipes/relative-date.pipe';

type DetailTab = 'content' | 'materials' | 'reviews';

@Component({
  selector: 'app-course-detail',
  standalone: true,
  imports: [
    RouterLink,
    OcIconComponent,
    ChapterAccordionComponent,
    CoursePricePipe,
    CourseDurationPipe,
    StudentsCountPipe,
    RelativeDatePipe,
  ],
  templateUrl: './course-detail.component.html',
  styleUrl: './course-detail.component.scss',
})
export class CourseDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly courseService = inject(ClientCourseService);

  readonly course = signal<Course | undefined>(undefined);
  readonly notFound = signal(false);
  readonly activeTab = signal<DetailTab>('content');
  readonly isPlayingPreview = signal(false);

  readonly ctaLabel = computed(() => {
    const course = this.course();
    return course ? courseCtaLabel(course) : '';
  });

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.courseService.getCourseById(id).subscribe((course) => {
      this.course.set(course);
      this.notFound.set(!course);
    });
  }

  setTab(tab: DetailTab): void {
    this.activeTab.set(tab);
  }

  playPreview(): void {
    if (this.course()?.previewVideoUrl) this.isPlayingPreview.set(true);
  }

  goBack(): void {
    this.router.navigate(['/client']);
  }
}
