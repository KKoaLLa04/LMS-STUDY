import { Routes } from '@angular/router';

export const studentsRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/user-list/user-list.component').then((m) => m.UserListComponent),
    data: {
      role: 'Student',
      pageTitle: 'Danh sách học sinh',
      entityLabel: 'học sinh',
      createLabel: 'Thêm học sinh'
    }
  }
];
