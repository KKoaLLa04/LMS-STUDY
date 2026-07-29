import { Routes } from '@angular/router';

export const clientsRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./layout/client-shell/client-shell.component').then((m) => m.ClientShellComponent),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./pages/home/home.component').then((m) => m.HomeComponent),
      },
      {
        path: 'khoa-hoc',
        loadComponent: () =>
          import('./pages/course-list/course-list.component').then((m) => m.CourseListComponent),
      },
      {
        path: 'bang-xep-hang',
        loadComponent: () =>
          import('./pages/leaderboard/leaderboard.component').then((m) => m.LeaderboardComponent),
      },
      {
        path: 'thanh-tich',
        loadComponent: () =>
          import('./pages/achievements/achievements.component').then((m) => m.AchievementsComponent),
      },
      {
        path: 'giao-vien',
        loadComponent: () =>
          import('./pages/teachers/teachers.component').then((m) => m.TeachersComponent),
      },
      {
        path: 'giao-vien/:id',
        loadComponent: () =>
          import('./pages/teacher-detail/teacher-detail.component').then((m) => m.TeacherDetailComponent),
      },
      {
        path: 'ho-so',
        loadComponent: () =>
          import('./pages/student-profile/student-profile.component').then((m) => m.StudentProfileComponent),
      },
      {
        path: 'doi-mat-khau',
        loadComponent: () =>
          import('./pages/change-password/change-password.component').then((m) => m.ChangePasswordComponent),
      },
      {
        path: 'tai-lieu',
        loadComponent: () =>
          import('./pages/document-list/document-list.component').then((m) => m.DocumentListComponent),
      },
      {
        path: 'tai-lieu/:id',
        loadComponent: () =>
          import('./pages/document-detail/document-detail.component').then((m) => m.DocumentDetailComponent),
      },
      {
        path: 'quiz',
        loadComponent: () =>
          import('./pages/quiz-list/quiz-list.component').then((m) => m.QuizListComponent),
      },
      {
        path: 'quiz/:id',
        loadComponent: () =>
          import('./pages/quiz-detail/quiz-detail.component').then((m) => m.QuizDetailComponent),
      },
      {
        path: ':id',
        loadComponent: () =>
          import('./pages/course-detail/course-detail.component').then((m) => m.CourseDetailComponent),
      },
    ],
  },
];
