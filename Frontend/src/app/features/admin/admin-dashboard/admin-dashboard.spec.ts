import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { AdminDashboard } from './admin-dashboard';
import { ApiService } from '../../../core/services/api.service';
import { UiService } from '../../../core/services/ui.service';
import { ActivatedRoute, Router } from '@angular/router';
import { of, Subject } from 'rxjs';
import { CommonModule } from '@angular/common';

describe('AdminDashboard', () => {
    let component: AdminDashboard;
    let fixture: ComponentFixture<AdminDashboard>;
    let apiServiceSpy: jasmine.SpyObj<ApiService>;
    let uiServiceSpy: jasmine.SpyObj<UiService>;
    let routerSpy: jasmine.SpyObj<Router>;
    let openModalSubject = new Subject<string>();

    beforeEach(async () => {
        apiServiceSpy = jasmine.createSpyObj('ApiService', [
            'getAdminDashboard',
            'getAgentPerformance',
            'getOfficerPerformance',
            'getPlans',
            'getPlanPerformance',
            'getCustomerPerformance',
            'createAgent',
            'createOfficer'
        ]);
        uiServiceSpy = {
            openModal$: openModalSubject.asObservable()
        } as any;
        routerSpy = jasmine.createSpyObj('Router', ['navigate']);

        apiServiceSpy.getAdminDashboard.and.returnValue(of({
            totalCustomers: 10,
            totalPolicies: 5,
            totalAgents: 2,
            totalClaimsOfficers: 2,
            totalRevenue: 50000,
            pendingClaims: 3
        }));
        apiServiceSpy.getAgentPerformance.and.returnValue(of([]));
        apiServiceSpy.getOfficerPerformance.and.returnValue(of([]));
        apiServiceSpy.getPlans.and.returnValue(of([]));
        apiServiceSpy.getPlanPerformance.and.returnValue(of([]));
        apiServiceSpy.getCustomerPerformance.and.returnValue(of([]));

        await TestBed.configureTestingModule({
            imports: [AdminDashboard, CommonModule],
            providers: [
                { provide: ApiService, useValue: apiServiceSpy },
                { provide: UiService, useValue: uiServiceSpy },
                { provide: Router, useValue: routerSpy },
                {
                    provide: ActivatedRoute,
                    useValue: {
                        queryParams: of({ tab: 'agents' })
                    }
                }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminDashboard);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load dashboard data on init', () => {
        expect(apiServiceSpy.getAdminDashboard).toHaveBeenCalled();
        expect(component.overview.totalCustomers).toBe(10);
    });

    it('should switch tabs', () => {
        component.switchTab('plans');
        expect(routerSpy.navigate).toHaveBeenCalled();
        // Tab switching updates query params usually
    });

    it('should handle modal events from UiService', fakeAsync(() => {
        spyOn(component, 'openAddAgentModal');
        openModalSubject.next('agent');
        tick();
        expect(component.openAddAgentModal).toHaveBeenCalled();
    }));

    it('should open and close add agent modal', () => {
        component.openAddAgentModal();
        expect(component.showAddAgentModal).toBeTrue();
        component.closeModals();
        expect(component.showAddAgentModal).toBeFalse();
    });

    it('should call createAgent on addAgent with valid data', () => {
        component.newStaff = {
            fullName: 'Test Agent',
            email: 'agent@test.com',
            password: 'password123',
            phoneNumber: '9876543210'
        };
        apiServiceSpy.createAgent.and.returnValue(of('Created' as any));

        component.addAgent();

        expect(apiServiceSpy.createAgent).toHaveBeenCalled();
    });
});
