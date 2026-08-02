import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { trigger, transition, style, animate, query, stagger } from '@angular/animations';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { StatCardComponent } from '../../components/stat-card/stat-card.component';
import { CourseRowComponent } from '../../components/course-row/course-row.component';
import { ActivityItemComponent } from '../../components/activity-item/activity-item.component';
import { IconComponent } from '../../components/icon/icon.component';
import { Activity, DashboardCourse, DashboardCourseStatus, LeaderboardEntry, Stat } from '../../models/student-dashboard.model';
import { AuthService } from '../../../../core/auth/auth.service';
import { CurrentUser } from '../../../../core/auth/models/auth.model';
import { ToastService } from '../../../../shared/services/toast.service';
import { PlatformStatsService } from '../../../../clients/services/platform-stats.service';
import { RankingService } from '../../../../clients/services/ranking.service';
import { formatRelativeDate, formatTeacherInitials } from '../../../../clients/utils/format.util';
import { CourseService } from '../../../courses/services/course.service';
import { CourseListItem } from '../../../courses/models/course.model';
import { QuizLibraryService } from '../../../quizzes/services/quiz.service';
import { QuizItem } from '../../../quizzes/models/quiz.model';
import { DocumentService } from '../../../documents/services/document.service';
import { DocumentItem } from '../../../documents/models/document.model';

