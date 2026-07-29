import { Component, Input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';
import { IconComponent } from '../icon/icon.component';
import { IconName } from '../../models/student-dashboard.model';

interface NavItem {
  icon: IconName;
  label: string;
  link: boolean;
  route?: string;
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

  readonly navItems: NavItem[] = [
    { icon: 'home', label: 'Tổng quan', link: true, route: '/dashboard' },
    { icon: 'award', label: 'Thành tích', link: true, route: '/achievements' },
    { icon: 'layers', label: 'Khối học', link: true, route: '/khoi-hoc' },
    { icon: 'tag', label: 'Danh mục khóa học', link: true, route: '/course-categories' },
    { icon: 'book', label: 'Khóa học', link: true, route: '/courses' },
    { icon: 'file-text', label: 'Tài liệu', link: true, route: '/documents' },
    { icon: 'help-circle', label: 'Quiz', link: true, route: '/quizzes' },
    { icon: 'graduation-cap', label: 'Danh sách giảng viên', link: true, route: '/teachers' },
    { icon: 'users', label: 'Danh sách học sinh', link: true, route: '/students' },
    { icon: 'message-circle', label: 'Thảo luận', link: true, route: '/discussion-forums' }
  ];

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
