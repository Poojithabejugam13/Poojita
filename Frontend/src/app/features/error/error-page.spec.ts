import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ErrorPage } from './error-page';
import { AuthService } from '../../core/services/auth.service';
import { RouterTestingModule } from '@angular/router/testing';
import { signal } from '@angular/core';

describe('ErrorPage', () => {
    let component: ErrorPage;
    let fixture: ComponentFixture<ErrorPage>;
    let authServiceSpy: jasmine.SpyObj<AuthService>;

    beforeEach(async () => {
        authServiceSpy = jasmine.createSpyObj('AuthService', ['isAuthenticated', 'currentUserRole']);

        await TestBed.configureTestingModule({
            imports: [ErrorPage, RouterTestingModule],
            providers: [
                { provide: AuthService, useValue: authServiceSpy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(ErrorPage);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should determine correct back route for Admin', () => {
        authServiceSpy.isAuthenticated.and.returnValue(true);
        authServiceSpy.currentUserRole.and.returnValue('Admin');
        expect(component.getBackRoute()).toBe('/admin/dashboard');
    });

    it('should determine correct back route for Customer', () => {
        authServiceSpy.isAuthenticated.and.returnValue(true);
        authServiceSpy.currentUserRole.and.returnValue('Customer');
        expect(component.getBackRoute()).toBe('/customer/dashboard');
    });

    it('should return to home if not authenticated', () => {
        authServiceSpy.isAuthenticated.and.returnValue(false);
        expect(component.getBackRoute()).toBe('/');
    });

    it('should show correct label', () => {
        authServiceSpy.isAuthenticated.and.returnValue(true);
        expect(component.getBackLabel()).toBe('Return to Dashboard');

        authServiceSpy.isAuthenticated.and.returnValue(false);
        expect(component.getBackLabel()).toBe('Return to Home');
    });
});
