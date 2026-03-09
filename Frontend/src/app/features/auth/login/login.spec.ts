import { TestBed, ComponentFixture } from '@angular/core/testing';
import { Login } from './login';
import { AuthService } from '../../../core/services/auth.service';
import { ApiService } from '../../../core/services/api.service';
import { of, throwError } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CaptchaComponent } from '../../../shared/components/captcha/captcha.component';
import { ActivatedRoute, Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';

describe('Login Component', () => {
    let component: Login;
    let fixture: ComponentFixture<Login>;
    let mockAuthService: any;
    let mockApiService: any;
    let mockRouter: Router;

    beforeEach(async () => {
        mockAuthService = {
            login: jasmine.createSpy('login'),
            currentUserRole: jasmine.createSpy('currentUserRole').and.returnValue('Customer')
        };
        mockApiService = {
            login: jasmine.createSpy('login')
        };

        await TestBed.configureTestingModule({
            imports: [CommonModule, FormsModule, Login, CaptchaComponent, RouterTestingModule],
            providers: [
                { provide: AuthService, useValue: mockAuthService },
                { provide: ApiService, useValue: mockApiService },
                { provide: ActivatedRoute, useValue: { queryParams: of({}) } }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Login);
        component = fixture.componentInstance;
        mockRouter = TestBed.inject(Router);
        spyOn(mockRouter, 'navigate');
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should show error if captcha is not verified', () => {
        component.email = 'test@example.com';
        component.password = 'password';
        component.isCaptchaVerified = false;
        component.onSubmit();
        expect(component.errorMessage).toBe('Please complete the security verification.');
        expect(mockApiService.login).not.toHaveBeenCalled();
    });

    it('should call apiService.login when form is valid and captcha verified', () => {
        component.email = 'test@example.com';
        component.password = 'password';
        component.isCaptchaVerified = true;
        mockApiService.login.and.returnValue(of({ token: 'fake-jwt' }));

        component.onSubmit();

        expect(mockApiService.login).toHaveBeenCalledWith({ email: 'test@example.com', password: 'password' });
        expect(mockAuthService.login).toHaveBeenCalledWith('fake-jwt');
        expect(mockRouter.navigate).toHaveBeenCalled();
    });

    it('should set error message on failed login', () => {
        component.email = 'test@example.com';
        component.password = 'wrong';
        component.isCaptchaVerified = true;
        mockApiService.login.and.returnValue(throwError(() => new Error('Invalid')));

        component.onSubmit();

        expect(component.errorMessage).toBe('Invalid email or password. Please try again.');
    });
});
