import { Routes } from '@angular/router';
import { LayoutComponent } from './layout/layout.component';

export const routes: Routes = [
  // Landing page — hiển thị độc lập, không qua admin layout
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () =>
      import('./pages/landing/landing.component').then(
        (m) => m.LandingComponent
      ),
  },
  // Admin routes — sử dụng LayoutComponent (sidebar + navbar)
  {
    path: '',
    component: LayoutComponent,
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./pages/dashboard/dashboard.component').then(
            (m) => m.DashboardComponent
          ),
      },
      {
        path: 'courses',
        loadChildren: () =>
          import('./features/courses/courses.routes').then(
            (m) => m.coursesRoutes
          ),
      },
    ],
  },
];
