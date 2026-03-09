import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const customerOnlyGuard: CanActivateFn = () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    // Allow unauthenticated users (prospective customers) through
    if (!auth.isAuthenticated()) {
        return true;
    }

    // Authenticated customers may also access registration if needed,
    // but staff/internal roles should be redirected.
    const role = auth.currentUserRole();
    if (role === 'Customer') {
        // Redirect logged-in customers to their dashboard instead
        return router.createUrlTree(['/customer/dashboard']);
    }

    // Internal staff: show unauthorized error instead of silent redirect
    if (['Admin', 'Agent', 'ClaimsOfficer'].includes(role || '')) {
        router.navigate(['/error'], {
            state: { errorMessage: 'Access Denied: Logged-in staff members cannot access customer registration.' }
        });
        return false;
    }

    return true;
};
