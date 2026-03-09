import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-add-plan',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './add-plan.component.html'
})
export class AddPlanComponent {
  private apiService = inject(ApiService);
  private router = inject(Router);

  currentStep = 1;
  totalSteps = 3;
  loading = false;
  errorMessage = '';
  fieldErrors: { [key: string]: string } = {};

  newPlanImage: File | null = null;

  planForm: any = {
    planName: '',
    description: '',
    planType: 'Individual',
    tiers: {
      Shield: { enabled: false, basePremium: 2000, coverageLimit: 500000, ageLockProtection: false, coverageRestoreEnabled: true, maxRestoresPerYear: 1, boosterMultiplier: 2, preExistingDiseaseWaitingMonths: 24, coPaymentPercentage: 10, commissionPercentage: 0 },
      Elevate: { enabled: false, basePremium: 3500, coverageLimit: 1000000, ageLockProtection: false, coverageRestoreEnabled: true, maxRestoresPerYear: 3, boosterMultiplier: 3, preExistingDiseaseWaitingMonths: 12, coPaymentPercentage: 5, commissionPercentage: 0 },
      Apex: { enabled: false, basePremium: 6000, coverageLimit: 2500000, ageLockProtection: true, coverageRestoreEnabled: true, maxRestoresPerYear: 99, boosterMultiplier: 5, preExistingDiseaseWaitingMonths: 0, coPaymentPercentage: 0, commissionPercentage: 0 }
    }
  };

  onImageSelected(event: any) {
    if (event.target.files && event.target.files.length > 0) {
      this.newPlanImage = event.target.files[0];
    }
  }

  validateStep1(): boolean {
    this.fieldErrors = {};
    if (!this.planForm.planName) this.fieldErrors['planName'] = 'Plan name is required';
    if (!this.planForm.planType) this.fieldErrors['planType'] = 'Plan Type is required';
    return Object.keys(this.fieldErrors).length === 0;
  }

  validateStep2(): boolean {
    this.fieldErrors = {};
    const selectedTiers = this.getSelectedTiers();
    if (selectedTiers.length === 0) {
      this.errorMessage = 'Please select at least one tier.';
      return false;
    }
    this.errorMessage = '';

    selectedTiers.forEach(tier => {
      const data = this.planForm.tiers[tier];
      if (data.basePremium <= 0) this.fieldErrors[`${tier}_basePremium`] = 'Must be > 0';
      if (data.coverageLimit <= 0) this.fieldErrors[`${tier}_coverageLimit`] = 'Must be > 0';
      if (data.commissionPercentage < 0 || data.commissionPercentage > 100)
        this.fieldErrors[`${tier}_commissionPercentage`] = 'Must be 0-100%';
    });

    return Object.keys(this.fieldErrors).length === 0;
  }

  validateStep3(): boolean {
    this.fieldErrors = {};
    const selectedTiers = this.getSelectedTiers();
    selectedTiers.forEach(tier => {
      const data = this.planForm.tiers[tier];
      if (data.coPaymentPercentage < 0 || data.coPaymentPercentage > 100)
        this.fieldErrors[`${tier}_coPaymentPercentage`] = '0-100%';
      if (data.preExistingDiseaseWaitingMonths < 0) this.fieldErrors[`${tier}_ped`] = 'Cannot be negative';
    });
    return Object.keys(this.fieldErrors).length === 0;
  }

  nextStep() {
    if (this.currentStep === 1 && !this.validateStep1()) return;
    if (this.currentStep === 2 && !this.validateStep2()) return;
    if (this.currentStep < this.totalSteps) this.currentStep++;
  }

  prevStep() { if (this.currentStep > 1) this.currentStep--; }

  cancel() { this.router.navigate(['/admin/dashboard']); }

  getSelectedTiers(): string[] {
    return Object.keys(this.planForm.tiers).filter(t => this.planForm.tiers[t].enabled);
  }

  submitNewPlan() {
    if (!this.validateStep3()) return;

    this.loading = true;
    this.errorMessage = '';

    const selectedTiers = this.getSelectedTiers();
    if (selectedTiers.length === 0) {
      this.errorMessage = 'Please select at least one tier before submitting.';
      this.loading = false;
      return;
    }
    const firstTierName = selectedTiers[0];
    const firstTierData = this.planForm.tiers[firstTierName];

    const formData = new FormData();
    formData.append('PlanName', this.planForm.planName);
    formData.append('Description', this.planForm.description);
    formData.append('PlanType', this.planForm.planType);

    formData.append('TierName', firstTierName);
    formData.append('BasePremium', firstTierData.basePremium.toString());
    formData.append('BaseCoverageAmount', firstTierData.coverageLimit.toString());
    formData.append('AgeLockProtection', firstTierData.ageLockProtection.toString());
    formData.append('CoverageRestoreEnabled', firstTierData.coverageRestoreEnabled.toString());
    formData.append('MaxRestoresPerYear', firstTierData.maxRestoresPerYear.toString());
    formData.append('BoosterMultiplier', firstTierData.boosterMultiplier.toString());
    formData.append('PreExistingDiseaseWaitingMonths', firstTierData.preExistingDiseaseWaitingMonths.toString());
    formData.append('CoPaymentPercentage', firstTierData.coPaymentPercentage.toString());
    formData.append('CommissionPercentage', firstTierData.commissionPercentage.toString());

    if (this.newPlanImage) formData.append('Image', this.newPlanImage);

    this.apiService.createPlan(formData).subscribe({
      next: (planId) => {
        const additionalTierNames = selectedTiers.slice(1);
        if (additionalTierNames.length > 0) {
          this.createAdditionalTiers(planId, additionalTierNames);
        } else {
          this.loading = false;
          this.router.navigate(['/admin/dashboard']);
        }
      },
      error: (err) => {
        const msg = typeof err.error === 'string' ? err.error : (err.error?.error || err.message);
        this.errorMessage = 'Failed to create plan: ' + (msg || 'Server error');
        this.loading = false;
      }
    });
  }

  private createAdditionalTiers(planId: number, tierNames: string[]) {
    let completed = 0;
    let hasError = false;

    for (const name of tierNames) {
      const data = this.planForm.tiers[name];
      const tierDto = {
        planId: planId,
        tierName: name,
        basePremium: data.basePremium,
        coverageLimit: data.coverageLimit,
        ageLockProtection: data.ageLockProtection,
        coverageRestoreEnabled: data.coverageRestoreEnabled,
        maxRestoresPerYear: data.maxRestoresPerYear,
        boosterMultiplier: data.boosterMultiplier,
        preExistingDiseaseWaitingMonths: data.preExistingDiseaseWaitingMonths,
        coPaymentPercentage: data.coPaymentPercentage,
        commissionPercentage: data.commissionPercentage
      };

      this.apiService.createTier(tierDto).subscribe({
        next: () => {
          completed++;
          if (completed === tierNames.length && !hasError) {
            this.loading = false;
            this.router.navigate(['/admin/dashboard']);
          }
        },
        error: (err) => {
          hasError = true;
          const msg = typeof err.error === 'string' ? err.error : (err.error?.error || err.message);
          this.errorMessage = 'Plan created, but failed to create some tiers: ' + (msg || 'Server error');
          this.loading = false;
        }
      });
    }
  }
}
