import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    // Get the auth token from local storage.
    const authToken = localStorage.getItem('token');

    if (authToken) {
        // Clone the request and add the authorization header.
        const authReq = req.clone({
            setHeaders: {
                Authorization: `Bearer ${authToken}`
            }
        });

        // Pass on the cloned request instead of the original request.
        return next(authReq);
    }

    // Pass on the original sequence if there's no token
    return next(req);
};
