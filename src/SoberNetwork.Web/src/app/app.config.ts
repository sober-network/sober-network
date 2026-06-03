import { APP_INITIALIZER, ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { lastValueFrom, of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

import { routes } from './app.routes';
import { AuthService } from './core/services/auth.service';
import { ClientLogService } from './core/services/client-log.service';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { timeoutInterceptor } from './core/interceptors/timeout.interceptor';
import { loggingInterceptor } from './core/interceptors/logging.interceptor';

function initializeSession(auth: AuthService, clientLog: ClientLogService) {
  return () => {
    clientLog.info('APP_INITIALIZER: starting session restore');
    return lastValueFrom(
      auth.tryRestoreSession().pipe(
        tap(r => clientLog.info('APP_INITIALIZER: session restored', { userId: r.userId ?? '' })),
        catchError(err => {
          clientLog.warn('APP_INITIALIZER: restore failed', { message: String(err) });
          return of(null);
        })
      ),
      { defaultValue: null }
    ).then(result => {
      clientLog.info('APP_INITIALIZER: complete', { loggedIn: String(auth.isLoggedIn) });
      return result;
    });
  };
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(withInterceptors([loggingInterceptor, timeoutInterceptor, authInterceptor, errorInterceptor])),
    {
      provide: APP_INITIALIZER,
      useFactory: initializeSession,
      deps: [AuthService, ClientLogService],
      multi: true,
    },
  ]
};
