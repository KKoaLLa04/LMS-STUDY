import { Routes } from '@angular/router';

export const permissionGroupsRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/permission-group-list/permission-group-list.component').then(
        (m) => m.PermissionGroupListComponent
      )
  }
];
