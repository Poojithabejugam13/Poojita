import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-edit-plan',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './edit-plan.component.html'
})
export class EditPlanComponent implements OnInit {
  private apiService = inject(ApiService);
  private router = inject(Router);

  plans: any[] = [];
  loading = true;
  saving = false;
  errorMessage = '';
  successMessage = '';
  fieldErrors: { [key: string]: string } = {};

  // View State
  currentView: 'select' | 'edit' = 'select';

  // Selection State
  categories = ['All', 'Individual', 'Family', 'Senior Citizen', 'Specialized Care'];
  selectedPlanType = 'All';
  selectedPlan: any = null;
  selectedTierId: number | null = null;

  // Form State mapped to PlanTier
  planForm = {
    planId: 0,
    planName: '',
    description: '',
    planType: 'Individual',

    // Tier specific
    tierId: 0,
    tierName: '',
    basePremium: 0,
    coverageLimit: 0,
    ageLockProtection: false,
    coverageRestoreEnabled: true,
    maxRestoresPerYear: 1,
    boosterMultiplier: 2,
    preExistingDiseaseWaitingMonths: 24,
    coPaymentPercentage: 10,
    commissionPercentage: 0,
    imageUrl: '',
    deleteImage: false
  };
  newPlanImage: File | null = null;
  imagePreviewUrl: string | null = null;

  ngOnInit() {
    this.loadPlans();
  }

  loadPlans() {
    this.loading = true;
    this.apiService.getPlans().subscribe({
      next: (data: any[]) => {
        this.plans = data.map(item => {
          return {
            planId: item.planId,
            planName: item.planName,
            description: item.description || '',
            planType: item.planType || 'Individual',
            imageUrl: item.imageUrl,
            tiers: item.tiers || []
          };
        });
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = 'Failed to load plans.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  onImageSelected(event: any) {
    if (event.target.files && event.target.files.length > 0) {
      const file = event.target.files[0];
      this.newPlanImage = file;
      this.planForm.deleteImage = false;

      // Generate Preview
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreviewUrl = reader.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  deleteImage() {
    this.planForm.imageUrl = '';
    this.planForm.deleteImage = true;
    this.newPlanImage = null;
    this.imagePreviewUrl = null;
  }

  setPlanType(type: string) {
    this.selectedPlanType = type;
  }

  getFilteredPlans() {
    if (this.selectedPlanType === 'All') return this.plans;
    return this.plans.filter(p => p.planType === this.selectedPlanType);
  }

  selectPlanToEdit(plan: any) {
    this.selectedPlan = plan;
    this.currentView = 'edit';
    this.successMessage = '';
    this.errorMessage = '';
    this.fieldErrors = {};

    // Auto-select the first tier to populate the form
    if (plan.tiers && plan.tiers.length > 0) {
      this.onTierSelected(plan.tiers[0].tierId);
    }
  }

  onTierSelected(tierId: number | string) {
    if (!this.selectedPlan) return;
    const tid = Number(tierId);
    this.selectedTierId = tid;

    const tier = this.selectedPlan.tiers.find((t: any) => t.tierId === tid);
    if (tier) {
      this.planForm = {
        planId: this.selectedPlan.planId,
        planName: this.selectedPlan.planName,
        description: this.selectedPlan.description || '',
        planType: this.selectedPlan.planType || 'Individual',
        tierId: tier.tierId,
        tierName: tier.tierName,
        basePremium: tier.basePremium,
        coverageLimit: tier.coverageLimit,
        ageLockProtection: tier.ageLockProtection,
        coverageRestoreEnabled: tier.coverageRestoreEnabled,
        maxRestoresPerYear: tier.maxRestoresPerYear,
        boosterMultiplier: tier.boosterMultiplier,
        preExistingDiseaseWaitingMonths: tier.preExistingDiseaseWaitingMonths,
        coPaymentPercentage: tier.coPaymentPercentage,
        commissionPercentage: tier.commissionPercentage,
        imageUrl: this.selectedPlan.imageUrl,
        deleteImage: false
      };
      this.newPlanImage = null;
      this.imagePreviewUrl = null;
      this.fieldErrors = {};
    }
  }

  cancelEdit() {
    this.currentView = 'select';
    this.selectedPlan = null;
    this.selectedTierId = null;
    window.scrollTo(0, 0);
  }

  validate(): boolean {
    this.fieldErrors = {};
    if (!this.planForm.planName) this.fieldErrors['planName'] = 'Plan name required';
    if (this.planForm.basePremium <= 0) this.fieldErrors['basePremium'] = 'Must be > 0';
    if (this.planForm.coverageLimit <= 0) this.fieldErrors['coverageLimit'] = 'Must be > 0';
    if (this.planForm.commissionPercentage < 0 || this.planForm.commissionPercentage > 100)
      this.fieldErrors['commissionPercentage'] = '0-100%';
    if (this.planForm.coPaymentPercentage < 0 || this.planForm.coPaymentPercentage > 100)
      this.fieldErrors['coPaymentPercentage'] = '0-100%';

    return Object.keys(this.fieldErrors).length === 0;
  }

  submitEdit() {
    if (!this.validate()) return;

    this.saving = true;
    this.errorMessage = '';
    this.successMessage = '';

    const formData = new FormData();
    formData.append('TierId', this.planForm.tierId.toString());
    formData.append('PlanName', this.planForm.planName);
    formData.append('Description', this.planForm.description);
    formData.append('PlanType', this.planForm.planType);
    formData.append('TierName', this.planForm.tierName);
    formData.append('BasePremium', this.planForm.basePremium.toString());
    formData.append('BaseCoverageAmount', this.planForm.coverageLimit.toString());
    formData.append('AgeLockProtection', this.planForm.ageLockProtection.toString());
    formData.append('CoverageRestoreEnabled', this.planForm.coverageRestoreEnabled.toString());
    formData.append('MaxRestoresPerYear', this.planForm.maxRestoresPerYear.toString());
    formData.append('BoosterMultiplier', this.planForm.boosterMultiplier.toString());
    formData.append('PreExistingDiseaseWaitingMonths', this.planForm.preExistingDiseaseWaitingMonths.toString());
    formData.append('CoPaymentPercentage', this.planForm.coPaymentPercentage.toString());
    formData.append('CommissionPercentage', this.planForm.commissionPercentage.toString());
    formData.append('DeleteImage', this.planForm.deleteImage.toString());

    if (this.newPlanImage) {
      formData.append('Image', this.newPlanImage);
    }

    this.apiService.updatePlan(this.planForm.planId, formData).subscribe({
      next: () => {
        this.successMessage = 'Updated successfully!';
        this.saving = false;
        this.loadPlans();

        setTimeout(() => {
          this.cancelEdit();
        }, 2000);
      },
      error: (err) => {
        const msg = typeof err.error === 'string' ? err.error : (err.error?.error || err.message);
        this.errorMessage = 'Update failed: ' + (msg || 'Server error');
        this.saving = false;
      }
    });
  }
}
