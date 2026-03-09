import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';
import { CaptchaComponent } from '../../../shared/components/captcha/captcha.component';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, CaptchaComponent],
  template: `
    <div class="min-h-[80vh] flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8 relative bg-gray-50 z-0">
      <div class="absolute inset-0 z-[-1] bg-bg-blush/40 clip-path-diagonal"></div>
      
      <div class="max-w-md w-full card shadow-2xl bg-white border-0 z-10 relative">
        <div class="text-center mb-8">
          <div class="mx-auto w-16 h-16 bg-bg-blush rounded-full flex items-center justify-center shadow-inner mb-4">
            <svg class="w-8 h-8 text-primary-plum" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z"></path>
            </svg>
          </div>
          <h2 class="text-3xl font-extrabold text-primary-plum">Reset Password</h2>
          <p class="text-gray-500 mt-2 font-medium text-sm">Enter your email and new password.</p>
        </div>

        <div *ngIf="errorMessage" class="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm font-medium text-center">
          {{ errorMessage }}
        </div>
        <div *ngIf="successMessage" class="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg text-green-700 text-sm font-medium text-center">
          {{ successMessage }}
        </div>

        <form *ngIf="!successMessage" class="space-y-6" (ngSubmit)="onSubmit()">
          <div>
            <label class="label-text">Email address</label>
            <input name="email" type="email" required [(ngModel)]="email" class="input-field" placeholder="you@example.com">
          </div>
          <div>
            <label class="label-text">Registered Phone Number</label>
            <input name="phoneNumber" type="tel" required [(ngModel)]="phoneNumber" class="input-field" placeholder="+91 XXXX-XXXXXX">
          </div>
          <div>
            <label class="label-text">New Password</label>
            <input name="password" type="password" required [(ngModel)]="newPassword" class="input-field" placeholder="••••••••">
          </div>

          <app-captcha (onVerified)="isCaptchaVerified = $event"></app-captcha>

          <div>
            <button type="submit" class="w-full btn-primary text-lg shadow-lg">Reset Password</button>
          </div>
        </form>

        <div class="mt-6 text-center">
            <a routerLink="/login" class="text-sm font-bold text-primary-plum hover:underline">Back to Login</a>
        </div>
      </div>
    </div>
    <style>
      .clip-path-diagonal { clip-path: polygon(0 0, 100% 0, 100% 60%, 0 100%); }
    </style>
  `
})
export class ForgotPassword {
  private apiService = inject(ApiService);
  private router = inject(Router);

  email = '';
  phoneNumber = '';
  newPassword = '';
  isCaptchaVerified = false;
  errorMessage = '';
  successMessage = '';

  onSubmit() {
    if (!this.isCaptchaVerified) {
      this.errorMessage = 'Please complete the security verification.';
      return;
    }
    if (!this.email || !this.phoneNumber || !this.newPassword) {
      this.errorMessage = 'Please provide email, registered phone number, and new password.';
      return;
    }

    this.apiService.resetPassword({ email: this.email, phoneNumber: this.phoneNumber, newPassword: this.newPassword }).subscribe({
      next: () => {
        this.successMessage = 'Password reset successfully! You can now login.';
        setTimeout(() => this.router.navigate(['/login']), 3000);
      },
      error: (err: any) => {
        this.errorMessage = 'Failed to reset password. Please check your email and try again.';
        console.error(err);
      }
    });
  }
}
