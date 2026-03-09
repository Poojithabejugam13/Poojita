import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
    selector: 'app-error-page',
    standalone: true,
    imports: [CommonModule, RouterLink],
    templateUrl: './error-page.html'
})
export class ErrorPage {
    message: string;

    constructor(private auth: AuthService) {
        const nav = window.history.state as { errorMessage?: string };
        this.message = nav?.errorMessage || 'The page you are looking for might have been moved or doesn\'t exist.';
    }

    getBackRoute(): string {
        if (!this.auth.isAuthenticated()) return '/';

        const role = this.auth.currentUserRole();
        switch (role) {
            case 'Admin': return '/admin/dashboard';
            case 'Agent': return '/agent/dashboard';
            case 'ClaimsOfficer': return '/claims-officer/dashboard';
            case 'Customer': return '/customer/dashboard';
            default: return '/';
        }
    }

    getBackLabel(): string {
        return this.auth.isAuthenticated() ? 'Return to Dashboard' : 'Return to Home';
    }
}

