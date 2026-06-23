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
    path: 'create',
    loadComponent: () =>
      import('./pages/course-create/course-create.component').then(
        (m) => m.CourseCreateComponent
      )
  },
  {
    path: ':id/edit',
    loadComponent: () =>
      import('./pages/course-edit/course-edit.component').then(
        (m) => m.CourseEditComponent
      )
  }
];
