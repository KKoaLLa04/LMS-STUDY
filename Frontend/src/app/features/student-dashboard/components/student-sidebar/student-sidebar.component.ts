import { Component, Input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';
import { IconComponent } from '../icon/icon.component';
import { IconName } from '../../models/student-dashboard.model';
import { AuthService } from '../../../../core/auth/auth.service';
import { PermissionModule } from '../../../../core/auth/models/auth.model';

interface NavItem {
  icon: IconName;
  label: string;
  link: boolean;
  route?: string;
  /** Không có module = luôn hiện (vd. Tổng quan). Có module thì cần quyền Xem trên module đó. */
  module?: PermissionModule;
  /** Mục chỉ Admin mới thấy — không nằm trong hệ thống phân quyền module (vd. Danh sách giảng viên). */
  adminOnly?: boolean;
}

@Component({
  selector: 'app-student-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, IconComponent],
  templateUrl: './student-sidebar.component.html',
  styleUrl: './student-sidebar.component.scss',
  animations: [
    trigger('fadeLabel', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('160ms ease', style({ opacity: 1 }))
      ]),
      transition(':leave', [
        animate('120ms ease', style({ opacity: 0 }))
      ])
    ])
  ]
})
export class StudentSidebarComponent {
  @Input() collapsed = false;
  @Input() isMobile = false;
  @Input() mobileOpen = false;
  @Input({ required: true }) studentName!: string;
  @Input({ required: true }) grade!: string;
  @Input({ required: true }) level!: number;

  constructor(private authService: AuthService) {}

  private readonly allNavItems: NavItem[] = [
    { icon: 'home', label: 'Tổng quan', link: true, route: '/dashboard' },
    { icon: 'award', label: 'Thành tích', link: true, route: '/achievements', module: 'Achievements' },
    { icon: 'layers', label: 'Khối học', link: true, route: '/khoi-hoc', module: 'KhoiHocs' },
    { icon: 'tag', label: 'Danh mục khóa học', link: true, route: '/course-categories', module: 'CourseCategories' },
    { icon: 'book', label: 'Khóa học', link: true, route: '/courses', module: 'Courses' },
    { icon: 'file-text', label: 'Tài liệu', link: true, route: '/documents', module: 'Documents' },
    { icon: 'help-circle', label: 'Quiz', link: true, route: '/quizzes', module: 'Quizzes' },
    { icon: 'graduation-cap', label: 'Danh sách giảng viên', link: true, route: '/teachers', adminOnly: true },
    { icon: 'users', label: 'Danh sách học sinh', link: true, route: '/students', module: 'Students' },
    { icon: 'message-circle', label: 'Thảo luận', link: true, route: '/discussion-forums', module: 'DiscussionForums' },
    { icon: 'shield', label: 'Phân quyền', link: true, route: '/permissions', adminOnly: true }
  ];

  // Admin luôn thấy toàn bộ menu; Teacher chỉ thấy mục không yêu cầu module (Tổng quan) và mục
  // có quyền Xem — ẩn hẳn "Danh sách giảng viên" (adminOnly) khỏi Teacher thay vì để bấm vào rồi
  // bị điều hướng ngược lại bởi adminOnlyGuard.
  get navItems(): NavItem[] {
    const isAdmin = this.authService.getRole()?.toLowerCase() === 'admin';
    return this.allNavItems.filter((item) => {
      if (item.adminOnly) return isAdmin;
      if (!item.module) return true;
      return isAdmin || this.authService.hasPermission(item.module, 'view');
    });
  }

  get showLabels(): boolean {
    // On mobile the sidebar is either fully off-screen or fully open at 250px —
    // "collapsed" only means the desktop icon-only rail, so labels should always
    // show once the mobile drawer is visible (off-screen positioning already
    // handles hiding it, not label visibility).
    if (this.isMobile) return true;
    return !this.collapsed;
  }

  get initials(): string {
    const parts = this.studentName.trim().split(' ');
    return parts[parts.length - 1]?.[0] ?? 'B';
  }

  get width(): string {
    return this.isMobile ? '250px' : this.collapsed ? '84px' : '250px';
  }

  get position(): string {
    return this.isMobile ? 'fixed' : 'relative';
  }

  get left(): string {
    if (!this.isMobile) return '0px';
    return this.mobileOpen ? '0px' : '-260px';
  }
}
