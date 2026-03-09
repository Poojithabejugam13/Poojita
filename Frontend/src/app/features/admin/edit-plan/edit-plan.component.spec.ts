import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { EditPlanComponent } from './edit-plan.component';
import { ApiService } from '../../../core/services/api.service';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { FormsModule } from '@angular/forms';

describe('EditPlanComponent', () => {
    let component: EditPlanComponent;
    let fixture: ComponentFixture<EditPlanComponent>;
    let apiServiceSpy: jasmine.SpyObj<ApiService>;
    let routerSpy: jasmine.SpyObj<Router>;

    const mockPlans = [
        {
            planId: 1,
            planName: 'Family Health',
            planType: 'Family',
            imageUrl: 'img.png',
            tiers: [
                {
                    tierId: 10,
                    tierName: 'Shield',
                    basePremium: 2000,
                    coverageLimit: 500000,
                    ageLockProtection: 60,
                    coverageRestoreEnabled: true,
                    maxRestoresPerYear: 1,
                    boosterMultiplier: 1.5,
                    preExistingDiseaseWaitingMonths: 24,
                    coPaymentPercentage: 10,
                    commissionPercentage: 5
                }
            ]
        }
    ];

    beforeEach(async () => {
        apiServiceSpy = jasmine.createSpyObj('ApiService', ['getPlans', 'updatePlan']);
        routerSpy = jasmine.createSpyObj('Router', ['navigate']);

        apiServiceSpy.getPlans.and.returnValue(of(mockPlans));

        await TestBed.configureTestingModule({
            imports: [EditPlanComponent, FormsModule],
            providers: [
                { provide: ApiService, useValue: apiServiceSpy },
                { provide: Router, useValue: routerSpy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(EditPlanComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load plans on init', () => {
        expect(apiServiceSpy.getPlans).toHaveBeenCalled();
        expect(component.plans.length).toBe(1);
        expect(component.plans[0].planType).toBe('Family');
    });

    it('should select a plan to edit and populate form', () => {
        component.selectPlanToEdit(component.plans[0]);
        expect(component.currentView).toBe('edit');
        expect(component.planForm.planName).toBe('Family Health');
        expect(component.planForm.tierName).toBe('Shield');
    });

    it('should handle tier selection change', () => {
        component.selectedPlan = component.plans[0];
        component.onTierSelected(10);
        expect(component.planForm.tierName).toBe('Shield');
    });

    it('should call updatePlan on submitEdit', () => {
        component.selectPlanToEdit(component.plans[0]);
        apiServiceSpy.updatePlan.and.returnValue(of({}));

        component.submitEdit();

        expect(apiServiceSpy.updatePlan).toHaveBeenCalled();
    });

    it('should cancel edit and return to selection view', () => {
        component.currentView = 'edit';
        component.cancelEdit();
        expect(component.currentView).toBe('select');
        expect(component.selectedPlan).toBeNull();
    });
});
