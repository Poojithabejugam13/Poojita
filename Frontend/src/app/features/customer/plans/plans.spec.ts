import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Plans } from './plans';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { RouterTestingModule } from '@angular/router/testing';

describe('Plans', () => {
    let component: Plans;
    let fixture: ComponentFixture<Plans>;
    let apiServiceSpy: jasmine.SpyObj<ApiService>;
    let authServiceSpy: jasmine.SpyObj<AuthService>;
    let routerSpy: jasmine.SpyObj<Router>;

    const mockPlans = [
        { planId: 1, planName: 'Family Plan', planType: 'Family', tiers: [{ tierId: 10, tierName: 'Shield' }] }
    ];

    beforeEach(async () => {
        apiServiceSpy = jasmine.createSpyObj('ApiService', ['getPlans']);
        authServiceSpy = jasmine.createSpyObj('AuthService', ['isAuthenticated']);
        routerSpy = jasmine.createSpyObj('Router', ['navigate']);

        apiServiceSpy.getPlans.and.returnValue(of(mockPlans));

        await TestBed.configureTestingModule({
            imports: [Plans, CommonModule, RouterTestingModule],
            providers: [
                { provide: ApiService, useValue: apiServiceSpy },
                { provide: AuthService, useValue: authServiceSpy },
                { provide: Router, useValue: routerSpy },
                CurrencyPipe
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Plans);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load plans and categorize them', () => {
        expect(apiServiceSpy.getPlans).toHaveBeenCalled();
        expect(component.plans.length).toBe(1);
        expect(component.plans[0].planType).toBe('Family');
    });

    it('should filter plans by category', () => {
        component.setPlanType('Family');
        const filtered = component.getFilteredPlans();
        expect(filtered.length).toBe(1);

        component.setPlanType('Senior Citizen');
        expect(component.getFilteredPlans().length).toBe(0);
    });

    it('should navigate to purchase when buyNow is called', () => {
        component.buyNow(component.plans[0]);
        expect(routerSpy.navigate).toHaveBeenCalledWith(['/customer/purchase-policy'], {
            queryParams: { planId: 1, tierId: 10 }
        });
    });

    it('should select a plan and show tiers', () => {
        component.selectPlan(component.plans[0]);
        expect(component.currentStep).toBe('tiers');
        expect(component.selectedPlan).toEqual(component.plans[0]);
    });
});
