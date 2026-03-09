import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class ApiService {
    private apiUrl = 'http://localhost:5241/api';
    private http = inject(HttpClient);

    // Auth
    login(credentials: { email: string; password: string }): Observable<any> {
        return this.http.post(`${this.apiUrl}/auth/login`, credentials);
    }

    registerCustomer(userData: any): Observable<any> {
        return this.http.post(`${this.apiUrl}/auth/register`, userData, { responseType: 'text' });
    }

    resetPassword(data: { email: string; phoneNumber: string; newPassword: string }): Observable<any> {
        return this.http.post(`${this.apiUrl}/auth/reset-password`, data, { responseType: 'text' });
    }

    // Agent
    getAgentDashboard(): Observable<any> {
        return this.http.get(`${this.apiUrl}/agent/dashboard`);
    }

    getPendingPolicies(): Observable<any> {
        return this.http.get(`${this.apiUrl}/agent/pending-requests`);
    }

    approvePolicy(policyId: number, reason: string): Observable<any> {
        return this.http.put(`${this.apiUrl}/agent/${policyId}/approve`, { reason });
    }

    rejectPolicy(policyId: number, reason: string): Observable<any> {
        return this.http.put(`${this.apiUrl}/agent/${policyId}/reject`, { reason });
    }

    expirePolicy(policyId: number): Observable<any> {
        return this.http.put(`${this.apiUrl}/agent/${policyId}/expire`, {});
    }

    cancelPolicy(policyId: number): Observable<any> {
        return this.http.put(`${this.apiUrl}/agent/${policyId}/cancel`, {});
    }

    // Claims Officer
    getClaimsDashboard(): Observable<any> {
        return this.http.get(`${this.apiUrl}/claimsofficer/dashboard`);
    }

    getPendingClaims(): Observable<any> {
        return this.http.get(`${this.apiUrl}/claimsofficer/pending`);
    }

    approveClaim(claimId: number, reason?: string): Observable<any> {
        return this.http.put(`${this.apiUrl}/claimsofficer/${claimId}/approve`, { reason });
    }

    rejectClaim(claimId: number, reason: string): Observable<any> {
        return this.http.put(`${this.apiUrl}/claimsofficer/${claimId}/reject`, { reason });
    }

    // Customer Endpoints
    getCustomerDashboard(): Observable<any> {
        return this.http.get(`${this.apiUrl}/customer/dashboard`);
    }

    getPlans(): Observable<any> {
        return this.http.get(`${this.apiUrl}/customer/plans`);
    }

    requestPolicy(request: any): Observable<any> {
        return this.http.post(`${this.apiUrl}/customer/request-policy`, request, { responseType: 'text' });
    }

    uploadDocument(file: File): Observable<{ url: string }> {
        const formData = new FormData();
        formData.append('file', file);
        return this.http.post<{ url: string }>(`${this.apiUrl}/customer/upload-document`, formData);
    }

    makePayment(payload: any): Observable<any> {
        return this.http.post(`${this.apiUrl}/customer/make-payment`, payload);
    }

    getMyPolicies(): Observable<any> {
        return this.http.get(`${this.apiUrl}/customer/policies`);
    }

    getPolicyById(id: number): Observable<any> {
        return this.http.get(`${this.apiUrl}/customer/policies/${id}`);
    }

    raiseClaim(request: any): Observable<any> {
        return this.http.post(`${this.apiUrl}/customer/raise-claim`, request, { responseType: 'text' });
    }

    getMyClaims(): Observable<any> {
        return this.http.get(`${this.apiUrl}/customer/claims`);
    }

    renewPolicy(policyId: number): Observable<any> {
        return this.http.post(`${this.apiUrl}/customer/renew-policy/${policyId}`, {}, { responseType: 'text' });
    }

    // Agent Endpoints
    getAgentCustomers(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/agent/my-customers`);
    }

    updatePolicyStatus(policyId: number, status: number): Observable<any> {
        return this.http.put(`${this.apiUrl}/agent/${policyId}/status`, status);
    }

    // Claims Officer Endpoints
    getClaimsOfficerCustomers(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/claimsofficer/my-customers`);
    }

    // Admin Endpoints
    getAdminDashboard(): Observable<any> {
        return this.http.get(`${this.apiUrl}/admin/dashboard`);
    }

    getAgentPerformance(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/admin/agent-performance`);
    }

    getOfficerPerformance(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/admin/officer-performance`);
    }

    getPlanPerformance(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/admin/plan-performance`);
    }

    getCustomerPerformance(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/admin/customer-performance`);
    }

    createAgent(data: any): Observable<string> {
        return this.http.post(`${this.apiUrl}/admin/agents`, data, { responseType: 'text' });
    }

    deleteAgent(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/admin/agents/${id}`);
    }

    createOfficer(data: any): Observable<string> {
        return this.http.post(`${this.apiUrl}/admin/claimsofficers`, data, { responseType: 'text' });
    }

    deleteOfficer(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/admin/claimsofficers/${id}`);
    }

    createPlan(data: any): Observable<any> {
        return this.http.post(`${this.apiUrl}/admin/plans`, data);
    }

    createTier(data: any): Observable<any> {
        return this.http.post(`${this.apiUrl}/admin/tiers`, data, { responseType: 'text' });
    }

    updatePlan(planId: number, data: any): Observable<any> {
        return this.http.put(`${this.apiUrl}/admin/plans/${planId}`, data, { responseType: 'text' });
    }
}
