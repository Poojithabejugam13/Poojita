import { Component, inject, OnInit } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';

export interface PlanTier {
    tierId: number;
    planId: number;
    tierName: string;
    description?: string;
    basePremium: number;
    baseCoverageAmount: number;
    coverageLimit: number;
    ageLockProtection: boolean;
    coverageRestoreEnabled: boolean;
    maxRestoresPerYear: number;
    boosterMultiplier: number;
    preExistingDiseaseWaitingMonths: number;
    coPaymentPercentage: number;
}

export interface InsurancePlan {
    planId: number;
    planName: string;
    planType: string;
    imageUrl: string;
    description?: string;
    tiers?: PlanTier[];
}

interface Proposer {
    name: string;
    dob: string;
    gender: 'Male' | 'Female' | 'Other' | '';
    mobile: string;
    address: string;
}

interface HealthDetails {
    heightCm: number;
    weightKg: number;
    isSmoker: boolean;
    selectedPed: string;
    manualPed: string;
    preExistingDiseases: string[];
}

interface Member {
    name: string;
    dob: string;
    gender: 'Male' | 'Female' | 'Other' | '';
    relation: string;
}

interface CategoryGroup {
    type: string;
    plans: InsurancePlan[];
}

@Component({
    selector: 'app-policy-wizard',
    standalone: true,
    imports: [CommonModule, FormsModule, CurrencyPipe, RouterLink],
    templateUrl: './policy-wizard.html',
    styleUrls: ['./policy-wizard.css']
})
export class PolicyWizard implements OnInit {
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private apiService = inject(ApiService);

    currentStep = 1;
    totalSteps = 6;

    // STEP 1 State
    allPlans: InsurancePlan[] = [];
    groupedCategories: CategoryGroup[] = [];
    selectedPlanType: string = 'Individual'; // "Individual", "Family", "Senior Citizen", "Specialized Care"
    selectedPlan: InsurancePlan | null = null;
    selectedTier: PlanTier | null = null;
    policyStartDate: string = new Date().toISOString().split('T')[0];
    isDirectPurchase = false;

    // STEP 2 State (Proposer)
    proposer: Proposer = {
        name: '',
        dob: '',
        gender: '',
        mobile: '',
        address: ''
    };

    // STEP 3 State (Health)
    health: HealthDetails = {
        heightCm: 170,
        weightKg: 70,
        isSmoker: false,
        selectedPed: '',
        manualPed: '',
        preExistingDiseases: []
    };
    bmiValue = 0;

    // STEP 4 State (Members)
    members: Member[] = [];

    // STEP 5 State (Nominees)
    nominees: { name: string, relation: string, share: number }[] = [
        { name: '', relation: '', share: 100 }
    ];

    // STEP 6 State (Document)
    documentPreview: string | null = null;
    selectedFile: File | null = null;

    validationError = '';
    fieldErrors: { [key: string]: string } = {};

    actionMessage = '';
    actionType: 'success' | 'error' | 'loading' = 'success';
    submitSuccess = false;

    // RENEWAL State
    policyId: number | null = null;

    // PRICING State
    basePremium = 0;
    ageLoading = 0;
    bmiLoading = 0;
    smokingLoading = 0;
    preTaxTotal = 0;
    taxAmount = 0;
    finalPremium = 0;

    loadingPlans = true;

