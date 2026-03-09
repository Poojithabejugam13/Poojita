import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Chart } from 'chart.js/auto';
import { ApiService } from '../../../core/services/api.service';

@Component({
    selector: 'app-customer-dashboard',
    standalone: true,
    imports: [CommonModule, RouterLink, CurrencyPipe, DatePipe, FormsModule],
    templateUrl: './customer-dashboard.html',
    styleUrls: ['./customer-dashboard.css']
})
export class CustomerDashboard implements OnInit, OnDestroy {
    private apiService = inject(ApiService);
    private router = inject(Router);

    userName = '';
    activePoliciesCount = 0;
    pendingClaimsCount = 0;
    approvedClaims = 0;
    rejectedClaims = 0;
    totalPremiumDue = 0;
    dueDaysCount = 0;
    paymentDueDate = new Date(new Date().setDate(new Date().getDate() + 5));
    assignedAgentName = 'Not Assigned';
    assignedAgentPhone = 'N/A';
    assignedOfficerName = 'Not Assigned';
    assignedOfficerPhone = 'N/A';

    policies: any[] = [];
    claims: any[] = [];
    paidPolicyIds = new Set<number>();

    // Claim Modal State
    showClaimModal = false;
    isSubmittingClaim = false;
    claimData = {
        policyId: 0,
        amount: null as number | null,
        reason: '',
        file: null as File | null
    };
    plansMap = new Map<number, any>();
    chartInstance: any;

    // Payment & Invoice State
    isProcessingPayment = false;
    showInvoice = false;
    invoiceDate = new Date();
    invoiceNumber = 'INV-' + Math.floor(100000 + Math.random() * 900000);

    // Colored status feedback
    actionMessage = '';
    actionType: 'success' | 'error' | 'warning' = 'success';

    // Renewal result (shows backend-computed domain data)
    renewalResult: any = null;

    // Purchase Selection State
    availablePlans: any[] = [];
    selectedPlanId: number | null = null;
    selectedTierId: number | null = null;
    selectedPlanTiers: any[] = [];
    isPurchasing = false;
    private refreshInterval: any;

    ngOnInit(): void {
        this.loadDashboardData();
        // Poll every 30 seconds to pick up coverage changes after Claims Officer approval
        this.refreshInterval = setInterval(() => this.fetchPolicies(), 30000);
    }

    ngOnDestroy(): void {
        if (this.refreshInterval) clearInterval(this.refreshInterval);
    }

    refreshPolicies() {
        this.fetchPolicies();
        this.apiService.getMyClaims().subscribe(claims => {
            this.claims = claims;
            this.pendingClaimsCount = claims.filter((c: any) => c.status === 0).length;
            this.approvedClaims = claims.filter((c: any) => c.status === 1).length;
            this.rejectedClaims = claims.filter((c: any) => c.status === 2).length;
            this.initChart();
        });
        this.showStatus('Coverage balance refreshed!', 'success');
    }

