import { Routes } from '@angular/router';
import { StudentShellComponent } from './features/student-dashboard/components/student-shell/student-shell.component';
import { authGuard, adminOnlyGuard, modulePermissionGuard, guestGuard } from './core/auth/auth.guard';

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
    canActivate: [guestGuard],
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
        canActivate: [modulePermissionGuard('Courses')],
        loadChildren: () =>
          import('./features/courses/courses.routes').then(
            (m) => m.coursesRoutes
          ),
      },
      {
        path: 'khoi-hoc',
        canActivate: [modulePermissionGuard('KhoiHocs')],
        loadChildren: () =>
          import('./features/khoi-hoc/khoi-hoc.routes').then(
            (m) => m.khoiHocRoutes
          ),
      },
      {
        path: 'course-categories',
        canActivate: [modulePermissionGuard('CourseCategories')],
        loadChildren: () =>
          import('./features/course-categories/course-categories.routes').then(
            (m) => m.courseCategoriesRoutes
          ),
      },
      {
        path: 'achievements',
        canActivate: [modulePermissionGuard('Achievements')],
        loadChildren: () =>
          import('./features/achievements/achievements.routes').then(
            (m) => m.achievementsRoutes
          ),
      },
      {
        path: 'documents',
        canActivate: [modulePermissionGuard('Documents')],
        loadChildren: () =>
          import('./features/documents/documents.routes').then(
            (m) => m.documentsRoutes
          ),
      },
      {
        path: 'quizzes',
        canActivate: [modulePermissionGuard('Quizzes')],
        loadChildren: () =>
          import('./features/quizzes/quizzes.routes').then(
            (m) => m.quizzesRoutes
          ),
      },
      {
        // Quản lý tài khoản Teacher luôn là đặc quyền riêng của Admin — không nằm trong hệ
        // thống phân quyền module (tránh giáo viên tự nâng quyền cho chính mình/đồng nghiệp).
        path: 'teachers',
        canActivate: [adminOnlyGuard],
        loadChildren: () =>
          import('./features/users/teachers.routes').then((m) => m.teachersRoutes),
      },
      {
        path: 'students',
        canActivate: [modulePermissionGuard('Students')],
        loadChildren: () =>
          import('./features/users/students.routes').then((m) => m.studentsRoutes),
      },
      {
        path: 'discussion-forums',
        canActivate: [modulePermissionGuard('DiscussionForums')],
        loadChildren: () =>
          import('./features/discussion-forums/discussion-forums.routes').then(
            (m) => m.discussionForumsRoutes
          ),
      },
      {
        // Quản lý nhóm quyền + gán thành viên luôn là đặc quyền riêng của Admin — không nằm
        // trong hệ thống phân quyền module, cùng lý do như route 'teachers' ở trên.
        path: 'permissions',
        canActivate: [adminOnlyGuard],
        loadChildren: () =>
          import('./features/permission-groups/permission-groups.routes').then(
            (m) => m.permissionGroupsRoutes
          ),
      },
    ],
  },
];