    ngOnInit() {
        this.loadingPlans = true;
        this.apiService.getPlans().subscribe({
            next: (data: any[]) => {
                this.allPlans = (data || []).map(item => {
                    return {
                        planId: item.planId,
                        planName: item.planName || 'Unnamed Plan',
                        planType: item.planType,
                        imageUrl: item.imageUrl,
                        description: item.description,
                        tiers: (item.tiers || []).map((t: any) => ({
                            tierId: t.tierId,
                            planId: t.planId,
                            tierName: t.tierName || 'Standard Tier',
                            description: t.description || 'Comprehensive coverage for your health needs.',
                            basePremium: t.basePremium || 0,
                            baseCoverageAmount: t.baseCoverageAmount || t.coverageLimit || 0,
                            coverageLimit: t.coverageLimit || t.baseCoverageAmount || 0,
                            ageLockProtection: !!t.ageLockProtection,
                            coverageRestoreEnabled: !!t.coverageRestoreEnabled,
                            maxRestoresPerYear: t.maxRestoresPerYear || 0,
                            boosterMultiplier: t.boosterMultiplier || 1,
                            preExistingDiseaseWaitingMonths: t.preExistingDiseaseWaitingMonths || 0,
                            coPaymentPercentage: t.coPaymentPercentage || 0
                        })).sort((a: any, b: any) => {
                            const getOrder = (name: string) => {
                                const n = (name || '').toLowerCase();
                                if (n.includes('shield')) return 0;
                                if (n.includes('elevate')) return 1;
                                if (n.includes('apex') || n.includes('elite')) return 2;
                                return 3;
                            };
                            return getOrder(a.tierName) - getOrder(b.tierName);
                        })
                    };
                });

                this.groupPlansByType();
                this.loadingPlans = false;

                this.route.queryParams.subscribe(params => {
                    if (params['policyId']) {
                        this.policyId = Number(params['policyId']);
                        this.fetchPolicyForAutofill(this.policyId);
                    } else if (params['planId']) {
                        const planId = Number(params['planId']);
                        const foundPlan = this.allPlans.find(p => p.planId === planId);
                        if (foundPlan) {
                            this.selectedPlanType = foundPlan.planType!;
                            this.selectedPlan = foundPlan;
                            this.isDirectPurchase = true;

                            if (params['tierId']) {
                                const tierId = Number(params['tierId']);
                                const foundTier = foundPlan.tiers?.find(t => t.tierId === tierId);
                                if (foundTier) {
                                    this.selectedTier = foundTier;
                                    this.basePremium = foundTier.basePremium;
                                }
                            }
                        }
                    }
                });
            }
        });
    }

    private fetchPolicyForAutofill(policyId: number) {
        this.apiService.getPolicyById(policyId).subscribe({
            next: (res: any) => {
                const pol = res.policy;
                const noms = res.nominees;

                // 1. Select Plan and Tier
                const foundPlan = this.allPlans.find(p => p.planId === pol.planId);
                if (foundPlan) {
                    this.selectedPlanType = foundPlan.planType!;
                    this.selectedPlan = foundPlan;
                    this.selectedTier = foundPlan.tiers?.find(t => t.tierId === pol.tierId) || null;
                    this.isDirectPurchase = true;
                }

                // 2. Pre-fill Proposer
                this.proposer = {
                    name: pol.fullName,
                    dob: pol.dateOfBirth?.split('T')[0] || '',
                    gender: pol.gender || '',
                    mobile: pol.mobileNumber || '',
                    address: pol.address || ''
                };

                // 3. Pre-fill Health
                this.health = {
                    heightCm: pol.heightCm || 170,
                    weightKg: pol.weightKg || 70,
                    isSmoker: pol.isSmoker || false,
                    selectedPed: '',
                    manualPed: '',
                    preExistingDiseases: pol.preExistingDiseases ? pol.preExistingDiseases.split(',').map((s: string) => s.trim()).filter((s: string) => s) : []
                };

                // 4. Pre-fill Members (assuming Step 1-4 sync logic)
                // We'll set members from the proposer for now as DB only stores proposer details in main table
                // For a more advanced family plan, we'd need a separate members table.
                this.members = [{
                    name: this.proposer.name,
                    dob: this.proposer.dob,
                    gender: this.proposer.gender as any,
                    relation: 'Self'
                }];

                // 5. Pre-fill Nominees
                if (noms && noms.length > 0) {
                    this.nominees = noms.map((n: any) => ({
                        name: n.nomineeName,
                        relation: n.relationship,
                        share: n.percentageShare
                    }));
                }
            },
            error: (err) => this.showStatus('Failed to load existing policy details for renewal.', 'error')
        });
    }