    loadDashboardData() {
        this.apiService.getPlans().subscribe(plans => {
            const grouped = new Map<number, any>();
            plans.forEach((p: any) => {
                this.plansMap.set(p.planId, p);
                if (!grouped.has(p.planId)) {
                    grouped.set(p.planId, { planId: p.planId, planName: p.planName, tiers: [] });
                }
                grouped.get(p.planId).tiers.push({ tierId: p.tierId, tierName: p.tierName });
            });
            this.availablePlans = Array.from(grouped.values()).map((p: any) => {
                p.tiers.sort((a: any, b: any) => {
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
                return p;
            });
            this.fetchUserName();
            this.fetchPolicies();
        });

        this.apiService.getCustomerDashboard().subscribe(res => {
            this.activePoliciesCount = res.activePolicies;
            this.assignedAgentName = res.assignedAgentName || 'Not Assigned';
            this.assignedAgentPhone = res.assignedAgentPhone || 'N/A';
            this.assignedOfficerName = res.assignedOfficerName || 'Not Assigned';
            this.assignedOfficerPhone = res.assignedOfficerPhone || 'N/A';
        });

        this.apiService.getMyClaims().subscribe(claims => {
            this.claims = claims;
            this.pendingClaimsCount = claims.filter((c: any) => c.status === 0).length;
            this.approvedClaims = claims.filter((c: any) => c.status === 1).length;
            this.rejectedClaims = claims.filter((c: any) => c.status === 2).length;
            this.initChart();
        });
    }

    onPlanSelected() {
        const plan = this.availablePlans.find(p => String(p.planId) === String(this.selectedPlanId));
        this.selectedPlanTiers = plan ? plan.tiers : [];
        this.selectedTierId = null;
    }

    navigateToWizard() {
        if (!this.selectedPlanId || !this.selectedTierId) {
            alert('Please select both a plan and a tier.');
            return;
        }
        // Redirect to 6-step wizard
        this.router.navigate(['/customer/purchase-policy'], {
            queryParams: {
                planId: this.selectedPlanId,
                tierId: this.selectedTierId
            }
        });
    }

    getUserId() {
        const token = localStorage.getItem('token');
        if (!token) return 'unknown';
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            return payload['nameid'] || payload['sub'] || payload['id'] || 'unknown';
        } catch (e) {
            return 'unknown';
        }
    }

    showStatus(msg: string, type: 'success' | 'error' | 'warning' = 'success') {
        this.actionMessage = msg;
        this.actionType = type;
        setTimeout(() => this.actionMessage = '', 5000);
    }

    renewPolicy(policyId: number) {
        const policy = this.policies.find((p: any) => p.id === policyId);
        if (!policy) return;

        this.showStatus('Submitting renewal request...', 'success');
        this.apiService.renewPolicy(policyId).subscribe({
            next: () => {
                this.showStatus('✓ Policy renewed successfully! Coverage refilled. Awaiting agent approval.', 'success');
                this.loadDashboardData();
            },
            error: (err) => {
                let msg = 'Renewal failed';
                if (err.error) {
                    try {
                        const parsed = typeof err.error === 'string' ? JSON.parse(err.error) : err.error;
                        msg = parsed.error || parsed.message || err.error;
                    } catch { msg = err.error; }
                }
                this.showStatus(`Renewal failed: ${msg}`, 'error');
            }
        });
    }

    fetchPolicies() {
        this.apiService.getMyPolicies().subscribe(pols => {
            this.policies = pols.map((pol: any) => {
                const plan = this.plansMap.get(pol.planId);
                const tier = plan?.tiers?.find((t: any) => t.tierId === pol.tierId);
                let _status = 'Pending Approval';
                if (pol.status === 1) _status = 'Approved';
                if (pol.status === 2) _status = 'Rejected';
                if (pol.status === 3) _status = 'Active';
                if (pol.status === 4) _status = 'Expired';
                if (pol.status === 5) _status = 'Cancelled';

                return {
                    id: pol.policyId,
                    planId: pol.planId,
                    tierId: pol.tierId,
                    planName: plan ? plan.planName : `Plan #${pol.planId}`,
                    tierName: tier ? tier.tierName : null,
                    planType: plan ? plan.planType : null,
                    totalPremium: pol.totalPremium ?? 0,
                    remainingCoverage: pol.remainingCoverageAmount ?? 0,
                    maxCoverage: pol.annualMaxCoverage ?? 0,
                    rejectionReason: pol.rejectionReason ?? null,
                    decisionReason: pol.decisionReason ?? null,
                    nomineeName: pol.nominees?.[0]?.nomineeName || pol.nomineeName || null,
                    status: _status,
                    createdAt: pol.createdAt
                };
            });

            this.totalPremiumDue = this.policies
                .filter((p: any) => (p.status === 'Active' || p.status === 'Approved') && !this.paidPolicyIds.has(p.id))
                .reduce((sum: number, p: any) => sum + (p.totalPremium / 12), 0);
        });
    }

    generatedInvoices: string[] = [];
    paidPolicies: any[] = [];
    invoiceTotalMonthly = 0;

    payPremium(policy: any) {
        this.isProcessingPayment = true;
        this.generatedInvoices = [];
        this.paidPolicies = [policy];
        this.invoiceTotalMonthly = policy.totalPremium / 12;

        const payload = {
            policyId: policy.id,
            amount: policy.totalPremium / 12
        };

        this.apiService.makePayment(payload).subscribe({
            next: (res: any) => {
                if (res && res.invoiceUrl) {
                    this.generatedInvoices.push(res.invoiceUrl);
                }
                policy.status = 'Active';
                this.paidPolicyIds.add(policy.id);
                this.isProcessingPayment = false;
                this.showInvoice = true;
                this.showStatus(`✓ Payment successful! Policy POL-${policy.id} is now ACTIVE. Invoice generated.`, 'success');
                this.fetchPolicies();
            },
            error: (err) => {
                console.error("Payment failed", err);
                this.showStatus("Payment failed: " + (err.error || 'Server error'), 'error');
                this.isProcessingPayment = false;
            }
        });
    }

    closeInvoice() {
        this.showInvoice = false;
    }

    scrollToPolicies() {
        document.getElementById('policies-section')?.scrollIntoView({ behavior: 'smooth' });
    }

    openClaimModal(policyId: number) {
        this.claimData = { policyId, amount: null, reason: '', file: null };
        this.showClaimModal = true;
    }

    closeClaimModal() {
        this.showClaimModal = false;
    }

    onFileSelected(event: any) {
        const file = event.target.files[0];
        if (file) {
            this.claimData.file = file;
        }
    }

    submitClaim() {
        if (!this.claimData.amount || this.claimData.amount <= 0 || !this.claimData.reason.trim()) {
            this.showStatus('Please enter a valid claim amount and reason.', 'warning');
            return;
        }

        this.isSubmittingClaim = true;
        const policy = this.policies.find(p => p.id === this.claimData.policyId);
        // Allow the submission to proceed to backend as coverage will be evaluated and possibly restored.
        // We no longer block claims where amount exceeds remaining coverage since we now cap eligible amounts.

        if (this.claimData.file) {
            this.apiService.uploadDocument(this.claimData.file).subscribe({
                next: (res) => this.executeRaiseClaim(res.url),
                error: (err) => {
                    alert(`Failed to upload document: ${err.message}`);
                    this.isSubmittingClaim = false;
                }
            });
        } else {
            this.executeRaiseClaim(null);
        }
    }

    private executeRaiseClaim(documentUrl: string | null) {
        this.apiService.raiseClaim({
            policyId: this.claimData.policyId,
            claimAmount: this.claimData.amount,
            reason: this.claimData.reason,
            documentUrl: documentUrl
        }).subscribe({
            next: (msg) => {
                this.showStatus(msg || '✓ Claim submitted successfully! It is now under review by the Claims Officer.', 'success');
                this.closeClaimModal();
                this.loadDashboardData();
            },
            error: (err) => {
                let errorMsg = 'An unknown error occurred';
                if (err.error) {
                    try {
                        const parsedError = typeof err.error === 'string' ? JSON.parse(err.error) : err.error;
                        errorMsg = parsedError.error || parsedError.message || err.error;
                    } catch (e) {
                        errorMsg = err.error; // Fallback to raw string if not JSON
                    }
                } else if (err.message) {
                    errorMsg = err.message;
                }
                this.showStatus(`Failed to submit claim: ${errorMsg}`, 'error');
                this.isSubmittingClaim = false;
            }
        });
    }

    downloadInvoice(url: string) {
        window.open('http://localhost:5241' + url, '_blank');
    }

    initChart() {
        const canvas = document.getElementById('claimsChart') as HTMLCanvasElement;
        if (!canvas) return;

        if (this.chartInstance) {
            this.chartInstance.destroy();
        }

        this.chartInstance = new Chart(canvas, {
            type: 'doughnut',
            data: {
                labels: ['Approved', 'Pending', 'Rejected'],
                datasets: [{
                    data: [this.approvedClaims, this.pendingClaimsCount, this.rejectedClaims],
                    backgroundColor: ['#4B2E83', '#A3A3A3', '#C0C0C0'],
                    borderWidth: 0,
                    hoverOffset: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '70%',
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: (item) => ` ${item.label}: ${item.raw}`
                        }
                    }
                }
            },
            plugins: [{
                id: 'centerText',
                beforeDraw: (chart) => {
                    const { width, height, ctx } = chart;
                    ctx.restore();
                    const fontSize = (height / 114).toFixed(2);
                    ctx.font = `bold ${fontSize}em Inter, sans-serif`;
                    ctx.textBaseline = 'middle';
                    ctx.fillStyle = '#201236';
                    const text = (this.approvedClaims + this.pendingClaimsCount + this.rejectedClaims).toString();
                    const textX = Math.round((width - ctx.measureText(text).width) / 2);
                    const textY = height / 2;
                    ctx.fillText(text, textX, textY);
                    ctx.save();
                }
            }]
        });
    }

    fetchUserName() {
        const token = localStorage.getItem('token');
        if (token) {
            try {
                const payload = JSON.parse(atob(token.split('.')[1]));
                this.userName = payload['unique_name'] || payload['name'] || 'Customer';
            } catch (e) {
                this.userName = 'Customer';
            }
        }
    }
}
