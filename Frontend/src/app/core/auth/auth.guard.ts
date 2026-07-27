import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const isAdmin = authService.isLoggedIn() && authService.getRole()?.toLowerCase() === 'admin';
  if (isAdmin) return true;

  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};