    private showStatus(msg: string, type: 'success' | 'error') {
        this.actionMessage = msg;
        this.actionType = type;
        setTimeout(() => this.actionMessage = '', 3000);
    }

    groupPlansByType() {
        const typeMap = new Map<string, InsurancePlan[]>();
        ['Individual', 'Family', 'Senior Citizen', 'Specialized Care'].forEach(type => typeMap.set(type, []));

        this.allPlans.forEach(plan => {
            const type = plan.planType!;
            const arr = typeMap.get(type) || [];
            arr.push(plan);
            typeMap.set(type, arr);
        });
        this.groupedCategories = Array.from(typeMap.entries()).filter(([_, plans]) => plans.length > 0).map(([type, plans]) => ({ type, plans }));
    }

    selectPlanTier(plan: InsurancePlan, tier: PlanTier) {
        this.selectedPlan = plan;
        this.selectedTier = tier;
        this.validationError = '';
        this.resetFollowingSteps();
    }

    resetFollowingSteps() {
        // Always seed member list with the proposer as the first member.
        // For Family plans, user can then add additional members.
        const firstMember = { name: this.proposer.name, dob: this.proposer.dob, gender: this.proposer.gender, relation: 'Self' } as Member;
        this.members = [firstMember];
    }

    calculateAge(dobString: string): number {
        if (!dobString) return 0;
        const dob = new Date(dobString);
        const ageDifMs = Date.now() - dob.getTime();
        const ageDate = new Date(ageDifMs);
        return Math.abs(ageDate.getUTCFullYear() - 1970);
    }

    onPedChange() {
        if (this.health.selectedPed && this.health.selectedPed !== 'Other') {
            if (!this.health.preExistingDiseases.includes(this.health.selectedPed)) {
                this.health.preExistingDiseases.push(this.health.selectedPed);
            }
            this.health.selectedPed = '';
        }
    }

    addManualPed() {
        if (this.health.manualPed && this.health.manualPed.trim()) {
            const val = this.health.manualPed.trim();
            if (!this.health.preExistingDiseases.includes(val)) {
                this.health.preExistingDiseases.push(val);
            }
            this.health.manualPed = '';
            this.health.selectedPed = '';
        }
    }

    removePed(index: number) {
        this.health.preExistingDiseases.splice(index, 1);
    }

    calculateBMI() {
        const heightM = this.health.heightCm / 100;
        if (heightM > 0) {
            this.bmiValue = this.health.weightKg / (heightM * heightM);
        } else {
            this.bmiValue = 0;
        }
    }

    calculatePremium() {
        if (!this.selectedTier) return;

        const base = Number(this.selectedTier.basePremium);
        const age = this.calculateAge(this.proposer.dob);

        // 1. Age Loading
        this.ageLoading = (age > 40) ? (base * 0.10) : 0;
        const subtotal = base + this.ageLoading;

        // 2. BMI Loading
        this.calculateBMI();
        let bmiFactor = 0;
        if (this.bmiValue < 18.5) bmiFactor = 0.05;
        else if (this.bmiValue >= 18.5 && this.bmiValue <= 24.9) bmiFactor = 0;
        else if (this.bmiValue >= 25 && this.bmiValue <= 29.9) bmiFactor = 0.10;
        else if (this.bmiValue >= 30) bmiFactor = 0.20;

        // 3. Smoking Factor
        const smokingFactor = this.health.isSmoker ? 0.25 : 0;

        // 4. Risk Loading
        this.bmiLoading = subtotal * bmiFactor;
        this.smokingLoading = subtotal * smokingFactor;
        const totalRisk = this.bmiLoading + this.smokingLoading;

        this.preTaxTotal = subtotal + totalRisk;
        this.taxAmount = this.preTaxTotal * 0.18; // 18% GST
        this.finalPremium = this.preTaxTotal + this.taxAmount;
    }

    validateStep1(): boolean {
        if (!this.selectedPlan || !this.selectedTier || !this.selectedPlanType || !this.policyStartDate) {
            this.validationError = 'Please select a plan, tier, and policy start date.';
            return false;
        }
        return true;
    }

