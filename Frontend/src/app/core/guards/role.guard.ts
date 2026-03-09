import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
    return () => {
        const auth = inject(AuthService);
        const router = inject(Router);

        if (!auth.isAuthenticated()) {
            return router.createUrlTree(['/login']);
        }

        const userRole = auth.currentUserRole();
        if (userRole && allowedRoles.includes(userRole)) {
            return true;
        }

        // Not authorized
        router.navigate(['/error'], {
            state: {
                errorMessage: 'Access Denied: You do not have permission to view this page.'
            }
        });
        return false;
    };
};
