import { Routes } from '@angular/router';

export const discussionForumsRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/discussion-forum-list/discussion-forum-list.component').then(
        (m) => m.DiscussionForumListComponent
      )
  }
];
