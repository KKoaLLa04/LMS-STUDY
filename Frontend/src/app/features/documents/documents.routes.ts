import { Routes } from '@angular/router';

export const documentsRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/document-list/document-list.component').then(
        (m) => m.DocumentListComponent
      )
  }
];
