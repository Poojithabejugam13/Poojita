import { Component, OnInit } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';

export interface CustomerPolicy {
    policyId: number;
    policyNumber: string;
    customerName: string;
    planName: string;
    planDescription?: string;
    decisionReason?: string;
    totalPremium: number;
    commissionAmount: number;
    status: number;
    createdAt: string;
    documentUrl?: string;
    policyCategory: string;
    policyStartDate: string;
    fullName: string;
    dateOfBirth: string;
    gender: string;
    mobileNumber: string;
    address: string;
    heightCm: number;
    weightKg: number;
    isSmoker: boolean;
    nominees?: any[];
    showDetails?: boolean;
}

@Component({
    selector: 'app-agent-dashboard',
    standalone: true,
    imports: [CommonModule, CurrencyPipe, DatePipe, FormsModule],
    templateUrl: './agent-dashboard.html'
})
export class AgentDashboard implements OnInit {
    pendingApprovals = 0;
    totalCommission = 0;
    activeClients = 0;

    requests: CustomerPolicy[] = [];
    myCustomers: any[] = [];
    activeTab: 'pending' | 'customers' = 'pending';

    // Inline colored feedback
    actionMessage = '';
    actionType: 'success' | 'error' | 'warning' = 'success';

    // Approve modal state
    showApproveModal = false;
    approveReason = '';
    pendingApproveReq: CustomerPolicy | null = null;

    // Reject modal state
    showRejectModal = false;
    rejectReason = '';
    pendingRejectReq: CustomerPolicy | null = null;

    constructor(private apiService: ApiService) { }

    ngOnInit() {
        this.loadDashboardData();
        this.loadPendingRequests();
        this.loadMyCustomers();
    }

    showStatus(msg: string, type: 'success' | 'error' | 'warning' = 'success') {
        this.actionMessage = msg;
        this.actionType = type;
        setTimeout(() => this.actionMessage = '', 5000);
    }

    loadDashboardData() {
        this.apiService.getAgentDashboard().subscribe({
            next: (data) => {
                this.totalCommission = data.totalCommissionEarned;
                this.activeClients = data.policiesSold;
            },
            error: () => this.showStatus('Failed to load dashboard metrics', 'error')
        });
    }

    loadPendingRequests() {
        this.apiService.getPendingPolicies().subscribe({
            next: (data: CustomerPolicy[]) => {
                this.requests = data.map(req => ({ ...req, showDetails: false }));
                this.pendingApprovals = this.requests.length;
            },
            error: () => this.showStatus('Failed to load pending requests', 'error')
        });
    }

    toggleDetails(req: CustomerPolicy) {
        req.showDetails = !req.showDetails;
    }

    approvePolicy(req: CustomerPolicy) {
        this.pendingApproveReq = req;
        this.approveReason = '';
        this.showApproveModal = true;
    }

    closeApproveModal() {
        this.showApproveModal = false;
        this.pendingApproveReq = null;
        this.approveReason = '';
    }

    confirmApprove() {
        const req = this.pendingApproveReq!;
        const reason = this.approveReason;
        this.apiService.approvePolicy(req.policyId, reason).subscribe({
            next: () => {
                req.status = 1;
                this.pendingApprovals--;
                this.activeClients++;
                this.closeApproveModal();
                this.showStatus(`✓ Policy POL-${req.policyId} approved. Reason: ${reason}`, 'success');
                this.loadPendingRequests();
                this.loadMyCustomers();
            },
            error: (err: any) => {
                this.closeApproveModal();
                this.showStatus('Failed to approve: ' + (err.error || err.message), 'error');
            }
        });
    }

    rejectPolicy(req: CustomerPolicy) {
        this.pendingRejectReq = req;
        this.rejectReason = '';
        this.showRejectModal = true;
    }

    closeRejectModal() {
        this.showRejectModal = false;
        this.pendingRejectReq = null;
        this.rejectReason = '';
    }

    confirmReject() {
        if (!this.rejectReason.trim()) {
            this.showStatus('A reason is required to reject a policy.', 'warning');
            return;
        }
        const req = this.pendingRejectReq!;
        const reason = this.rejectReason;
        this.apiService.rejectPolicy(req.policyId, reason).subscribe({
            next: () => {
                req.status = 2;
                this.pendingApprovals--;
                this.closeRejectModal();
                this.showStatus(`✕ Policy POL-${req.policyId} rejected. Reason: ${reason}`, 'warning');
                this.loadPendingRequests();
                this.loadMyCustomers();
            },
            error: (err: any) => {
                this.closeRejectModal();
                this.showStatus('Failed to reject: ' + (err.error || err.message), 'error');
            }
        });
    }

    loadMyCustomers() {
        this.apiService.getAgentCustomers().subscribe({
            next: (data: any[]) => {
                this.myCustomers = data.map(pol => {
                    let _status = 'Pending Approval';
                    const s = Number(pol.status);
                    if (s === 1) _status = 'Approved';
                    else if (s === 2) _status = 'Rejected';
                    else if (s === 3) _status = 'Active';
                    else if (s === 4) _status = 'Expired';
                    else if (s === 5) _status = 'Cancelled';
                    return { ...pol, status: _status, rawStatus: s };
                });
            },
            error: () => this.showStatus('Failed to load customers', 'error')
        });
    }

    setTab(tab: 'pending' | 'customers') {
        this.activeTab = tab;
        this.actionMessage = '';
    }

    cancelPolicy(cust: any) {
        if (!confirm(`Cancel policy POL-${cust.policyId} for ${cust.customerName}?`)) return;
        this.apiService.cancelPolicy(cust.policyId).subscribe({
            next: () => {
                cust.status = 'Cancelled';
                cust.rawStatus = 5;
                this.showStatus(`Policy POL-${cust.policyId} has been CANCELLED.`, 'warning');
            },
            error: (err: any) => this.showStatus('Failed to cancel: ' + (err.error || err.message), 'error')
        });
    }

    expirePolicy(cust: any) {
        if (!confirm(`Mark policy POL-${cust.policyId} as EXPIRED?`)) return;
        this.apiService.expirePolicy(cust.policyId).subscribe({
            next: () => {
                cust.status = 'Expired';
                cust.rawStatus = 4;
                this.showStatus(`Policy POL-${cust.policyId} marked EXPIRED. Customer will be prompted to renew.`, 'warning');
            },
            error: (err: any) => this.showStatus('Failed to expire: ' + (err.error || err.message), 'error')
        });
    }
}
