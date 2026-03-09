import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Home } from './home';
import { AuthService } from '../../core/services/auth.service';
import { RouterTestingModule } from '@angular/router/testing';

describe('Home', () => {
    let component: Home;
    let fixture: ComponentFixture<Home>;
    let authServiceSpy: jasmine.SpyObj<AuthService>;

    beforeEach(async () => {
        authServiceSpy = jasmine.createSpyObj('AuthService', ['isAuthenticated']);

        await TestBed.configureTestingModule({
            imports: [Home, RouterTestingModule],
            providers: [
                { provide: AuthService, useValue: authServiceSpy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Home);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
