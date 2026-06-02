import { HttpInterceptorFn } from '@angular/common/http';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export const loggingInterceptor: HttpInterceptorFn = (req, next) => {
  if (environment.production) return next(req);

  const start = Date.now();
  console.debug(`[HTTP] ▶ ${req.method} ${req.url}`);

  return next(req).pipe(
    tap({
      next: event => {
        // Only log the final response, not progress events
        if ((event as { status?: number }).status !== undefined) {
          const ms = Date.now() - start;
          console.debug(`[HTTP] ✔ ${req.method} ${req.url} — ${ (event as { status: number }).status } (${ms}ms)`);
        }
      },
      error: err => {
        const ms = Date.now() - start;
        console.error(`[HTTP] ✖ ${req.method} ${req.url} — status=${err?.status ?? 'network'} (${ms}ms)`, err?.error ?? err);
      },
    })
  );
};
