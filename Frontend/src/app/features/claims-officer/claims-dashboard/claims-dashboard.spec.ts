import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ClaimsDashboard } from './claims-dashboard';
import { ApiService } from '../../../core/services/api.service';
import { of, throwError } from 'rxjs';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';

describe('ClaimsDashboard', () => {
    let component: ClaimsDashboard;
    let fixture: ComponentFixture<ClaimsDashboard>;
    let apiServiceSpy: jasmine.SpyObj<ApiService>;

    beforeEach(async () => {
        apiServiceSpy = jasmine.createSpyObj('ApiService', [
            'getClaimsDashboard',
            'getPendingClaims',
            'getClaimsOfficerCustomers',
            'approveClaim',
            'rejectClaim'
        ]);

        apiServiceSpy.getClaimsDashboard.and.returnValue(of({ pendingClaims: 5, approvedClaims: 2 }));
        apiServiceSpy.getPendingClaims.and.returnValue(of([]));
        apiServiceSpy.getClaimsOfficerCustomers.and.returnValue(of([]));

        await TestBed.configureTestingModule({
            imports: [ClaimsDashboard, CommonModule],
            providers: [
                { provide: ApiService, useValue: apiServiceSpy },
                CurrencyPipe,
                DatePipe
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(ClaimsDashboard);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load initial data', () => {
        expect(apiServiceSpy.getClaimsDashboard).toHaveBeenCalled();
        expect(apiServiceSpy.getPendingClaims).toHaveBeenCalled();
        expect(apiServiceSpy.getClaimsOfficerCustomers).toHaveBeenCalled();
        expect(component.pendingClaims).toBe(0); // Updated by loadPendingClaims as it's empty
    });

    it('should approve a claim', () => {
        const mockClaim = { claimId: 1, status: 0 } as any;
        apiServiceSpy.approveClaim.and.returnValue(of({}));
        spyOn(window, 'alert');

        component.approveClaim(mockClaim);

        expect(apiServiceSpy.approveClaim).toHaveBeenCalledWith(1, undefined);
        expect(mockClaim.status).toBe(1);
        expect(window.alert).toHaveBeenCalledWith('Claim #1 has been approved.');
    });

    it('should reject a claim with reason', () => {
        const mockClaim = { claimId: 2, status: 0 } as any;
        spyOn(window, 'prompt').and.returnValue('Fraudulent claim');
        apiServiceSpy.rejectClaim.and.returnValue(of({}));
        spyOn(window, 'alert');

        component.rejectClaim(mockClaim);

        expect(apiServiceSpy.rejectClaim).toHaveBeenCalledWith(2, 'Fraudulent claim');
        expect(mockClaim.status).toBe(2);
        expect(window.alert).toHaveBeenCalledWith('Claim #2 has been rejected. Reason logged: Fraudulent claim');
    });

    it('should cancel rejection if no reason provided', () => {
        const mockClaim = { claimId: 2, status: 0 } as any;
        spyOn(window, 'prompt').and.returnValue(null);
        spyOn(window, 'alert');

        component.rejectClaim(mockClaim);

        expect(apiServiceSpy.rejectClaim).not.toHaveBeenCalled();
        expect(window.alert).toHaveBeenCalledWith('Rejection cancelled. A reason is required to reject a claim.');
    });

    it('should switch tabs', () => {
        component.setTab('customers');
        expect(component.activeTab).toBe('customers');
    });
});
