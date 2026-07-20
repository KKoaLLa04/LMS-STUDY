import { Routes } from '@angular/router';

export const khoiHocRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/khoi-hoc-list/khoi-hoc-list.component').then(
        (m) => m.KhoiHocListComponent
      )
  },
  {
    path: 'create',
    loadComponent: () =>
      import('./pages/khoi-hoc-create/khoi-hoc-create.component').then(
        (m) => m.KhoiHocCreateComponent
      )
  },
  {
    path: ':id/edit',
    loadComponent: () =>
      import('./pages/khoi-hoc-edit/khoi-hoc-edit.component').then(
        (m) => m.KhoiHocEditComponent
      )
  }
];
