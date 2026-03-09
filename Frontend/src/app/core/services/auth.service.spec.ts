import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';
import { Router } from '@angular/router';

describe('AuthService', () => {
    let service: AuthService;
    let routerSpy: jasmine.SpyObj<Router>;

    beforeEach(() => {
        routerSpy = jasmine.createSpyObj('Router', ['navigate']);

        TestBed.configureTestingModule({
            providers: [
                AuthService,
                { provide: Router, useValue: routerSpy }
            ]
        });

        service = TestBed.inject(AuthService);
        localStorage.clear();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should set authenticated to false if no token in storage', () => {
        service.checkStoredToken();
        expect(service.isAuthenticated()).toBeFalse();
    });

    it('should decode token and set signals', () => {
        // Mock JWT payload: { "role": "Admin", "FullName": "Admin User" }
        // header.payload.signature
        const payload = btoa(JSON.stringify({ role: 'Admin', FullName: 'Admin User' }));
        const mockToken = `header.${payload}.signature`;

        localStorage.setItem('token', mockToken);
        service.checkStoredToken();

        expect(service.isAuthenticated()).toBeTrue();
        expect(service.currentUserRole()).toBe('Admin');
        expect(service.currentUserName()).toBe('Admin User');
    });

    it('should clear signals and navigate on logout', () => {
        localStorage.setItem('token', 'some-token');
        service.logout();

        expect(localStorage.getItem('token')).toBeNull();
        expect(service.isAuthenticated()).toBeFalse();
        expect(service.currentUserRole()).toBeNull();
        expect(routerSpy.navigate).toHaveBeenCalledWith(['/']);
    });

    it('should perform login by storing token', () => {
        const payload = btoa(JSON.stringify({ role: 'Customer', FullName: 'John Doe' }));
        const mockToken = `header.${payload}.signature`;

        service.login(mockToken);

        expect(localStorage.getItem('token')).toBe(mockToken);
        expect(service.isAuthenticated()).toBeTrue();
        expect(service.currentUserRole()).toBe('Customer');
    });
});