    validateStep2(): boolean {
        const p = this.proposer;
        this.fieldErrors = {};
        let isValid = true;

        if (!p.name || p.name.length < 3) {
            this.fieldErrors['name'] = 'Full name must be at least 3 characters.';
            isValid = false;
        }

        if (!p.dob) {
            this.fieldErrors['dob'] = 'Date of birth is required.';
            isValid = false;
        } else {
            const age = this.calculateAge(p.dob);
            if (age < 18) {
                this.fieldErrors['dob'] = 'Proposer must be at least 18 years old.';
                isValid = false;
            }
        }

        if (!p.gender) {
            this.fieldErrors['gender'] = 'Gender is required.';
            isValid = false;
        }

        if (!p.mobile || !/^\d{10}$/.test(p.mobile)) {
            this.fieldErrors['mobile'] = 'Enter a valid 10-digit mobile number.';
            isValid = false;
        }

        if (!p.address || p.address.length < 10) {
            this.fieldErrors['address'] = 'Please provide a complete address (min 10 chars).';
            isValid = false;
        }

        if (!isValid) {
            this.validationError = 'Please correct the highlighted errors.';
        }
        return isValid;
    }

    validateStep3(): boolean {
        this.fieldErrors = {};
        let isValid = true;

        if (!this.health.heightCm || this.health.heightCm < 50 || this.health.heightCm > 250) {
            this.fieldErrors['height'] = 'Height must be between 50 and 250 cm.';
            isValid = false;
        }
        if (!this.health.weightKg || this.health.weightKg < 5 || this.health.weightKg > 300) {
            this.fieldErrors['weight'] = 'Weight must be between 5 and 300 kg.';
            isValid = false;
        }

        if (!isValid) {
            this.validationError = 'Please enter valid health metrics.';
        }
        return isValid;
    }

    validateStep4(): boolean {
        const type = this.selectedPlanType;
        const count = this.members.length;
        this.fieldErrors = {};
        let isValid = true;

        if (type === 'Individual' && count !== 1) {
            this.validationError = 'Individual plans cover exactly 1 member (the proposer).';
            return false;
        }
        if (type === 'Family' && count < 2) {
            this.validationError = 'Family Floater plans require a minimum of 2 members.';
            return false;
        }
        if (type === 'Senior Citizen') {
            if (count !== 1) {
                this.validationError = 'Senior Citizen plans cover exactly 1 member (age 60+).';
                return false;
            }
            if (this.calculateAge(this.members[0].dob) < 60) {
                this.validationError = 'Senior Citizen plans require the insured member to be age 60 or above.';
                return false;
            }
        }
        if (type === 'Specialized Care' && count !== 1) {
            this.validationError = 'Specialised Care plans cover exactly 1 member.';
            return false;
        }

        this.members.forEach((m, i) => {
            if (!m.name || m.name.length < 3) {
                this.fieldErrors[`member_name_${i}`] = 'Name is required (min 3 chars).';
                isValid = false;
            }
            if (!m.dob) {
                this.fieldErrors[`member_dob_${i}`] = 'Date of birth is required.';
                isValid = false;
            }
            if (!m.gender) {
                this.fieldErrors[`member_gender_${i}`] = 'Gender is required.';
                isValid = false;
            }
            if (!m.relation) {
                this.fieldErrors[`member_relation_${i}`] = 'Relation is required.';
                isValid = false;
            }
        });

        if (!isValid && !this.validationError) {
            this.validationError = 'Please complete all member details before continuing.';
        }
        return isValid;
    }

