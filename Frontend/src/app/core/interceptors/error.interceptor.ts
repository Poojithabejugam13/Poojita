import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const router = inject(Router);

    return next(req).pipe(
        catchError((err: unknown) => {
            if (err instanceof HttpErrorResponse) {
                const status = err.status;
                const body: any = err.error;

                const backendMessage =
                    (body && (body.error || body.message)) ||
                    err.message ||
                    'An unexpected error occurred. Please try again.';

                // Force redirect to error page only for real critical failures
                if (status === 0 || status >= 500) {
                    router.navigate(['/error'], { state: { errorMessage: backendMessage } });
                }
            }

            return throwError(() => err);
        })
    );
};
