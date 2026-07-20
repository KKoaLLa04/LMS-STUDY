import { Routes } from '@angular/router';

export const khoiHocRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/khoi-hoc-list/khoi-hoc-list.component').then(
        (m) => m.KhoiHocListComponent
      )
  }
];
