import { Routes } from '@angular/router';

export const quizzesRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/quiz-list/quiz-list.component').then(
        (m) => m.QuizListComponent
      )
  }
];
