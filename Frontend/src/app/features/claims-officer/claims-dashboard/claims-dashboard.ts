import { Component, OnInit } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { ApiService } from '../../../core/services/api.service';

export interface Claim {
    claimId: number;
    policyId: number;
    claimAmount: number;
    reason: string;        // matches ClaimDto.Reason (camelCase JSON: "reason")
    documentUrl?: string;
    status: number;
    createdAt: string;
    policyNumber?: string;
    planName?: string;
    planDescription?: string;
    isPedWaitingViolated?: boolean;
    approvalReason?: string;
    rejectionReason?: string;
}

@Component({
    selector: 'app-claims-dashboard',
    standalone: true,
    imports: [CommonModule, CurrencyPipe, DatePipe],
    templateUrl: './claims-dashboard.html'
})
export class ClaimsDashboard implements OnInit {
    pendingClaims = 0;
    approvedToday = 0;
    claims: Claim[] = [];
    myCustomers: any[] = [];
    activeTab: 'pending' | 'customers' = 'pending';

    constructor(private apiService: ApiService) { }

    ngOnInit() {
        this.loadDashboardData();
        this.loadPendingClaims();
        this.loadMyCustomers();
    }

    loadDashboardData() {
        this.apiService.getClaimsDashboard().subscribe({
            next: (data: any) => {
                this.pendingClaims = data.pendingClaims;
                this.approvedToday = data.approvedClaims; // Adjust to what dashboard returns
            },
            error: (err: any) => console.error('Failed to load dashboard metrics', err)
        });
    }

    loadPendingClaims() {
        this.apiService.getPendingClaims().subscribe({
            next: (data: any) => {
                this.claims = data;
                this.pendingClaims = this.claims.length;
            },
            error: (err: any) => console.error('Failed to load pending claims', err)
        });
    }

    approveClaim(claim: Claim) {
        let reason: string | null = null;

        if (claim.isPedWaitingViolated) {
            const confirmed = confirm(`WARNING: This claim is filed during the policy's Waiting Period.\n\nPlease verify medical documents to ensure this is NOT a pre-existing condition before proceeding.\n\nAre you sure you want to approve?`);
            if (!confirmed) return;

            const justification = prompt("Optional: Provide a justification for manual approval (e.g., verified documentation - unrelated illness):");
            reason = justification || null;
        }

        this.apiService.approveClaim(claim.claimId, reason || undefined).subscribe({
            next: () => {
                claim.status = 1; // 1 = Approved
                this.pendingClaims--;
                this.approvedToday++;
                alert(`Claim #${claim.claimId} has been approved.`);
                this.loadPendingClaims();
            },
            error: (err: any) => {
                const msg = err.error?.message || err.message || 'Failed to approve claim';
                alert('Cannot approve: ' + msg);
            }
        });
    }

    rejectClaim(claim: Claim) {
        const reason = prompt("Please provide a reason for rejecting this claim:");
        if (reason === null || reason.trim() === '') {
            alert("Rejection cancelled. A reason is required to reject a claim.");
            return;
        }

        this.apiService.rejectClaim(claim.claimId, reason).subscribe({
            next: () => {
                claim.status = 2; // 2 = Rejected
                this.pendingClaims--;
                alert(`Claim #${claim.claimId} has been rejected. Reason logged: ${reason}`);
                this.loadPendingClaims();
            },
            error: (err: any) => alert('Failed to reject claim: ' + (err.error?.message || err.message))
        });
    }

    loadMyCustomers() {
        this.apiService.getClaimsOfficerCustomers().subscribe({
            next: (data: any[]) => {
                this.myCustomers = data.map(pol => {
                    let _pStatus = 'Pending Approval';
                    if (pol.status === 1) _pStatus = 'Approved';
                    else if (pol.status === 2) _pStatus = 'Rejected';
                    else if (pol.status === 3) _pStatus = 'Active';
                    else if (pol.status === 4) _pStatus = 'Expired';
                    else if (pol.status === 5) _pStatus = 'Cancelled';

                    return {
                        ...pol,
                        policyStatus: _pStatus,
                        latestClaimStatus: pol.latestClaimStatus || 'No Claims'
                    };
                });
            },
            error: (err: any) => console.error('Failed to load customers', err)
        });
    }

    setTab(tab: 'pending' | 'customers') {
        this.activeTab = tab;
    }
}
