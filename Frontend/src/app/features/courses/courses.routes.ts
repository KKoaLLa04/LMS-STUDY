import { Routes } from '@angular/router';

export const coursesRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/course-list/course-list.component').then(
        (m) => m.CourseListComponent
      )
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./pages/course-preview/course-preview.component').then(
        (m) => m.CoursePreviewComponent
      )
  }
];
