import { Component, OnInit, ViewChild, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Course } from '../../models/course.model';
import { ClientCourseService } from '../../services/client-course.service';
import { CourseReviewService } from '../../services/course-review.service';
import { CourseRatingSummary, CourseReview } from '../../models/course-review.model';
import { courseCtaLabel } from '../../utils/course-progress.util';
import { findNextLesson } from '../../utils/chapters.util';
import { OcIconComponent } from '../../components/icon/icon.component';
import { ChapterAccordionComponent } from '../../components/chapter-accordion/chapter-accordion.component';
import { CoursePricePipe } from '../../pipes/course-price.pipe';
import { CourseDurationPipe } from '../../pipes/course-duration.pipe';
import { StudentsCountPipe } from '../../pipes/students-count.pipe';
import { RelativeDatePipe } from '../../pipes/relative-date.pipe';
import { DiscussionPanelComponent } from '../../components/discussion-panel/discussion-panel.component';
import { ToastService } from '../../../shared/services/toast.service';
import { AuthService } from '../../../core/auth/auth.service';

type DetailTab = 'content' | 'materials' | 'reviews' | 'discussion';

// Đăng ký khóa học hiện xử lý thủ công qua Facebook thay vì tự động ghi danh — theo yêu cầu chủ dự án.
const ENROLLMENT_CONTACT_URL = 'https://www.facebook.com/kien.nguyenduy.169067';

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
  @ViewChild(ChapterAccordionComponent) private readonly chapterAccordion?: ChapterAccordionComponent;

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly courseService = inject(ClientCourseService);
  private readonly reviewService = inject(CourseReviewService);
  private readonly toast = inject(ToastService);
  private readonly authService = inject(AuthService);

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

  /** Tab "Tài liệu" chỉ hiển thị các bài học lessonType === 'Document', giữ nguyên nhóm theo
   * chương — chương không còn bài Tài liệu nào sau khi lọc thì bỏ luôn khỏi danh sách. */
  readonly documentChapters = computed(() => {
    const course = this.course();
    if (!course) return [];
    return course.chapters
      .map((chapter) => ({ ...chapter, lessons: chapter.lessons.filter((l) => l.lessonType === 'Document') }))
      .filter((chapter) => chapter.lessons.length > 0);
  });

  ngOnInit(): void {
    this.loadCourse(Number(this.route.snapshot.paramMap.get('id')));
  }

  private loadCourse(id: number): void {
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

    // Chỉ đánh dấu "đã tải" SAU KHI request thực sự trả kết quả (kể cả lỗi) — trước đây set true
    // ngay khi bắt đầu gọi API khiến một lần gọi lỗi (vd. 401 lúc chưa đăng nhập) làm mất luôn cơ
    // hội tải lại: bấm lại tab "Đánh giá" sau khi đăng nhập vẫn không gọi lại API, cứ hiện 0 mãi.
    this.reviewService.getSummary(course.id).subscribe((res) => {
      if (res.data) this.ratingSummary.set(res.data);
    });
    this.reviewService.getByCourse(course.id).subscribe({
      next: (res) => {
        const items = res.data?.items ?? [];
        this.reviews.set(items);
        const mine = items.find((r) => r.isMine);
        if (mine) {
          this.reviewFormRating.set(mine.rating);
          this.reviewFormComment.set(mine.comment ?? '');
        }
        this.reviewsLoaded.set(true);
      },
      error: () => this.reviewsLoaded.set(true),
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

  /** Video giới thiệu là nội dung quảng bá công khai — xem được dù chưa đăng nhập/chưa đăng ký,
   * khác với video bài học thật (bên trong chương trình học) vẫn yêu cầu đăng nhập. */
  playPreview(): void {
    if (this.course()?.previewVideoUrl) this.isPlayingPreview.set(true);
  }

  onLessonVideoBlocked(): void {
    this.requireLoginForVideo();
  }

  /** Backend vừa xác nhận một bài học đạt ngưỡng hoàn thành — tải lại khóa học để cập nhật
   * % tiến độ, trạng thái "đã hoàn thành" (icon check) và nhãn nút "Tiếp tục: bài kế tiếp". */
  onLessonCompleted(): void {
    const course = this.course();
    if (course) this.loadCourse(course.id);
  }

  /** Nút "Tiếp tục: bài ..." — chuyển sang tab Nội dung rồi mở đúng bài học kế tiếp
   * (mở rộng chương chứa nó + tự phát, xử lý bởi ChapterAccordionComponent.openLesson). */
  continueLearning(): void {
    const course = this.course();
    if (!course) return;

    const nextLesson = findNextLesson(course.chapters);
    if (!nextLesson) return;

    this.activeTab.set('content');
    setTimeout(() => this.chapterAccordion?.openLesson(nextLesson.id));
  }

  /** Không tự ghi danh nữa — đưa học viên sang Facebook để đăng ký thủ công. */
  enrollNow(): void {
    window.open(ENROLLMENT_CONTACT_URL, '_blank', 'noopener');
  }

  goBack(): void {
    this.router.navigate(['/client/khoa-hoc']);
  }

  /** Video bài học (không phải video giới thiệu) yêu cầu đăng nhập — chặn tại đây thay vì route
   * guard vì chỉ video bị hạn chế, các tab nội dung khác vẫn mở tự do cho khách chưa đăng nhập. */
  private requireLoginForVideo(): boolean {
    if (this.authService.isLoggedIn()) return true;

    this.toast.info('Vui lòng đăng nhập để xem video');
    this.router.navigate(['/login'], { queryParams: { returnUrl: this.router.url } });
    return false;
  }
}
