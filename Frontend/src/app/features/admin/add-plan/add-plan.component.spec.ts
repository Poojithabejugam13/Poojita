import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { AddPlanComponent } from './add-plan.component';
import { ApiService } from '../../../core/services/api.service';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { FormsModule } from '@angular/forms';

describe('AddPlanComponent', () => {
    let component: AddPlanComponent;
    let fixture: ComponentFixture<AddPlanComponent>;
    let apiServiceSpy: jasmine.SpyObj<ApiService>;
    let routerSpy: jasmine.SpyObj<Router>;

    beforeEach(async () => {
        apiServiceSpy = jasmine.createSpyObj('ApiService', ['createPlan', 'createTier']);
        routerSpy = jasmine.createSpyObj('Router', ['navigate']);

        await TestBed.configureTestingModule({
            imports: [AddPlanComponent, FormsModule],
            providers: [
                { provide: ApiService, useValue: apiServiceSpy },
                { provide: Router, useValue: routerSpy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AddPlanComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should move to next and previous steps', () => {
        expect(component.currentStep).toBe(1);
        // Step 1 requires planName + category to advance
        component.planForm.planName = 'Test Plan';
        component.planForm.planType = 'Individual';
        component.nextStep();
        expect(component.currentStep).toBe(2);
        component.prevStep();
        expect(component.currentStep).toBe(1);
    });

    it('should not submit when no tier is selected', () => {
        component.planForm.planName = 'Test Plan';
        component.planForm.tiers.Shield.enabled = false;
        component.planForm.tiers.Elevate.enabled = false;
        component.planForm.tiers.Apex.enabled = false;
        component.submitNewPlan();
        // validateStep3 returns true but firstTierData is undefined => no API call
        expect(apiServiceSpy.createPlan).not.toHaveBeenCalled();
    });

    it('should call createPlan on submitNewPlan with valid data', () => {
        component.planForm.planName = 'Test Plan';
        component.planForm.tiers.Shield.enabled = true;
        apiServiceSpy.createPlan.and.returnValue(of(123));

        component.submitNewPlan();

        expect(apiServiceSpy.createPlan).toHaveBeenCalled();
        expect(routerSpy.navigate).toHaveBeenCalledWith(['/admin/dashboard']);
    });

    it('should handle multiple tiers on submission', fakeAsync(() => {
        component.planForm.planName = 'Test Plan';
        component.planForm.tiers.Shield.enabled = true;
        component.planForm.tiers.Elevate.enabled = true;

        apiServiceSpy.createPlan.and.returnValue(of(123));
        apiServiceSpy.createTier.and.returnValue(of({}));

        component.submitNewPlan();
        tick();

        expect(apiServiceSpy.createPlan).toHaveBeenCalled();
        expect(apiServiceSpy.createTier).toHaveBeenCalledTimes(1); // One additional tier
        expect(routerSpy.navigate).toHaveBeenCalledWith(['/admin/dashboard']);
    }));

    it('should handle error during plan creation', () => {
        component.planForm.planName = 'Test Plan';
        component.planForm.tiers.Shield.enabled = true;
        apiServiceSpy.createPlan.and.returnValue(throwError(() => ({ error: 'Creation failed' })));

        component.submitNewPlan();

        expect(component.errorMessage).toContain('Failed to create plan');
        expect(component.loading).toBeFalse();
    });
});
