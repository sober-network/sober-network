import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, EMPTY, Observable, throwError, tap } from 'rxjs';
import {
  AuthResponse, CurrentUser, LoginRequest, RegisterRequest,
  RefreshTokenRequest, ForgotPasswordRequest, ResetPasswordRequest,
  ConfirmEmailRequest, RevokeTokenRequest
} from '@app/core/models';
import { environment } from '../../../environments/environment';

const REFRESH_TOKEN_KEY = 'sn_refresh_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly base = `${environment.apiUrl}/api/auth`;

  // Access token and user live only in memory — never written to localStorage.
  // This limits XSS exposure to the current page lifetime.
  private readonly _currentUser$ = new BehaviorSubject<CurrentUser | null>(null);
  readonly currentUser$ = this._currentUser$.asObservable();

  get currentUser(): CurrentUser | null { return this._currentUser$.value; }
  get isLoggedIn(): boolean { return !!this._currentUser$.value; }
  get isSuperAdmin(): boolean { return this._currentUser$.value?.isSuperAdmin ?? false; }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.base}/login`, request).pipe(
      tap(r => this.handleAuthResponse(r))
    );
  }

  register(request: RegisterRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/register`, request);
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (!refreshToken) return throwError(() => new Error('No refresh token available'));
    const body: RefreshTokenRequest = { refreshToken };
    return this.http.post<AuthResponse>(`${this.base}/refresh-token`, body).pipe(
      tap(r => this.handleAuthResponse(r))
    );
  }

  forgotPassword(request: ForgotPasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/forgot-password`, request);
  }

  resetPassword(request: ResetPasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/reset-password`, request);
  }

  confirmEmail(request: ConfirmEmailRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/confirm-email`, request);
  }

  logout(): void {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (refreshToken) {
      const body: RevokeTokenRequest = { refreshToken };
      // Fire-and-forget — clear local state regardless of server response
      this.http.post(`${this.base}/revoke-token`, body).subscribe({ error: () => {} });
    }
    this.clearSession();
    this.router.navigate(['/auth/login']);
  }

  /** Attempt silent token refresh on app startup using stored refresh token. */
  tryRestoreSession(): Observable<AuthResponse> {
    if (!localStorage.getItem(REFRESH_TOKEN_KEY)) return EMPTY;
    return this.refreshToken();
  }

  private handleAuthResponse(response: AuthResponse): void {
    const user: CurrentUser = {
      userId: response.userId,
      displayName: response.displayName,
      email: response.email,
      isSuperAdmin: response.isSuperAdmin,
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      expiresAt: new Date(response.expiresAt),
    };
    this._currentUser$.next(user);
    localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
  }

  private clearSession(): void {
    this._currentUser$.next(null);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
  }
}
