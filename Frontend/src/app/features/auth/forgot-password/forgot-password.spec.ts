import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ForgotPassword } from './forgot-password';
import { ApiService } from '../../../core/services/api.service';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { RouterTestingModule } from '@angular/router/testing';
import { CaptchaComponent } from '../../../shared/components/captcha/captcha.component';

describe('ForgotPassword', () => {
    let component: ForgotPassword;
    let fixture: ComponentFixture<ForgotPassword>;
    let mockApiService: any;
    let mockRouter: any;

    beforeEach(async () => {
        mockApiService = {
            resetPassword: jasmine.createSpy('resetPassword').and.returnValue(of({}))
        };
        mockRouter = {
            navigate: jasmine.createSpy('navigate')
        };

        await TestBed.configureTestingModule({
            imports: [ForgotPassword, FormsModule, RouterTestingModule, CaptchaComponent],
            providers: [
                { provide: ApiService, useValue: mockApiService },
                { provide: ActivatedRoute, useValue: { queryParams: of({}) } }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(ForgotPassword);
        component = fixture.componentInstance;
        mockRouter = TestBed.inject(Router);
        spyOn(mockRouter, 'navigate');
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should show error if captcha is not verified', () => {
        component.isCaptchaVerified = false;
        component.onSubmit();
        expect(component.errorMessage).toBe('Please complete the security verification.');
    });

    it('should show error if fields are empty', () => {
        component.isCaptchaVerified = true;
        component.email = '';
        component.phoneNumber = '';
        component.newPassword = '';
        component.onSubmit();
        expect(component.errorMessage).toBe('Please provide email, registered phone number, and new password.');
    });

    it('should call resetPassword and navigate on success', fakeAsync(() => {
        component.isCaptchaVerified = true;
        component.email = 'test@example.com';
        component.phoneNumber = '1234567890';
        component.newPassword = 'password123';
        mockApiService.resetPassword.and.returnValue(of({}));

        component.onSubmit();

        expect(mockApiService.resetPassword).toHaveBeenCalledWith({
            email: 'test@example.com',
            phoneNumber: '1234567890',
            newPassword: 'password123'
        });
        expect(component.successMessage).toContain('Password reset successfully');

        tick(3000);
        expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
    }));

    it('should handle error from api', () => {
        component.isCaptchaVerified = true;
        component.email = 'test@example.com';
        component.phoneNumber = '1234567890';
        component.newPassword = 'password123';
        mockApiService.resetPassword.and.returnValue(throwError(() => ({ error: 'Error' })));

        component.onSubmit();

        expect(component.errorMessage).toContain('Failed to reset password');
    });
});
