import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ApiService } from '../../../core/services/api.service';
import { CaptchaComponent } from '../../../shared/components/captcha/captcha.component';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterLink, CaptchaComponent],
    templateUrl: './login.html'
})
export class Login {
    private authService = inject(AuthService);
    private apiService = inject(ApiService);
    private router = inject(Router);

    email = '';
    password = '';
    errorMessage = '';
    isCaptchaVerified = false;
    fieldErrors: { [key: string]: string } = {};

    validateEmail(email: string): boolean {
        const re = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
        return re.test(email);
    }

    onSubmit() {
        this.fieldErrors = {};
        this.errorMessage = '';

        if (!this.email) {
            this.fieldErrors['email'] = 'Email is required';
        } else if (!this.validateEmail(this.email)) {
            this.fieldErrors['email'] = 'Invalid email format';
        }

        if (!this.password) {
            this.fieldErrors['password'] = 'Password is required';
        }

        if (Object.keys(this.fieldErrors).length > 0) {
            return;
        }

        if (!this.isCaptchaVerified) {
            this.errorMessage = 'Please complete the security verification.';
            return;
        }

        this.apiService.login({ email: this.email, password: this.password }).subscribe({
            next: (res) => {
                if (res.token) {
                    this.authService.login(res.token);
                    const role = this.authService.currentUserRole();
                    switch (role) {
                        case 'Customer': this.router.navigate(['/customer/dashboard']); break;
                        case 'Admin': this.router.navigate(['/admin/dashboard']); break;
                        case 'Agent': this.router.navigate(['/agent/dashboard']); break;
                        case 'ClaimsOfficer': this.router.navigate(['/claims-officer/dashboard']); break;
                        default: this.router.navigate(['/customer/dashboard']); break;
                    }
                }
            },
            error: () => {
                this.errorMessage = 'Invalid email or password. Please try again.';
            }
        });
    }
}
