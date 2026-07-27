import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse } from './models/auth.model';

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
}

interface StoredSession {
  username: string;
  role: string;
}

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

  private persistSession(data: LoginResponse): void {
    localStorage.setItem(TOKEN_KEY, data.token);
    const session: StoredSession = { username: data.username, role: data.role };
    localStorage.setItem(USER_KEY, JSON.stringify(session));
  }
}
