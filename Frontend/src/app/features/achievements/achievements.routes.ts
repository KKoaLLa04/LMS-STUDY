import { Routes } from '@angular/router';

export const achievementsRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/achievement-list/achievement-list.component').then(
        (m) => m.AchievementListComponent
      )
  }
];
