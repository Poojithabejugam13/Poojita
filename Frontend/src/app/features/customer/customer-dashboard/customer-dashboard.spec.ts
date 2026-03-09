import { TestBed, ComponentFixture } from '@angular/core/testing';
import { CustomerDashboard } from './customer-dashboard';
import { ApiService } from '../../../core/services/api.service';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

describe('CustomerDashboard Component', () => {
    let component: CustomerDashboard;
    let fixture: ComponentFixture<CustomerDashboard>;
    let mockApiService: any;
    let mockRouter: any;

    const mockPlans = [
        { planId: 1, planName: 'Plan A', tierId: 101, tierName: 'Shield' },
        { planId: 1, planName: 'Plan A', tierId: 102, tierName: 'Elevate' }
    ];

    const mockDashboardInfo = {
        activePolicies: 2,
        assignedAgentName: 'John Agent',
        assignedAgentPhone: '1234567890',
        assignedOfficerName: 'Jane Officer',
        assignedOfficerPhone: '0987654321'
    };

    const mockPolicies = [
        { policyId: 1, planId: 1, tierId: 101, status: 3, totalPremium: 12000, remainingCoverageAmount: 50000 }, // Active
        { policyId: 2, planId: 1, tierId: 102, status: 1, totalPremium: 24000, remainingCoverageAmount: 100000 }  // Approved
    ];

    const mockClaims = [
        { id: 1, status: 0, claimAmount: 5000 }, // Pending
        { id: 2, status: 1, claimAmount: 10000 } // Approved
    ];

    beforeEach(async () => {
        mockApiService = {
            getPlans: jasmine.createSpy('getPlans').and.returnValue(of(mockPlans)),
            getCustomerDashboard: jasmine.createSpy('getCustomerDashboard').and.returnValue(of(mockDashboardInfo)),
            getMyPolicies: jasmine.createSpy('getMyPolicies').and.returnValue(of(mockPolicies)),
            getMyClaims: jasmine.createSpy('getMyClaims').and.returnValue(of(mockClaims)),
            makePayment: jasmine.createSpy('makePayment').and.returnValue(of({ invoiceUrl: '/inv1' })),
            renewPolicy: jasmine.createSpy('renewPolicy').and.returnValue(of('Success')),
            uploadDocument: jasmine.createSpy('uploadDocument').and.returnValue(of({ url: '/doc' })),
            raiseClaim: jasmine.createSpy('raiseClaim').and.returnValue(of('Success'))
        };
        mockRouter = {
            navigate: jasmine.createSpy('navigate')
        };

        await TestBed.configureTestingModule({
            imports: [CommonModule, FormsModule, CustomerDashboard],
            providers: [
                { provide: ApiService, useValue: mockApiService },
                { provide: Router, useValue: mockRouter },
                CurrencyPipe,
                DatePipe
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(CustomerDashboard);
        component = fixture.componentInstance;

        // Mock localStorage for fetchUserName
        const token = 'header.' + btoa(JSON.stringify({ unique_name: 'TestUser' })) + '.signature';
        spyOn(localStorage, 'getItem').and.returnValue(token);

        fixture.detectChanges();
    });

    it('should create and load dashboard data', () => {
        expect(component).toBeTruthy();
        expect(mockApiService.getPlans).toHaveBeenCalled();
        expect(mockApiService.getCustomerDashboard).toHaveBeenCalled();
        expect(component.userName).toBe('TestUser');
        expect(component.activePoliciesCount).toBe(2);
    });

    it('should correctly format policies for display', () => {
        expect(component.policies.length).toBe(2);
        expect(component.policies[0].status).toBe('Active');
        expect(component.policies[1].status).toBe('Approved');
    });

    it('should set selected tiers when a plan is selected', () => {
        component.selectedPlanId = 1;
        component.onPlanSelected();
        expect(component.selectedPlanTiers.length).toBe(2);
    });

    it('should navigate to wizard with correct params', () => {
        component.selectedPlanId = 1;
        component.selectedTierId = 101;
        component.navigateToWizard();
        expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer/purchase-policy'], {
            queryParams: { planId: 1, tierId: 101 }
        });
    });

    it('should calculate premium due correctly', () => {
        // Active (12000/12 = 1000) + Approved (24000/12 = 2000) = 3000
        expect(component.totalPremiumDue).toBe(3000);
    });

    it('should process payment and show invoice', () => {
        const policy = component.policies[0];
        component.payPremium(policy);
        expect(mockApiService.makePayment).toHaveBeenCalled();
        expect(component.showInvoice).toBeTrue();
        expect(component.paidPolicyIds.has(policy.id)).toBeTrue();
    });

    it('should open and close claim modal', () => {
        component.openClaimModal(1);
        expect(component.showClaimModal).toBeTrue();
        expect(component.claimData.policyId).toBe(1);

        component.closeClaimModal();
        expect(component.showClaimModal).toBeFalse();
    });
});
