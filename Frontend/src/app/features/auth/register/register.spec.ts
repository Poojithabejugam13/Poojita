import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Register } from './register';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { RouterTestingModule } from '@angular/router/testing';
import { CaptchaComponent } from '../../../shared/components/captcha/captcha.component';

describe('Register', () => {
    let component: Register;
    let fixture: ComponentFixture<Register>;
    let apiServiceSpy: jasmine.SpyObj<ApiService>;
    let authServiceSpy: jasmine.SpyObj<AuthService>;
    let routerSpy: jasmine.SpyObj<Router>;

    beforeEach(async () => {
        apiServiceSpy = jasmine.createSpyObj('ApiService', ['login', 'registerCustomer']);
        authServiceSpy = jasmine.createSpyObj('AuthService', ['login', 'logout', 'currentUserRole']);
        routerSpy = jasmine.createSpyObj('Router', ['navigate']);

        await TestBed.configureTestingModule({
            imports: [Register, FormsModule, RouterTestingModule, CaptchaComponent],
            providers: [
                { provide: ApiService, useValue: apiServiceSpy },
                { provide: AuthService, useValue: authServiceSpy },
                { provide: Router, useValue: routerSpy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Register);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should default to register mode', () => {
        expect(component.isLoginMode).toBeFalse();
    });

    it('should switch to login mode when isLoginMode is set to true', () => {
        component.isLoginMode = true;
        expect(component.isLoginMode).toBeTrue();
    });

    it('should handle registration submission and navigate to login', () => {
        component.isLoginMode = false;
        component.isCaptchaVerified = true;
        component.isTermsAccepted = true;
        component.name = 'John Doe';
        component.email = 'john@example.com';
        component.password = 'password123';
        component.dateOfBirth = '1990-01-01';

        apiServiceSpy.registerCustomer.and.returnValue(of('success'));
        spyOn(window, 'alert');

        component.onSubmit();

        expect(apiServiceSpy.registerCustomer).toHaveBeenCalled();
        expect(window.alert).toHaveBeenCalled();
        expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
    });

    it('should handle login submission and navigate to customer dashboard', () => {
        component.isLoginMode = true;
        component.isCaptchaVerified = true;
        component.email = 'john@example.com';
        component.password = 'password123';

        apiServiceSpy.login.and.returnValue(of({ token: 'fake-token' }));
        authServiceSpy.currentUserRole.and.returnValue('Customer');

        component.onSubmit();

        expect(apiServiceSpy.login).toHaveBeenCalled();
        expect(authServiceSpy.login).toHaveBeenCalledWith('fake-token');
        expect(routerSpy.navigate).toHaveBeenCalledWith(['/customer/dashboard']);
    });

    it('should show error when registering without terms accepted', () => {
        component.isLoginMode = false;
        component.isCaptchaVerified = true;
        component.isTermsAccepted = false;
        component.name = 'John Doe';
        component.email = 'john@example.com';
        component.password = 'password123';
        component.dateOfBirth = '1990-01-01';

        component.onSubmit();

        expect(component.fieldErrors['terms']).toBeTruthy();
        expect(apiServiceSpy.registerCustomer).not.toHaveBeenCalled();
    });

    it('should set errorMessage on login failure', () => {
        component.isLoginMode = true;
        component.isCaptchaVerified = true;
        component.email = 'bad@example.com';
        component.password = 'wrongpass';

        apiServiceSpy.login.and.returnValue(throwError(() => new Error('Unauthorized')));

        component.onSubmit();

        expect(component.errorMessage).toBe('Invalid email or password. Please try again.');
    });
});
