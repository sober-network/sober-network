import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '@app/core/services/auth.service';

let isRefreshing = false;

/**
 * On 401: attempt one silent token refresh, then retry the original request.
 * If refresh also fails, logout and redirect to /auth/login.
 */
export const errorInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const auth = inject(AuthService);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401) {
        return throwError(() => error);
      }

      // Don't retry refresh/login endpoints — would cause an infinite loop
      if (req.url.includes('/auth/')) {
        auth.logout();
        return throwError(() => error);
      }

      if (isRefreshing) {
        auth.logout();
        return throwError(() => error);
      }

      isRefreshing = true;
      return auth.refreshToken().pipe(
        switchMap(response => {
          isRefreshing = false;
          // Retry original request with new token
          return next(req.clone({
            setHeaders: { Authorization: `Bearer ${response.accessToken}` }
          }));
        }),
        catchError(refreshError => {
          isRefreshing = false;
          auth.logout();
          return throwError(() => refreshError);
        })
      );
    })
  );
};
