import { HttpInterceptorFn } from '@angular/common/http';
import { TimeoutError, timeout } from 'rxjs';

const REQUEST_TIMEOUT_MS = 15_000;

export const timeoutInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    timeout({
      each: REQUEST_TIMEOUT_MS,
      with: () => { throw new TimeoutError(); },
    })
  );
};
