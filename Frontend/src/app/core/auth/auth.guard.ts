import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { PermissionModule } from './models/auth.model';

// Cho phép Admin và Teacher vào shell quản trị — trước đây chỉ Admin mới vào được, khiến Teacher
// bị đá thẳng về /login dù được cấp quyền module nào đi nữa. Việc ẩn/chặn theo từng module cụ
// thể do modulePermissionGuard (bên dưới) và sidebar đảm nhiệm.
export const authGuard: CanActivateFn = (_route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const role = authService.isLoggedIn() ? authService.getRole()?.toLowerCase() : null;
  if (role === 'admin' || role === 'teacher') return true;

  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

// Chặn người đã đăng nhập quay lại /login — Admin/Teacher vào thẳng /dashboard, Student về trang chủ.
export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isLoggedIn()) return true;

  const role = authService.getRole()?.toLowerCase();
  if (role === 'admin' || role === 'teacher') return router.createUrlTree(['/dashboard']);

  return router.createUrlTree(['/']);
};

// Chỉ Admin mới được vào — dùng cho các route cố tình không nằm trong hệ thống phân quyền module
// (vd. quản lý tài khoản Teacher), tránh giáo viên tự nâng quyền cho chính mình/đồng nghiệp.
export const adminOnlyGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.getRole()?.toLowerCase() === 'admin') return true;

  return router.createUrlTree(['/dashboard']);
};

// Admin luôn qua; Teacher chỉ qua nếu được cấp quyền Xem (View) trên module — nếu không, điều
// hướng về /dashboard thay vì để lộ trang trống/gọi API rồi nhận toàn 403.
export function modulePermissionGuard(module: PermissionModule): CanActivateFn {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (authService.hasPermission(module, 'view')) return true;

    return router.createUrlTree(['/dashboard']);
  };
}
