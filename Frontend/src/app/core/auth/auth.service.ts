import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, of, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CurrentUser, LoginRequest, LoginResponse, PermissionModule, TeacherPermission } from './models/auth.model';

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
}

interface StoredSession {
  username: string;
  role: string;
  permissions: TeacherPermission[];
}

type PermissionCrudAction = 'view' | 'create' | 'update' | 'delete';

const TOKEN_KEY = 'lms_auth_token';
const USER_KEY = 'lms_auth_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = `${environment.apiBaseUrl}/auth`;

  constructor(private http: HttpClient, private router: Router) {}

  login(username: string, password: string): Observable<ApiResponse<LoginResponse>> {
    const body: LoginRequest = { username, password };
    return this.http.post<ApiResponse<LoginResponse>>(`${this.baseUrl}/login`, body).pipe(
      tap((res) => {
        if (res.data) this.persistSession(res.data);
      })
    );
  }

  getCurrentUser(): Observable<CurrentUser | null> {
    return this.http.get<ApiResponse<CurrentUser>>(`${this.baseUrl}/me`).pipe(
      map((res) => res.data ?? null),
      tap((user) => {
        // Đồng bộ lại quyền/role trong session — cho phép Admin đổi quyền của giáo viên có
        // hiệu lực ngay khi FE gọi lại /me (vd. sau khi điều hướng), không cần đăng nhập lại.
        if (user) this.updateStoredPermissions(user.role, user.permissions ?? []);
      }),
      catchError(() => of(null))
    );
  }

  changePassword(currentPassword: string, newPassword: string): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${this.baseUrl}/change-password`, {
      currentPassword,
      newPassword,
    });
  }

  logout(): void {
    this.http
      .post(`${this.baseUrl}/logout`, {})
      .pipe(catchError(() => of(null)))
      .subscribe(() => this.clearSession());
  }

  clearSession(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  getSession(): StoredSession | null {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? (JSON.parse(raw) as StoredSession) : null;
  }

  getRole(): string | null {
    return this.getSession()?.role ?? null;
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  getPermissions(): TeacherPermission[] {
    return this.getSession()?.permissions ?? [];
  }

  /** Admin luôn có toàn quyền; Student không có quyền quản trị nào; Teacher tra theo module đã được cấp. */
  hasPermission(module: PermissionModule, action: PermissionCrudAction): boolean {
    const role = this.getRole()?.toLowerCase();
    if (role === 'admin') return true;
    if (role !== 'teacher') return false;

    const permission = this.getPermissions().find((p) => p.module === module);
    if (!permission) return false;

    switch (action) {
      case 'view':
        return permission.canView;
      case 'create':
        return permission.canCreate;
      case 'update':
        return permission.canUpdate;
      case 'delete':
        return permission.canDelete;
    }
  }

  private persistSession(data: LoginResponse): void {
    localStorage.setItem(TOKEN_KEY, data.token);
    const session: StoredSession = { username: data.username, role: data.role, permissions: data.permissions ?? [] };
    localStorage.setItem(USER_KEY, JSON.stringify(session));
  }

  private updateStoredPermissions(role: string, permissions: TeacherPermission[]): void {
    const current = this.getSession();
    if (!current) return;
    const session: StoredSession = { ...current, role, permissions };
    localStorage.setItem(USER_KEY, JSON.stringify(session));
  }
}
