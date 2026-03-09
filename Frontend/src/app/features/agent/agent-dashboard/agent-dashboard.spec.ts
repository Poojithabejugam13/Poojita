import { TestBed, ComponentFixture } from '@angular/core/testing';
import { AgentDashboard } from './agent-dashboard';
import { ApiService } from '../../../core/services/api.service';
import { of } from 'rxjs';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';

describe('AgentDashboard Component', () => {
    let component: AgentDashboard;
    let fixture: ComponentFixture<AgentDashboard>;
    let mockApiService: any;

    const mockDashboardMetrics = {
        totalCommissionEarned: 5000,
        policiesSold: 10
    };

    const mockPendingRequests = [
        {
            policyId: 1,
            customerName: 'Alice',
            planName: 'Basic Plan',
            totalPremium: 1000,
            status: 0,
            policyCategory: 'Individual',
            fullName: 'Alice Smith',
            showDetails: false
        }
    ];

    const mockMyCustomers = [
        { policyId: 2, customerName: 'Bob', status: 3, totalPremium: 2000 } // Active
    ];

    beforeEach(async () => {
        mockApiService = {
            getAgentDashboard: jasmine.createSpy('getAgentDashboard').and.returnValue(of(mockDashboardMetrics)),
            getPendingPolicies: jasmine.createSpy('getPendingPolicies').and.returnValue(of(mockPendingRequests)),
            getAgentCustomers: jasmine.createSpy('getAgentCustomers').and.returnValue(of(mockMyCustomers)),
            approvePolicy: jasmine.createSpy('approvePolicy').and.returnValue(of({})),
            rejectPolicy: jasmine.createSpy('rejectPolicy').and.returnValue(of({})),
            updatePolicyStatus: jasmine.createSpy('updatePolicyStatus').and.returnValue(of({})),
            cancelPolicy: jasmine.createSpy('cancelPolicy').and.returnValue(of({})),
            expirePolicy: jasmine.createSpy('expirePolicy').and.returnValue(of({}))
        };

        await TestBed.configureTestingModule({
            imports: [CommonModule, AgentDashboard],
            providers: [
                { provide: ApiService, useValue: mockApiService },
                CurrencyPipe,
                DatePipe
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AgentDashboard);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create and load data', () => {
        expect(component).toBeTruthy();
        expect(mockApiService.getAgentDashboard).toHaveBeenCalled();
        expect(mockApiService.getPendingPolicies).toHaveBeenCalled();
        expect(component.totalCommission).toBe(5000);
        expect(component.pendingApprovals).toBe(1);
    });

    it('should toggle details', () => {
        const req = component.requests[0];
        expect(req.showDetails).toBeFalse();
        component.toggleDetails(req);
        expect(req.showDetails).toBeTrue();
    });

    it('should open approve modal on approvePolicy', () => {
        const req = component.requests[0];
        component.approvePolicy(req);
        expect(component.showApproveModal).toBeTrue();
        expect(component.pendingApproveReq).toBe(req);
    });

    it('should open reject modal', () => {
        const req = component.requests[0];
        component.rejectPolicy(req);
        expect(component.showRejectModal).toBeTrue();
        expect(component.pendingRejectReq).toBe(req);
    });

    it('should confirm rejection and call api', () => {
        const req = component.requests[0];
        component.pendingRejectReq = req;
        component.rejectReason = 'Documentation missing';
        component.confirmReject();
        expect(mockApiService.rejectPolicy).toHaveBeenCalledWith(1, 'Documentation missing');
        expect(component.showRejectModal).toBeFalse();
    });

    it('should cancel policy after confirmation', () => {
        spyOn(window, 'confirm').and.returnValue(true);
        const cust = component.myCustomers[0];
        component.cancelPolicy(cust);
        expect(mockApiService.cancelPolicy).toHaveBeenCalledWith(cust.policyId);
    });

    it('should expire policy after confirmation', () => {
        spyOn(window, 'confirm').and.returnValue(true);
        const cust = component.myCustomers[0];
        component.expirePolicy(cust);
        expect(mockApiService.expirePolicy).toHaveBeenCalledWith(cust.policyId);
    });

    it('should correctly format customer status for display', () => {
        expect(component.myCustomers[0].status).toBe('Active');
    });
});
