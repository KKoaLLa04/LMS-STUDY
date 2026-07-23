import { Routes } from '@angular/router';
import { StudentShellComponent } from './features/student-dashboard/components/student-shell/student-shell.component';

export const routes: Routes = [
  // Landing page — hiển thị độc lập, không qua shell
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () =>
      import('./pages/landing/landing.component').then(
        (m) => m.LandingComponent
      ),
  },
  // Shell chung (sidebar + header mới) cho toàn bộ các trang sau đăng nhập,
  // để điều hướng giữa các trang không bị đổi lại sidebar/header cũ
  {
    path: '',
    component: StudentShellComponent,
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/student-dashboard/pages/dashboard/student-dashboard.component').then(
            (m) => m.StudentDashboardComponent
          ),
      },
      {
        path: 'courses',
        loadChildren: () =>
          import('./features/courses/courses.routes').then(
            (m) => m.coursesRoutes
          ),
      },
      {
        path: 'khoi-hoc',
        loadChildren: () =>
          import('./features/khoi-hoc/khoi-hoc.routes').then(
            (m) => m.khoiHocRoutes
          ),
      },
    ],
  },
];
