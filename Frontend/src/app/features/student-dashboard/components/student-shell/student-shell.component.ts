import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BreakpointObserver } from '@angular/cdk/layout';
import { StudentSidebarComponent } from '../student-sidebar/student-sidebar.component';
import { StudentDashboardHeaderComponent } from '../student-dashboard-header/student-dashboard-header.component';
import { ToastComponent } from '../../../../shared/components/toast/toast.component';
import { MOCK_STUDENT } from '../../data/mock-student.data';

const MOBILE_BREAKPOINT = '(max-width: 900px)';

@Component({
  selector: 'app-student-shell',
  standalone: true,
  imports: [RouterOutlet, StudentSidebarComponent, StudentDashboardHeaderComponent, ToastComponent],
  templateUrl: './student-shell.component.html',
  styleUrl: './student-shell.component.scss'
})
export class StudentShellComponent {
  private breakpointObserver = inject(BreakpointObserver);
  private destroyRef = inject(DestroyRef);

  readonly student = MOCK_STUDENT;
  readonly dateLabel = this.formatVietnameseDate(new Date());

  isMobile = signal(false);
  collapsed = signal(false);
  mobileOpen = signal(false);

  showBackdrop = computed(() => this.isMobile() && this.mobileOpen());

  get initials(): string {
    const parts = this.student.name.trim().split(' ');
    return parts[parts.length - 1]?.[0] ?? 'B';
  }

  constructor() {
    this.breakpointObserver
      .observe(MOBILE_BREAKPOINT)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((state) => {
        this.isMobile.set(state.matches);
        this.collapsed.set(state.matches);
        this.mobileOpen.set(false);
      });
  }

  toggleSidebar(): void {
    if (this.isMobile()) {
      this.mobileOpen.update((v) => !v);
    } else {
      this.collapsed.update((v) => !v);
    }
  }

  closeMobileSidebar(): void {
    this.mobileOpen.set(false);
  }

  private formatVietnameseDate(date: Date): string {
    const weekdays = ['Chủ Nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy'];
    return `${weekdays[date.getDay()]}, ${date.getDate()} Tháng ${date.getMonth() + 1}`;
  }
}
