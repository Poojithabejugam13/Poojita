import { Component, OnInit, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';
import { UiService } from '../../../core/services/ui.service';
import { Chart, registerables } from 'chart.js';
import { Subscription } from 'rxjs';

Chart.register(...registerables);

@Component({
    selector: 'app-admin-dashboard',
    standalone: true,
    imports: [CommonModule, FormsModule, CurrencyPipe],
    templateUrl: './admin-dashboard.html'
})
export class AdminDashboard implements OnInit, AfterViewInit {
    overview = {
        totalCustomers: 0,
        totalPolicies: 0,
        totalAgents: 0,
        totalClaimsOfficers: 0,
        totalRevenue: 0,
        pendingClaims: 0
    };

    activeTab: 'agents' | 'officers' | 'plans' | 'customers' = 'agents';

    agentPerformance: any[] = [];
    officerPerformance: any[] = [];
    customerPerformance: any[] = [];
    plans: any[] = [];
    planPerformance: any[] = [];
    categoryPerformance: any[] = []; // Grouped by planType

    // Drill-down & Explanation State
    showChartExplanation = false;
    explanationTitle = '';
    explanationText = '';

    showPlanDrilldown = false;
    selectedCategoryName = '';
    drilldownPlans: any[] = [];

    loading = true;
    errorMessage = '';
    successMessage = '';

    // Staff Management State
    showAddAgentModal = false;
    showAddOfficerModal = false;
    processingStaff = false;
    newStaff = { fullName: '', email: '', password: '', phoneNumber: '' };
    validationErrors: any = {};

    // Modal States for View All
    showAllAgentsModal = false;
    showAllOfficersModal = false;
    showAllCustomersModal = false;

    // Canvas references for charts
    @ViewChild('agentChart') agentChartCanvas!: ElementRef;
    @ViewChild('officerChart') officerChartCanvas!: ElementRef;
    @ViewChild('planChart') planChartCanvas!: ElementRef;
    @ViewChild('customerChart') customerChartCanvas!: ElementRef;

    // Chart Instances
    private agentChartInstance: any;
    private officerChartInstance: any;
    private planChartInstance: any;
    private customerChartInstance: any;
    private modalSubscription?: Subscription;

    constructor(
        private apiService: ApiService,
        private uiService: UiService,
        private route: ActivatedRoute,
        private router: Router
    ) { }

    ngOnInit() {
        this.route.queryParams.subscribe(params => {
            const tab = params['tab'];
            if (['agents', 'officers', 'plans', 'customers'].includes(tab)) {
                this.activeTab = tab;
            }

            const open = params['open'];
            if (open === 'agent') {
                this.openAddAgentModal();
            } else if (open === 'officer') {
                this.openAddOfficerModal();
            }

            // Once handled, remove the `open` parameter so modals don't auto-open again
            if (open) {
                this.router.navigate([], {
                    relativeTo: this.route,
                    queryParams: { open: null },
                    queryParamsHandling: 'merge'
                });
            }
        });

        this.loadDashboardData();
        this.modalSubscription = this.uiService.openModal$.subscribe(modal => {
            if (modal === 'agent') this.openAddAgentModal();
            if (modal === 'officer') this.openAddOfficerModal();
        });
    }

    ngOnDestroy() {
        if (this.modalSubscription) {
            this.modalSubscription.unsubscribe();
        }
    }

    ngAfterViewInit() {
        // Charts will be rendered once data is loaded and tab is active
    }

    loadDashboardData() {
        this.loading = true;
        this.apiService.getAdminDashboard().subscribe({
            next: (data) => {
                this.overview = data;
                this.loadPerformanceMetrics();
            },
            error: (err) => {
                this.errorMessage = 'Failed to load dashboard data.';
                this.loading = false;
            }
        });
    }

    loadPerformanceMetrics() {
        let completedRequests = 0;
        const totalRequests = 5; // agent, officer, plans, planPerformance, customerPerformance

        const checkLoadingComplete = () => {
            completedRequests++;
            if (completedRequests === totalRequests) {
                this.loading = false;
                setTimeout(() => {
                    this.renderActiveChart();
                    this.renderCustomerChart(); // Render customer chart as well
                }, 100);
            }
        };

        this.apiService.getAgentPerformance().subscribe({
            next: (data) => { this.agentPerformance = data; checkLoadingComplete(); },
            error: (err) => { console.error('Failed to load agent performance', err); checkLoadingComplete(); }
        });
        this.apiService.getOfficerPerformance().subscribe({
            next: (data) => { this.officerPerformance = data; checkLoadingComplete(); },
            error: (err) => { console.error('Failed to load officer performance', err); checkLoadingComplete(); }
        });
        this.apiService.getPlans().subscribe({
            next: (data) => { this.plans = data; checkLoadingComplete(); },
            error: (err) => { console.error('Failed to load plans', err); checkLoadingComplete(); }
        });
        this.apiService.getPlanPerformance().subscribe({
            next: (data) => {
                this.planPerformance = data;
                this.groupPlansByType();
                checkLoadingComplete();
            },
            error: (err) => { console.error('Failed to load plan performance', err); checkLoadingComplete(); }
        });
        this.apiService.getCustomerPerformance().subscribe({
            next: (data: any) => { this.customerPerformance = data; checkLoadingComplete(); },
            error: (err: any) => { console.error('Failed to load customer performance', err); checkLoadingComplete(); }
        });
    }

    groupPlansByType() {
        const types = ['Individual', 'Family', 'Senior Citizen', 'Specialized Care'];
        const grouped = new Map<string, any>();

        types.forEach(type => grouped.set(type, { planType: type, totalRevenue: 0, policiesSold: 0, plans: [] }));

        this.planPerformance.forEach(p => {
            const type = p.planType || 'Individual';

            if (!grouped.has(type)) {
                grouped.set(type, { planType: type, totalRevenue: 0, policiesSold: 0, plans: [] });
            }

            const entry = grouped.get(type);
            if (entry) {
                entry.totalRevenue += p.totalRevenueGenerated;
                entry.policiesSold += p.totalPoliciesSold;
                entry.plans.push(p);
            }
        });

        this.categoryPerformance = Array.from(grouped.values()).filter(c => c.totalRevenue > 0 || c.policiesSold > 0);
    }

    switchTab(tab: 'agents' | 'officers' | 'plans' | 'customers') {
        this.router.navigate([], {
            relativeTo: this.route,
            queryParams: { tab: tab },
            queryParamsHandling: 'merge'
        });
        setTimeout(() => this.renderActiveChart(), 50);
    }

    renderActiveChart() {
        if (this.activeTab === 'agents' && this.agentChartCanvas) {
            this.renderAgentChart();
        } else if (this.activeTab === 'officers' && this.officerChartCanvas) {
            this.renderOfficerChart();
        } else if (this.activeTab === 'plans' && this.planChartCanvas) {
            this.renderPlanChart();
        } else if (this.activeTab === 'customers' && this.customerChartCanvas) {
            this.renderCustomerChart();
        }
    }

    renderAgentChart() {
        if (this.agentChartInstance) {
            this.agentChartInstance.destroy();
        }

        // Take top 5 agents by commission
        const topAgents = [...this.agentPerformance]
            .sort((a, b) => b.totalCommissionEarned - a.totalCommissionEarned)
            .slice(0, 5);

        this.agentChartInstance = new Chart(this.agentChartCanvas.nativeElement, {
            type: 'bar',
            data: {
                labels: topAgents.map(a => a.agentName),
                datasets: [{
                    label: 'Commission (₹)',
                    data: topAgents.map(a => a.totalCommissionEarned),
                    backgroundColor: 'rgba(79, 70, 229, 0.8)',
                    borderColor: 'rgba(79, 70, 229, 1)',
                    borderWidth: 2,
                    borderRadius: 12,
                    borderSkipped: false,
                    barThickness: 40
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                onClick: () => {
                    this.showExplanation('Agent Performance', 'This chart ranks the top 5 insurance agents based on the total commission they have earned. The X-axis represents individual agent names, and the Y-axis shows their total earnings in Rupees (₹).');
                },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: 'rgba(255, 255, 255, 0.95)',
                        titleColor: '#1e1b4b',
                        bodyColor: '#4338ca',
                        borderColor: '#e5e7eb',
                        borderWidth: 1,
                        padding: 12,
                        displayColors: false,
                        callbacks: {
                            label: (context: any) => `Earnings: ₹${context.raw.toLocaleString()} `
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        title: { display: true, text: 'Total Commission Earned (₹)', font: { weight: 'bold' } },
                        grid: { display: false },
                        ticks: { font: { weight: 'bold' } }
                    },
                    x: {
                        title: { display: true, text: 'Top Performing Agents', font: { weight: 'bold' } },
                        grid: { display: false },
                        ticks: { font: { weight: 'bold' } }
                    }
                }
            }
        });
    }

    renderOfficerChart() {
        if (this.officerChartInstance) {
            this.officerChartInstance.destroy();
        }

        let totalApproved = 0;
        let totalRejected = 0;
        this.officerPerformance.forEach(o => {
            totalApproved += o.approvedClaims;
            totalRejected += o.rejectedClaims;
        });

        this.officerChartInstance = new Chart(this.officerChartCanvas.nativeElement, {
            type: 'doughnut',
            data: {
                labels: ['Approved Claims', 'Rejected Claims'],
                datasets: [{
                    data: [totalApproved, totalRejected],
                    backgroundColor: [
                        'rgba(34, 197, 94, 0.7)', // Green
                        'rgba(239, 68, 68, 0.7)'  // Red
                    ],
                    borderColor: [
                        'rgba(34, 197, 94, 1)',
                        'rgba(239, 68, 68, 1)'
                    ],
                    borderWidth: 2,
                    hoverOffset: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                onClick: () => {
                    this.showExplanation('Claims Resolution', 'This pie chart visualizes the balance between approved and rejected insurance claims across the entire platform. Green indicates successfully processed claims, while Red shows rejected ones.');
                },
                plugins: {
                    title: { display: true, text: 'Platform-Wide Resolution Ratio', font: { size: 16 } }
                }
            }
        });
    }

    renderPlanChart() {
        if (this.planChartInstance) {
            this.planChartInstance.destroy();
        }

        const labels = this.categoryPerformance.map(c => c.planType);
        const revenueData = this.categoryPerformance.map(c => c.totalRevenue);
        const policiesData = this.categoryPerformance.map(c => c.policiesSold);

        this.planChartInstance = new Chart(this.planChartCanvas.nativeElement, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Total Revenue (₹)',
                        data: revenueData,
                        backgroundColor: 'rgba(99, 102, 241, 0.8)',
                        borderRadius: 8,
                        xAxisID: 'x'
                    },
                    {
                        label: 'Policies Sold',
                        data: policiesData,
                        backgroundColor: 'rgba(236, 72, 153, 0.8)',
                        borderRadius: 8,
                        xAxisID: 'x1'
                    }
                ]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                onClick: (event, elements) => {
                    if (elements.length > 0) {
                        const index = elements[0].index;
                        this.openPlanDrilldown(this.categoryPerformance[index]);
                    } else {
                        this.showExplanation('Plans Overview', 'This chart shows how different insurance plan types are performing. The bottom axis represents total revenue earned, and the top axis shows the number of policies sold in each type.');
                    }
                },
                plugins: {
                    legend: { position: 'top', labels: { font: { weight: 'bold' } } },
                    tooltip: {
                        padding: 12,
                        backgroundColor: 'rgba(255, 255, 255, 0.95)',
                        titleColor: '#1e1b4b',
                        bodyColor: '#334155',
                        borderColor: 'rgba(99, 102, 241, 0.1)',
                        borderWidth: 1
                    }
                },
                scales: {
                    x: {
                        display: true,
                        position: 'bottom',
                        title: { display: true, text: 'Total Revenue Generated (₹)', font: { weight: 'bold' }, color: '#4f46e5' },
                        grid: { color: 'rgba(0,0,0,0.05)' }
                    },
                    x1: {
                        display: true,
                        position: 'top',
                        title: { display: true, text: 'Total Policies Sold (Count)', font: { weight: 'bold' }, color: '#db2777' },
                        grid: { drawOnChartArea: false }
                    },
                    y: {
                        title: { display: true, text: 'Insurance Plan Types', font: { weight: 'bold' } },
                        grid: { display: false },
                        ticks: { font: { weight: 'bold' } }
                    }
                }
            }
        });
    }

    openPlanDrilldown(typeData: any) {
        this.selectedCategoryName = typeData.planType;
        this.drilldownPlans = typeData.plans;
        this.showPlanDrilldown = true;
    }

    closeDrilldown() {
        this.showPlanDrilldown = false;
    }

    showExplanation(title: string, text: string) {
        this.explanationTitle = title;
        this.explanationText = text;
        this.showChartExplanation = true;
    }

    closeExplanation() {
        this.showChartExplanation = false;
    }

    renderCustomerChart() {
        if (this.customerChartInstance) {
            this.customerChartInstance.destroy();
        }

        if (!this.customerChartCanvas?.nativeElement) return;

        const ctx = this.customerChartCanvas.nativeElement.getContext('2d');

        // Only show top 10 customers by active policies to keep it clean
        const topCustomers = [...this.customerPerformance]
            .sort((a, b) => b.activePolicies - a.activePolicies)
            .slice(0, 10);

        this.customerChartInstance = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: topCustomers.map(c => c.customerName || 'Unknown'),
                datasets: [
                    {
                        label: 'Active Policies',
                        data: topCustomers.map(c => c.activePolicies),
                        backgroundColor: 'rgba(56, 189, 248, 0.8)',
                        borderRadius: 6
                    },
                    {
                        label: 'Total Claims',
                        data: topCustomers.map(c => c.totalClaims),
                        backgroundColor: 'rgba(239, 68, 68, 0.8)',
                        borderRadius: 6
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                onClick: () => {
                    this.showExplanation('Customer Activity', 'This chart reveals the engagement levels of your top 10 customers. The Blue bars show how many active policies they hold, while the Red bars indicate their claim history. X-axis shows customer names, Y-axis shows the count of policies/claims.');
                },
                plugins: {
                    legend: { position: 'top' }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        title: { display: true, text: 'Number of Policies / Claims', font: { weight: 'bold' } },
                        ticks: { stepSize: 1 }
                    },
                    x: {
                        title: { display: true, text: 'Top 10 Customers', font: { weight: 'bold' } },
                        grid: { display: false }
                    }
                }
            }
        });
    }

    // Staff Management Methods
    openAddAgentModal() {
        this.newStaff = { fullName: '', email: '', password: '', phoneNumber: '' };
        this.showAddAgentModal = true;
    }

    openAddOfficerModal() {
        this.newStaff = { fullName: '', email: '', password: '', phoneNumber: '' };
        this.showAddOfficerModal = true;
    }

    closeModals() {
        this.showAddAgentModal = false;
        this.showAddOfficerModal = false;
        this.validationErrors = {};
        this.errorMessage = '';
        this.successMessage = '';
    }

    validateStaffForm(): boolean {
        this.validationErrors = {};
        let isValid = true;

        if (!this.newStaff.fullName || this.newStaff.fullName.trim().length < 3) {
            this.validationErrors.fullName = 'Full name must be at least 3 characters.';
            isValid = false;
        }

        const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
        if (!this.newStaff.email || !emailPattern.test(this.newStaff.email)) {
            this.validationErrors.email = 'Please enter a valid email address.';
            isValid = false;
        }

        const phonePattern = /^\d{10}$/;
        if (!this.newStaff.phoneNumber || !phonePattern.test(this.newStaff.phoneNumber)) {
            this.validationErrors.phoneNumber = 'Phone number must be exactly 10 digits.';
            isValid = false;
        }

        if (!this.newStaff.password || this.newStaff.password.length < 6) {
            this.validationErrors.password = 'Password must be at least 6 characters.';
            isValid = false;
        }

        return isValid;
    }

    addAgent() {
        if (!this.validateStaffForm()) return;
        this.processingStaff = true;

        const payload = {
            FullName: this.newStaff.fullName,
            Email: this.newStaff.email,
            Password: this.newStaff.password,
            PhoneNumber: this.newStaff.phoneNumber
        };

        this.apiService.createAgent(payload).subscribe({
            next: () => {
                this.successMessage = 'Agent added successfully!';
                this.processingStaff = false;
                setTimeout(() => { this.closeModals(); this.loadDashboardData(); }, 1500);
            },
            error: () => {
                this.errorMessage = 'Failed to add agent.';
                this.processingStaff = false;
            }
        });
    }

    deleteAgent(id: number) {
        if (!confirm('Are you sure you want to remove this agent?')) return;
        this.apiService.deleteAgent(id).subscribe({
            next: () => {
                this.loadDashboardData();
            },
            error: () => alert('Failed to delete agent.')
        });
    }

    addOfficer() {
        if (!this.validateStaffForm()) return;
        this.processingStaff = true;

        const payload = {
            FullName: this.newStaff.fullName,
            Email: this.newStaff.email,
            Password: this.newStaff.password,
            PhoneNumber: this.newStaff.phoneNumber
        };

        this.apiService.createOfficer(payload).subscribe({
            next: () => {
                this.successMessage = 'Claims Officer added successfully!';
                this.processingStaff = false;
                setTimeout(() => { this.closeModals(); this.loadDashboardData(); }, 1500);
            },
            error: () => {
                this.errorMessage = 'Failed to add officer.';
                this.processingStaff = false;
            }
        });
    }

    deleteOfficer(id: number) {
        if (!confirm('Are you sure you want to remove this officer?')) return;
        this.apiService.deleteOfficer(id).subscribe({
            next: () => {
                this.loadDashboardData();
            },
            error: (err) => {
                const msg = err?.error?.error || err?.error?.message || 'Failed to delete officer.';
                alert(msg);
            }
        });
    }

    // Modal Methods for View All
    openAllAgents() { this.showAllAgentsModal = true; }
    openAllOfficers() { this.showAllOfficersModal = true; }
    openAllCustomers() { this.showAllCustomersModal = true; }
    closeListModals() {
        this.showAllAgentsModal = false;
        this.showAllOfficersModal = false;
        this.showAllCustomersModal = false;
    }
}
