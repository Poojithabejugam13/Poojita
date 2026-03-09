import { TestBed, ComponentFixture, fakeAsync, tick } from '@angular/core/testing';
import { PolicyWizard, InsurancePlan, PlanTier } from './policy-wizard';
import { Router, ActivatedRoute } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';
import { of } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { CommonModule, CurrencyPipe } from '@angular/common';

describe('PolicyWizard Component', () => {
    let component: PolicyWizard;
    let fixture: ComponentFixture<PolicyWizard>;
    let mockApiService: any;
    let mockRouter: any;
    let mockActivatedRoute: any;

    const mockPlans: InsurancePlan[] = [
        {
            planId: 1,
            planName: 'Basic Individual Plan',
            planType: 'Individual',
            imageUrl: '',
            tiers: [
                { tierId: 101, planId: 1, tierName: 'Shield', basePremium: 1000, baseCoverageAmount: 100000, coverageLimit: 100000, ageLockProtection: false, coverageRestoreEnabled: false, maxRestoresPerYear: 0, boosterMultiplier: 1, preExistingDiseaseWaitingMonths: 24, coPaymentPercentage: 10 }
            ]
        },
        {
            planId: 2,
            planName: 'Family Floater Plus',
            planType: 'Family',
            imageUrl: '',
            tiers: [
                { tierId: 201, planId: 2, tierName: 'Elevate', basePremium: 2000, baseCoverageAmount: 500000, coverageLimit: 500000, ageLockProtection: true, coverageRestoreEnabled: true, maxRestoresPerYear: 1, boosterMultiplier: 2, preExistingDiseaseWaitingMonths: 12, coPaymentPercentage: 0 }
            ]
        }
    ];

    beforeEach(async () => {
        mockApiService = {
            getPlans: jasmine.createSpy('getPlans').and.returnValue(of(mockPlans)),
            requestPolicy: jasmine.createSpy('requestPolicy').and.returnValue(of('success')),
            uploadDocument: jasmine.createSpy('uploadDocument').and.returnValue(of({ url: '/uploads/doc.pdf' }))
        };
        mockRouter = {
            navigate: jasmine.createSpy('navigate')
        };
        mockActivatedRoute = {
            queryParams: of({})
        };

        await TestBed.configureTestingModule({
            imports: [CommonModule, FormsModule, PolicyWizard],
            providers: [
                { provide: ApiService, useValue: mockApiService },
                { provide: Router, useValue: mockRouter },
                { provide: ActivatedRoute, useValue: mockActivatedRoute },
                CurrencyPipe
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(PolicyWizard);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create and load plans', () => {
        expect(component).toBeTruthy();
        expect(mockApiService.getPlans).toHaveBeenCalled();
        expect(component.allPlans.length).toBe(2);
    });

    it('should validate step 1 - selection required', () => {
        component.selectedPlan = null;
        expect(component.validateStep1()).toBeFalse();
        expect(component.validationError).toContain('Please select a plan');
    });

    it('should navigate from step 1 to 2 when selection is made', () => {
        component.selectPlanTier(mockPlans[0], mockPlans[0].tiers![0]);
        component.nextStep();
        expect(component.currentStep).toBe(2);
    });

    it('should enforce 2 members for Family category in step 4', () => {
        component.selectedPlanType = 'Family';
        component.members = [{ name: 'Self', dob: '1990-01-01', gender: 'Male', relation: 'Self' }];
        expect(component.validateStep4()).toBeFalse();
        expect(component.validationError).toContain('minimum of 2 members');

        component.addMember();
        component.members[1] = { name: 'Member 2', dob: '1995-05-05', gender: 'Female', relation: 'Spouse' };
        expect(component.validateStep4()).toBeTrue();
    });

    it('should calculate premium correctly with loadings and 18% GST', () => {
        component.selectedTier = mockPlans[0].tiers![0]; // base 1000
        component.proposer.dob = '1970-01-01'; // Age 54 (as of 2024), > 40
        // Loading: base * 0.1 = 100. subtotal = 1100.

        component.health.heightCm = 170;
        component.health.weightKg = 90; // BMI ~ 31.1 (Obese, 20% factor)
        // BMI Loading: 1100 * 0.2 = 220.

        component.health.isSmoker = true; // 25% factor
        // Smoking Loading: 1100 * 0.25 = 275.

        // PreTax: 1100 + 220 + 275 = 1595.
        // Tax (18%): 1595 * 0.18 = 287.1
        // Final: 1595 + 287.1 = 1882.1

        component.calculatePremium();

        expect(component.ageLoading).toBe(100);
        expect(component.bmiLoading).toBe(220);
        expect(component.smokingLoading).toBe(275);
        expect(component.preTaxTotal).toBe(1595);
        expect(component.taxAmount).toBeCloseTo(287.1, 1);
        expect(component.finalPremium).toBeCloseTo(1882.1, 1);
    });

    it('should handle file selection', () => {
        const file = new File([''], 'aadhaar.pdf', { type: 'application/pdf' });
        const event = { target: { files: [file] } };
        component.onFileSelected(event);
        expect(component.selectedFile).toBe(file);
        expect(component.documentPreview).toBe('aadhaar.pdf');
    });

    it('should submit policy and navigate on success', fakeAsync(() => {
        component.selectedPlan = mockPlans[0];
        component.selectedTier = mockPlans[0].tiers![0];
        component.selectedFile = new File([''], 'test.pdf');
        mockApiService.uploadDocument.and.returnValue(of({ fileName: 'test.pdf' }));
        mockApiService.requestPolicy.and.returnValue(of({}));

        component.submitPolicy();
        tick(3000);

        expect(mockApiService.uploadDocument).toHaveBeenCalled();
        expect(mockApiService.requestPolicy).toHaveBeenCalled();
        expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer/dashboard']);
    }));
});
