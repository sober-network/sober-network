import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ClientLogService } from '../services/client-log.service';

export const loggingInterceptor: HttpInterceptorFn = (req, next) => {
  // Never log the log endpoint itself — avoids infinite recursion.
  if (req.url.includes('/api/client-log')) return next(req);

  const clientLog = inject(ClientLogService);
  const start = Date.now();

  if (!environment.production) {
    console.debug(`[HTTP] ▶ ${req.method} ${req.url}`);
  }

  return next(req).pipe(
    tap({
      next: event => {
        if ((event as { status?: number }).status !== undefined) {
          const ms = Date.now() - start;
          if (!environment.production) {
            console.debug(`[HTTP] ✔ ${req.method} ${req.url} — ${(event as { status: number }).status} (${ms}ms)`);
          }
        }
      },
      error: err => {
        const ms = Date.now() - start;
        if (!environment.production) {
          console.error(`[HTTP] ✖ ${req.method} ${req.url} — status=${err?.status ?? 'network'} (${ms}ms)`, err?.error ?? err);
        }
        clientLog.error(`HTTP ${req.method} ${req.url} failed`, {
          status: String(err?.status ?? 'network'),
          statusText: String(err?.statusText ?? ''),
          ms: String(ms),
          // err?.error may contain PII or stack traces — log only its type/shape, never the raw value (T12).
          errorType: typeof err?.error === 'string' ? 'string' : typeof err?.error,
        });
      },
    })
  );
};
