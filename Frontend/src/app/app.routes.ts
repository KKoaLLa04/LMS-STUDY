import { Routes } from '@angular/router';
import { StudentShellComponent } from './features/student-dashboard/components/student-shell/student-shell.component';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  // Landing page — hiển thị độc lập, không qua shell
  // {
  //   path: '',
  //   pathMatch: 'full',
  //   loadComponent: () =>
  //     import('./pages/landing/landing.component').then(
  //       (m) => m.LandingComponent
  //     ),
  // },
  {
    path: '',
    pathMatch: 'full',
    loadChildren: () =>
      import('./clients/clients.routes').then((m) => m.clientsRoutes),
  },
  // Đăng nhập — public, ngoài shell
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/pages/login/login.component').then(
        (m) => m.LoginComponent
      ),
  },
  // Khu vực học sinh (client) — giao diện Organic riêng, độc lập với shell admin.
  {
    path: 'client',
    loadChildren: () =>
      import('./clients/clients.routes').then((m) => m.clientsRoutes),
  },
  // Shell chung (sidebar + header mới) cho toàn bộ các trang sau đăng nhập,
  // để điều hướng giữa các trang không bị đổi lại sidebar/header cũ.
  // Toàn bộ khu vực này yêu cầu đăng nhập admin (authGuard).
  {
    path: '',
    component: StudentShellComponent,
    canActivate: [authGuard],
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
