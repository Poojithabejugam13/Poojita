import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { Router, RouterModule } from '@angular/router';

export interface PlanTier {
    tierId: number;
    planId: number;
    tierName: string;
    description?: string;
    basePremium: number;
    coverageLimit: number;
    ageLockProtection: boolean;
    coverageRestoreEnabled: boolean;
    maxRestoresPerYear: number;
    boosterMultiplier: number;
    preExistingDiseaseWaitingMonths: number;
    coPaymentPercentage: number;
    commissionPercentage: number;
}

@Component({
    selector: 'app-plans',
    standalone: true,
    imports: [CommonModule, CurrencyPipe, RouterModule],
    templateUrl: './plans.html',
})
export class Plans implements OnInit {
    public authService = inject(AuthService);
    plans: any[] = [];
    loading = true;

    // Navigation State
    currentStep: 'plans' | 'tiers' = 'plans';
    selectedPlanType: string | null = 'All';
    selectedPlan: any | null = null;

    categories = ['All', 'Individual', 'Family', 'Senior Citizen', 'Specialized Care'];

    private router = inject(Router);

    constructor(private apiService: ApiService) { }

    ngOnInit() {
        this.loadPlans();
    }

    loadPlans() {
        this.loading = true;
        this.apiService.getPlans().subscribe({
            next: (data: any[]) => {
                this.plans = data.map(item => {
                    const sortedTiers = (item.tiers || []).map((t: any) => {
                        return {
                            ...t,
                            description: t.description || 'Comprehensive medical coverage tailored for your health needs.',
                            coverageLimit: t.coverageLimit || t.baseCoverageAmount
                        };
                    }).sort((a: any, b: any) => {
                        const nameA = a.tierName?.toLowerCase() || '';
                        const nameB = b.tierName?.toLowerCase() || '';

                        const getOrder = (name: string) => {
                            if (name.includes('shield')) return 0;
                            if (name.includes('elevate')) return 1;
                            if (name.includes('apex') || name.includes('elite')) return 2;
                            return 3;
                        };

                        return getOrder(nameA) - getOrder(nameB);
                    });

                    return {
                        planId: item.planId,
                        planName: item.planName,
                        planType: item.planType,
                        description: item.description,
                        imageUrl: item.imageUrl,
                        tiers: sortedTiers
                    };
                });
                this.loading = false;
            },
            error: (err) => {
                console.warn('Could not fetch real plans', err);
                this.loading = false;
            }
        });
    }

    setPlanType(type: string) {
        this.selectedPlanType = type;
        this.currentStep = 'plans';
        window.scrollTo(0, 0);
    }

    selectPlan(plan: any) {
        this.selectedPlan = plan;
        this.currentStep = 'tiers';
        window.scrollTo(0, 0);
    }

    buyNow(plan: any) {
        // Find the first tier (Shield/lowest)
        const tierId = plan.tiers?.[0]?.tierId;
        this.router.navigate(['/customer/purchase-policy'], {
            queryParams: {
                planId: plan.planId,
                tierId: tierId
            }
        });
    }

    goBack() {
        if (this.currentStep === 'tiers') {
            this.currentStep = 'plans';
            this.selectedPlan = null;
        }
        window.scrollTo(0, 0);
    }

    getFilteredPlans() {
        if (!this.selectedPlanType) return [];
        if (this.selectedPlanType === 'All') return this.plans;
        return this.plans.filter(p => p.planType === this.selectedPlanType);
    }

    // loadMockFallbackPlans() {
    //     this.plans = [
    //         {
    //             planId: 991, planName: 'Essential Health Shield', category: 'Individual', tiers: [
    //                 { tierId: 9911, tierName: 'Standard', description: 'Essential coverage for basic health needs.', basePremium: 150, baseCoverageAmount: 50000, coPaymentPercentage: 20, preExistingDiseaseWaitingMonths: 48, coverageRestoreEnabled: false, boosterMultiplier: 1, ageLockProtection: false },
    //                 { tierId: 9912, tierName: 'Silver', description: 'Enhanced protection balancing cost and broad coverage.', basePremium: 250, baseCoverageAmount: 100000, coPaymentPercentage: 15, preExistingDiseaseWaitingMonths: 24, coverageRestoreEnabled: true, maxRestoresPerYear: 1, boosterMultiplier: 1.5, ageLockProtection: false }
    //             ]
    //         },
    //         {
    //             planId: 992, planName: 'Comprehensive Care Family', category: 'Family', tiers: [
    //                 { tierId: 9921, tierName: 'Gold', description: 'Comprehensive care with great benefits.', basePremium: 400, baseCoverageAmount: 250000, coPaymentPercentage: 10, preExistingDiseaseWaitingMonths: 12, coverageRestoreEnabled: true, maxRestoresPerYear: 2, boosterMultiplier: 2, ageLockProtection: true },
    //                 { tierId: 9922, tierName: 'Platinum', description: 'Maximum peace of mind with premium capabilities.', basePremium: 650, baseCoverageAmount: 500000, coPaymentPercentage: 5, preExistingDiseaseWaitingMonths: 0, coverageRestoreEnabled: true, maxRestoresPerYear: 3, boosterMultiplier: 3, ageLockProtection: true }
    //             ]
    //         },
    //         {
    //             planId: 993, planName: 'Heart Vitality Plus', category: 'Specialized Care', tiers: [
    //                 { tierId: 9931, tierName: 'Gold', description: 'Tailored care for cardiac conditions.', basePremium: 350, baseCoverageAmount: 180000, coPaymentPercentage: 15, preExistingDiseaseWaitingMonths: 6, coverageRestoreEnabled: true, maxRestoresPerYear: 1, boosterMultiplier: 1, ageLockProtection: true },
    //             ]
    //         },
    //         {
    //             planId: 994, planName: 'Senior Safe Guard', category: 'Senior Citizen', tiers: [
    //                 { tierId: 9941, tierName: 'Silver', description: 'Dedicated coverage for older adults.', basePremium: 300, baseCoverageAmount: 150000, coPaymentPercentage: 20, preExistingDiseaseWaitingMonths: 12, coverageRestoreEnabled: false, maxRestoresPerYear: 0, boosterMultiplier: 1, ageLockProtection: false },
    //             ]
    //         }
    //     ];
    // }
    // this.loading = false;               

}