    validateStep5(): boolean {
        const type = this.selectedPlanType;
        const count = this.nominees.length;
        this.fieldErrors = {};
        let isValid = true;

        if (type === 'Family' && count < 2) {
            this.validationError = 'Family plans MANDATORILY require at least 2 nominees.';
            return false;
        }
        if (count < 1) {
            this.validationError = 'At least 1 nominee is required.';
            return false;
        }

        let totalShare = 0;
        this.nominees.forEach((n, i) => {
            if (!n.name) {
                this.fieldErrors[`nominee_name_${i}`] = 'Nominee name is required.';
                isValid = false;
            }
            if (!n.relation) {
                this.fieldErrors[`nominee_relation_${i}`] = 'Relationship is required.';
                isValid = false;
            }
            if (n.share <= 0 || n.share > 100) {
                this.fieldErrors[`nominee_share_${i}`] = 'Share must be between 1 and 100.';
                isValid = false;
            }
            totalShare += Number(n.share);
        });

        if (totalShare !== 100) {
            this.validationError = 'Total nominee share must equal exactly 100%. Current: ' + totalShare + '%';
            isValid = false;
        }

        if (!isValid && !this.validationError) {
            this.validationError = 'Please fix the errors in the nominee list.';
        }
        return isValid;
    }

    nextStep() {
        this.validationError = '';
        if (this.currentStep === 1 && !this.validateStep1()) return;
        if (this.currentStep === 2 && !this.validateStep2()) {
            return;
        } else if (this.currentStep === 2) {
            // Auto-sync proposer to member[0]
            this.resetFollowingSteps();
        }

        if (this.currentStep === 3 && !this.validateStep3()) return;

        if (this.currentStep === 4 && !this.validateStep4()) return;

        if (this.currentStep === 5) {
            if (!this.validateStep5()) return;
            this.calculatePremium();
        }

        if (this.currentStep < this.totalSteps) {
            this.currentStep++;
        }
    }

    prevStep() {
        if (this.currentStep > 1) {
            this.currentStep--;
            this.validationError = '';
        }
    }

    addMember() {
        this.members.push({ name: '', dob: '', gender: '', relation: '' });
    }
    removeMember(i: number) { this.members.splice(i, 1); }

    addNominee() {
        this.nominees.push({ name: '', relation: '', share: 0 });
    }
    removeNominee(i: number) { this.nominees.splice(i, 1); }

    onFileSelected(event: any) {
        const file: File = event.target.files[0];
        if (file) {
            this.selectedFile = file;
            this.documentPreview = file.name;
        }
    }

    submitPolicy() {
        if (!this.selectedFile) {
            this.validationError = 'Aadhaar document upload is required for verification.';
            return;
        }
        this.apiService.uploadDocument(this.selectedFile).subscribe({
            next: (res) => this.executeRequest(res.url),
            error: (err) => this.validationError = 'Upload failed: ' + err.message
        });
    }

    private executeRequest(docPath: string) {
        if (!this.selectedPlan || !this.selectedTier) return;
        // NOTE: Premium fields (totalPremium, ageLoading, taxAmount) are intentionally
        // NOT sent. The backend PremiumCalculator domain service computes these authoritatively.
        const payload = {
            planId: this.selectedPlan.planId,
            tierId: this.selectedTier.tierId,
            age: this.calculateAge(this.proposer.dob),
            planType: this.selectedPlanType,
            policyStartDate: this.policyStartDate,
            fullName: this.proposer.name,
            dateOfBirth: this.proposer.dob,
            gender: this.proposer.gender,
            mobileNumber: this.proposer.mobile,
            address: this.proposer.address,
            heightCm: this.health.heightCm,
            weightKg: this.health.weightKg,
            isSmoker: this.health.isSmoker,
            preExistingDiseases: this.health.preExistingDiseases.join(', '),
            documentUrl: docPath,
            nominees: this.nominees.map(n => ({
                nomineeName: n.name,
                relationship: n.relation,
                percentageShare: Number(n.share)
            }))
        };

        this.apiService.requestPolicy(payload).subscribe({
            next: () => {
                this.submitSuccess = true;
                this.actionMessage = `✓ Policy application for ${this.selectedPlan?.planName} (${this.selectedTier?.tierName}) submitted successfully! Awaiting agent verification.`;
                this.actionType = 'success';
                setTimeout(() => this.router.navigate(['/customer/dashboard']), 3000);
            },
            error: (err) => {
                this.actionMessage = 'Submission failed: ' + (err.error || err.message);
                this.actionType = 'error';
            }
        });
    }
}
