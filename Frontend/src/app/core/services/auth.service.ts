import { Injectable, signal, inject } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    // Using Angular signals for reactive state
    currentUserRole = signal<string | null>(null);
    currentUserName = signal<string | null>(null);
    isAuthenticated = signal<boolean>(false);

    private router = inject(Router);

    constructor() {
        // Check local storage for existing token on init
        this.checkStoredToken();
    }

    checkStoredToken() {
        const token = localStorage.getItem('token');
        if (token) {
            // Decode JWT to get role (Mocking for now before backend integration)
            this.isAuthenticated.set(true);

            // Basic mock decoding: In reality, we use jwt-decode
            try {
                const payload = JSON.parse(atob(token.split('.')[1]));
                // Support multiple common JWT claim formats
                const roleStr =
                    payload['role'] ||
                    payload['Role'] ||
                    payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
                    'Customer';

                const nameStr =
                    payload['name'] ||
                    payload['unique_name'] ||
                    payload['FullName'] ||
                    payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
                    payload['sub'] ||
                    null;

                this.currentUserRole.set(roleStr);
                this.currentUserName.set(nameStr);
            } catch (e) {
                console.error('Initial token decode failed:', e);
                this.currentUserRole.set('Customer');
            }
        } else {
            this.isAuthenticated.set(false);
            this.currentUserRole.set(null);
            this.currentUserName.set(null);
        }
    }

    // Real login method that consumes a JWT from the backend API
    login(token: string) {
        localStorage.setItem('token', token);
        this.checkStoredToken();
    }

    logout() {
        localStorage.removeItem('token');
        this.isAuthenticated.set(false);
        this.currentUserRole.set(null);
        this.currentUserName.set(null);
        this.router.navigate(['/']);
    }
}