const listStagger = trigger('listStagger', [
  transition('* => *', [
    query(
      ':enter',
      [
        style({ opacity: 0, transform: 'translateY(6px)' }),
        stagger('60ms', [animate('220ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))])
      ],
      { optional: true }
    )
  ])
]);

const SUBJECT_BG_PALETTE = ['#EEF2FF', '#FEF3C7', '#ECFDF5', '#FEE2E2', '#ECFEFF', '#F3E8FF'];

const STATUS_STYLE: Record<DashboardCourseStatus, { label: string; bg: string; color: string }> = {
  Draft: { label: 'Bản nháp', bg: '#F1F5F9', color: '#64748B' },
  Published: { label: 'Đã xuất bản', bg: '#ECFDF5', color: '#047857' },
  Upcoming: { label: 'Sắp mở', bg: '#FEF9C3', color: '#A16207' }
};

interface TimedEntry {
  createdAt: string;
  activity: Activity;
}

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [RouterLink, StatCardComponent, CourseRowComponent, ActivityItemComponent, IconComponent],
  templateUrl: './student-dashboard.component.html',
  styleUrl: './student-dashboard.component.scss',
  animations: [listStagger]
})
export class StudentDashboardComponent implements OnInit {
  loading = true;
  currentUser: CurrentUser | null = null;
  greeting = '';
  roleLabel = '';

  readonly canViewCourses = this.auth.hasPermission('Courses', 'view');
  readonly canViewQuizzes = this.auth.hasPermission('Quizzes', 'view');
  readonly canViewDocuments = this.auth.hasPermission('Documents', 'view');
  readonly canViewStudents = this.auth.hasPermission('Students', 'view');
  readonly isAdmin = this.auth.getRole()?.toLowerCase() === 'admin';

  stats: Stat[] = [];
  recentCourses: DashboardCourse[] = [];
  topStudents: LeaderboardEntry[] = [];
  activities: Activity[] = [];

  constructor(
    private auth: AuthService,
    private toast: ToastService,
    private platformStatsService: PlatformStatsService,
    private rankingService: RankingService,
    private courseService: CourseService,
    private quizService: QuizLibraryService,
    private documentService: DocumentService
  ) {}

  ngOnInit(): void {
    this.auth.getCurrentUser().subscribe((user) => {
      this.currentUser = user;
      const name = user?.fullName ?? this.auth.getSession()?.username ?? '';
      this.greeting = `${this.greetingPrefix(new Date())}, ${name}! 👋`;
      this.roleLabel = this.roleLabelFor(user?.role ?? this.auth.getRole());
    });

    this.loading = true;
    forkJoin({
      platform: this.platformStatsService.getStats().pipe(catchError(() => of(null))),
      courses: this.canViewCourses
        ? this.courseService.getCourses(1, 5).pipe(catchError(() => of(null)))
        : of(null),
      quizzes: this.canViewQuizzes ? this.quizService.getAll().pipe(catchError(() => of(null))) : of(null),
      documents: this.canViewDocuments
        ? this.documentService.getAll().pipe(catchError(() => of(null)))
        : of(null),
      leaderboard: this.canViewStudents
        ? this.rankingService.getLeaderboard('month', undefined, undefined, 1, 5).pipe(catchError(() => of(null)))
        : of(null)
    }).subscribe(({ platform, courses, quizzes, documents, leaderboard }) => {
      const platformData = platform?.data ?? null;
      const courseItems = courses?.data?.items ?? [];
      const quizItems = quizzes?.data ?? [];
      const documentItems = documents?.data ?? [];
      const leaderboardItems = leaderboard?.items?.items ?? [];

      this.stats = this.buildStats(platformData, courses?.data?.totalCount, quizItems.length, documentItems.length);
      this.recentCourses = this.buildRecentCourses(courseItems);
      this.topStudents = leaderboardItems.map((entry) => ({
        rank: entry.rank,
        name: entry.fullName,
        initials: formatTeacherInitials(entry.fullName),
        points: entry.totalPoints
      }));
      this.activities = this.buildActivities(courseItems, quizItems, documentItems);
      this.loading = false;

      if (!platform) this.toast.error('Không thể tải số liệu tổng quan');
    });
  }

  private buildStats(
    platform: { studentsCount: number; teachersCount: number } | null,
    coursesTotal: number | undefined,
    quizzesTotal: number,
    documentsTotal: number
  ): Stat[] {
    const stats: Stat[] = [];

    if (this.canViewStudents && platform) {
      stats.push({
        icon: 'users',
        value: `${platform.studentsCount}`,
        label: 'Học viên',
        note: 'Toàn hệ thống',
        gradientFrom: '#67E8F9',
        gradientTo: '#0891B2',
        noteColor: '#0891B2',
        noteBg: '#ECFEFF'
      });
    }

    if (this.isAdmin && platform) {
      stats.push({
        icon: 'graduation-cap',
        value: `${platform.teachersCount}`,
        label: 'Giáo viên',
        note: 'Toàn hệ thống',
        gradientFrom: '#FDBA74',
        gradientTo: '#EA580C',
        noteColor: '#C2410C',
        noteBg: '#FFF7ED'
      });
    }

    if (this.canViewCourses && coursesTotal !== undefined) {
      stats.push({
        icon: 'book',
        value: `${coursesTotal}`,
        label: 'Khóa học',
        note: 'Tổng số',
        gradientFrom: '#A78BFA',
        gradientTo: '#6366F1',
        noteColor: '#4338CA',
        noteBg: '#EEF2FF'
      });
    }

    if (this.canViewQuizzes) {
      stats.push({
        icon: 'layers',
        value: `${quizzesTotal}`,
        label: 'Quiz',
        note: 'Đã tạo',
        gradientFrom: '#6EE7B7',
        gradientTo: '#059669',
        noteColor: '#047857',
        noteBg: '#ECFDF5'
      });
    }

    if (this.canViewDocuments) {
      stats.push({
        icon: 'file-text',
        value: `${documentsTotal}`,
        label: 'Tài liệu',
        note: 'Đã tạo',
        gradientFrom: '#FDA4AF',
        gradientTo: '#E11D48',
        noteColor: '#BE123C',
        noteBg: '#FFF1F2'
      });
    }

    return stats;
  }

  private buildRecentCourses(items: CourseListItem[]): DashboardCourse[] {
    return [...items]
      .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
      .slice(0, 5)
      .map((course, index) => {
        const status = this.normalizeStatus(course.status);
        const style = STATUS_STYLE[status];
        return {
          id: course.id,
          emoji: course.emoji || '📘',
          subjectBg: SUBJECT_BG_PALETTE[index % SUBJECT_BG_PALETTE.length],
          name: course.title,
          teacher: course.teacher || 'Chưa gán giáo viên',
          status,
          statusLabel: style.label,
          statusBg: style.bg,
          statusColor: style.color,
          createdLabel: formatRelativeDate(new Date(course.createdAt))
        };
      });
  }

  private buildActivities(courses: CourseListItem[], quizzes: QuizItem[], documents: DocumentItem[]): Activity[] {
    const entries: TimedEntry[] = [];

    if (this.canViewCourses) {
      for (const course of this.topByDate(courses, (c) => c.createdAt, 5)) {
        entries.push({
          createdAt: course.createdAt,
          activity: {
            icon: 'book',
            iconColor: '#4338CA',
            iconBg: '#EEF2FF',
            iconStrokeWidth: 2,
            text: `Khóa học mới: "${course.title}"`,
            time: formatRelativeDate(new Date(course.createdAt))
          }
        });
      }
    }

    if (this.canViewQuizzes) {
      for (const quiz of this.topByDate(quizzes, (q) => q.createdAt, 5)) {
        entries.push({
          createdAt: quiz.createdAt,
          activity: {
            icon: 'layers',
            iconColor: '#0891B2',
            iconBg: '#ECFEFF',
            iconStrokeWidth: 2,
            text: `Quiz mới: "${quiz.title}"`,
            time: formatRelativeDate(new Date(quiz.createdAt))
          }
        });
      }
    }

    if (this.canViewDocuments) {
      for (const doc of this.topByDate(documents, (d) => d.createdAt, 5)) {
        entries.push({
          createdAt: doc.createdAt,
          activity: {
            icon: 'file-text',
            iconColor: '#EA580C',
            iconBg: '#FFF7ED',
            iconStrokeWidth: 2,
            text: `Tài liệu mới: "${doc.title}"`,
            time: formatRelativeDate(new Date(doc.createdAt))
          }
        });
      }
    }

    return entries
      .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
      .slice(0, 6)
      .map((entry) => entry.activity);
  }

  private topByDate<T>(items: T[], getDate: (item: T) => string, count: number): T[] {
    return [...items].sort((a, b) => new Date(getDate(b)).getTime() - new Date(getDate(a)).getTime()).slice(0, count);
  }

  private normalizeStatus(status: string): DashboardCourseStatus {
    return status === 'Published' || status === 'Upcoming' ? status : 'Draft';
  }

  private roleLabelFor(role: string | null | undefined): string {
    switch (role?.toLowerCase()) {
      case 'admin':
        return 'Quản trị viên hệ thống';
      case 'teacher':
        return 'Giáo viên';
      default:
        return '';
    }
  }

  private greetingPrefix(date: Date): string {
    const hour = date.getHours();
    if (hour < 12) return 'Chào buổi sáng';
    if (hour < 18) return 'Chào buổi chiều';
    return 'Chào buổi tối';
  }
}
