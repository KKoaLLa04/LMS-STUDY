import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { CourseService } from '../../services/course.service';
import { CourseDetail, LessonResponse, SectionDetail } from '../../models/course.model';
import { UploadService } from '../../../../shared/services/upload.service';

const DEFAULT_EMOJI = '📘';

function toEmbedUrl(url: string): string | null {
  let u: URL;
  try {
    u = new URL(url);
  } catch {
    return null;
  }
  const host = u.hostname.replace(/^www\.|^m\./, '');

  if (host === 'youtube.com') {
    const id = u.searchParams.get('v');
    if (id) return `https://www.youtube.com/embed/${id}`;
    const match = u.pathname.match(/^\/(shorts|embed|live)\/([^/?]+)/);
    return match ? `https://www.youtube.com/embed/${match[2]}` : null;
  }
  if (host === 'youtu.be') {
    const id = u.pathname.slice(1);
    return id ? `https://www.youtube.com/embed/${id}` : null;
  }
  if (host === 'vimeo.com') {
    const id = u.pathname.split('/').filter(Boolean)[0];
    return id && /^\d+$/.test(id) ? `https://player.vimeo.com/video/${id}` : null;
  }
  return null;
}

@Component({
  selector: 'app-course-preview',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './course-preview.component.html',
  styleUrl: './course-preview.component.scss'
})
export class CoursePreviewComponent implements OnInit {
  course: CourseDetail | null = null;
  loading = false;
  errorMessage = '';

  selectedSection: SectionDetail | null = null;
  selectedLesson: LessonResponse | null = null;
  selectedEmbedUrl: SafeResourceUrl | null = null;
  /** Signed URL để phát video R2 (bucket private) — phải xin lại mỗi lần chọn lesson khác vì
   * lesson.videoUrl giờ chỉ là object key, không phát trực tiếp được. */
  selectedPlaybackUrl: string | null = null;
  loadingPlayback = false;
  collapsedSectionIds = new Set<number>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private courseService: CourseService,
    private sanitizer: DomSanitizer,
    private uploadService: UploadService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.errorMessage = 'Khóa học không hợp lệ.';
      return;
    }
    this.loadCourse(id);
  }

  loadCourse(id: number): void {
    this.loading = true;
    this.errorMessage = '';
    this.courseService.getCourseById(id).subscribe({
      next: (res) => {
        this.loading = false;
        if (!res.success || !res.data) {
          this.errorMessage = res.message || 'Không tìm thấy khóa học.';
          return;
        }
        this.course = res.data;
        const firstSection = this.course.sections[0];
        const firstLesson = firstSection?.lessons[0];
        if (firstSection && firstLesson) {
          this.selectLesson(firstSection, firstLesson);
        }
      },
      error: () => {
        this.loading = false;
        this.errorMessage = 'Không thể tải dữ liệu khóa học. Vui lòng thử lại.';
      }
    });
  }

  selectLesson(section: SectionDetail, lesson: LessonResponse): void {
    this.selectedSection = section;
    this.selectedLesson = lesson;
    this.selectedPlaybackUrl = null;

    const embedUrl = lesson.videoUrl ? toEmbedUrl(lesson.videoUrl) : null;
    this.selectedEmbedUrl = embedUrl ? this.sanitizer.bypassSecurityTrustResourceUrl(embedUrl) : null;

    if (!embedUrl && lesson.lessonType === 'Video' && lesson.videoUrl) {
      this.loadingPlayback = true;
      this.uploadService.getLessonVideoPlaybackUrl(lesson.id).subscribe({
        next: (res) => {
          this.loadingPlayback = false;
          if (res.success && res.data) this.selectedPlaybackUrl = res.data.url;
        },
        error: () => {
          this.loadingPlayback = false;
        }
      });
    }
  }

  toggleSection(sectionId: number): void {
    if (this.collapsedSectionIds.has(sectionId)) {
      this.collapsedSectionIds.delete(sectionId);
    } else {
      this.collapsedSectionIds.add(sectionId);
    }
  }

  isSectionCollapsed(sectionId: number): boolean {
    return this.collapsedSectionIds.has(sectionId);
  }

  editCourse(): void {
    if (!this.course) return;
    this.router.navigate(['/courses'], { queryParams: { edit: this.course.id } });
  }

  get totalLessons(): number {
    return this.course?.sections.reduce((sum, s) => sum + s.lessons.length, 0) ?? 0;
  }

  get emoji(): string {
    return this.course?.emoji || DEFAULT_EMOJI;
  }

  formatPrice(price: number): string {
    return price === 0 ? 'Miễn phí' : price.toLocaleString('vi-VN') + ' đ';
  }

  getStatusClass(status: string): string {
    if (status === 'Published') return 'status-published';
    if (status === 'Upcoming') return 'status-upcoming';
    return 'status-draft';
  }

  getStatusLabel(status: string): string {
    if (status === 'Published') return 'Đang học';
    if (status === 'Upcoming') return 'Sắp tới';
    return 'Bản nháp';
  }

  lessonIcon(lessonType: string): string {
    if (lessonType === 'Video') return 'bi-play-circle';
    if (lessonType === 'Quiz') return 'bi-patch-question';
    return 'bi-file-earmark-text';
  }

  lessonTypeLabel(lessonType: string): string {
    if (lessonType === 'Video') return 'Video';
    if (lessonType === 'Quiz') return 'Quiz';
    return 'Tài liệu';
  }
}
