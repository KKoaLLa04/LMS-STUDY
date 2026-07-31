import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Course } from '../../models/course.model';
import { ClientCourseService } from '../../services/client-course.service';
import { CourseReviewService } from '../../services/course-review.service';
import { CourseRatingSummary, CourseReview } from '../../models/course-review.model';
import { courseCtaLabel } from '../../utils/course-progress.util';
import { OcIconComponent } from '../../components/icon/icon.component';
import { ChapterAccordionComponent } from '../../components/chapter-accordion/chapter-accordion.component';
import { CoursePricePipe } from '../../pipes/course-price.pipe';
import { CourseDurationPipe } from '../../pipes/course-duration.pipe';
import { StudentsCountPipe } from '../../pipes/students-count.pipe';
import { RelativeDatePipe } from '../../pipes/relative-date.pipe';
import { DiscussionPanelComponent } from '../../components/discussion-panel/discussion-panel.component';
import { ToastService } from '../../../shared/services/toast.service';

type DetailTab = 'content' | 'materials' | 'reviews' | 'discussion';

const EMPTY_RATING_SUMMARY: CourseRatingSummary = {
  averageRating: 0,
  ratingCount: 0,
  breakdown: [5, 4, 3, 2, 1].map((stars) => ({ stars, count: 0, percent: 0 })),
};

@Component({
  selector: 'app-course-detail',
  standalone: true,
  imports: [
    RouterLink,
    FormsModule,
    OcIconComponent,
    ChapterAccordionComponent,
    CoursePricePipe,
    CourseDurationPipe,
    StudentsCountPipe,
    RelativeDatePipe,
    DiscussionPanelComponent,
  ],
  templateUrl: './course-detail.component.html',
  styleUrl: './course-detail.component.scss',
})
export class CourseDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly courseService = inject(ClientCourseService);
  private readonly reviewService = inject(CourseReviewService);
  private readonly toast = inject(ToastService);

  readonly course = signal<Course | undefined>(undefined);
  readonly notFound = signal(false);
  readonly activeTab = signal<DetailTab>('content');
  readonly isPlayingPreview = signal(false);

  readonly reviews = signal<CourseReview[]>([]);
  readonly ratingSummary = signal<CourseRatingSummary>(EMPTY_RATING_SUMMARY);
  readonly reviewsLoaded = signal(false);
  readonly myReview = computed(() => this.reviews().find((r) => r.isMine));
  readonly reviewFormRating = signal(0);
  readonly reviewFormComment = signal('');
  readonly submittingReview = signal(false);

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
    if (tab === 'reviews' && !this.reviewsLoaded()) this.loadReviews();
  }

  private loadReviews(): void {
    const course = this.course();
    if (!course) return;

    this.reviewsLoaded.set(true);
    this.reviewService.getSummary(course.id).subscribe((res) => {
      if (res.data) this.ratingSummary.set(res.data);
    });
    this.reviewService.getByCourse(course.id).subscribe((res) => {
      const items = res.data?.items ?? [];
      this.reviews.set(items);
      const mine = items.find((r) => r.isMine);
      if (mine) {
        this.reviewFormRating.set(mine.rating);
        this.reviewFormComment.set(mine.comment ?? '');
      }
    });
  }

  setReviewFormRating(stars: number): void {
    this.reviewFormRating.set(stars);
  }

  submitReview(): void {
    const course = this.course();
    if (!course || this.reviewFormRating() < 1 || this.submittingReview()) return;

    this.submittingReview.set(true);
    this.reviewService
      .submit(course.id, { rating: this.reviewFormRating(), comment: this.reviewFormComment().trim() || undefined })
      .subscribe({
        next: () => {
          this.submittingReview.set(false);
          this.toast.success('Gửi đánh giá thành công');
          this.reviewsLoaded.set(false);
          this.loadReviews();
        },
        error: (err) => {
          this.submittingReview.set(false);
          this.toast.error(err?.error?.message ?? 'Có lỗi xảy ra, vui lòng thử lại');
        },
      });
  }

  deleteMyReview(): void {
    const mine = this.myReview();
    if (!mine) return;

    this.reviewService.delete(mine.id).subscribe({
      next: () => {
        this.toast.success('Đã xóa đánh giá');
        this.reviewFormRating.set(0);
        this.reviewFormComment.set('');
        this.reviewsLoaded.set(false);
        this.loadReviews();
      },
      error: () => this.toast.error('Có lỗi xảy ra, vui lòng thử lại'),
    });
  }

  playPreview(): void {
    if (this.course()?.previewVideoUrl) this.isPlayingPreview.set(true);
  }

  goBack(): void {
    this.router.navigate(['/client/khoa-hoc']);
  }
}
