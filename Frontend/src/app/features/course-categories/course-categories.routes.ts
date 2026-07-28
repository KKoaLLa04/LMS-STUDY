import { Routes } from '@angular/router';

export const courseCategoriesRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/course-category-list/course-category-list.component').then(
        (m) => m.CourseCategoryListComponent
      )
  }
];
