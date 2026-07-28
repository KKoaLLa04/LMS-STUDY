import { Routes } from '@angular/router';

export const teachersRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/user-list/user-list.component').then((m) => m.UserListComponent),
    data: {
      role: 'Teacher',
      pageTitle: 'Danh sách giảng viên',
      entityLabel: 'giảng viên',
      createLabel: 'Thêm giảng viên'
    }
  }
];
