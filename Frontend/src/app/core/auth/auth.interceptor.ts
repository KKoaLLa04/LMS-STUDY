import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  const token = authService.getToken();
  const isApiRequest = req.url.startsWith(environment.apiBaseUrl);
  const hasToken = !!token && isApiRequest;

  const authReq = hasToken
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Chỉ tự đăng xuất khi request ĐÃ đính kèm token mà vẫn bị 401
      // (phiên hết hạn/không hợp lệ) — không áp dụng cho chính request
      // đăng nhập sai mật khẩu, vì lúc đó chưa hề có token nào cả.
      if (error.status === 401 && hasToken) {
        authService.clearSession();
      }
      return throwError(() => error);
    })
  );
};
