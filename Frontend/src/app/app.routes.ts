import { Routes } from '@angular/router';
import { customerOnlyGuard } from './core/guards/customer-only.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () => import('./features/home/home').then(m => m.Home)
    },
    {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login').then(m => m.Login)
    },
    {
        path: 'forgot-password',
        loadComponent: () => import('./features/auth/forgot-password/forgot-password').then(m => m.ForgotPassword)
    },
    {
        path: 'register',
        canActivate: [customerOnlyGuard],
        loadComponent: () => import('./features/auth/register/register').then(m => m.Register)
    },
    {
        path: 'customer/dashboard',
        canActivate: [roleGuard(['Customer'])],
        loadComponent: () => import('./features/customer/customer-dashboard/customer-dashboard').then(m => m.CustomerDashboard)
    },
    {
        path: 'customer/claims',
        canActivate: [roleGuard(['Customer'])],
        loadComponent: () => import('./features/customer/customer-claims/customer-claims').then(m => m.CustomerClaims)
    },
    {
        path: 'customer/purchase-policy',
        canActivate: [roleGuard(['Customer'])],
        loadComponent: () => import('./features/customer/policy-wizard/policy-wizard').then(m => m.PolicyWizard)
    },
    {
        path: 'admin/add-plan',
        canActivate: [roleGuard(['Admin'])],
        loadComponent: () => import('./features/admin/add-plan/add-plan.component').then(m => m.AddPlanComponent)
    },
    {
        path: 'admin/edit-plan',
        canActivate: [roleGuard(['Admin'])],
        loadComponent: () => import('./features/admin/edit-plan/edit-plan.component').then(m => m.EditPlanComponent)
    },
    {
        path: 'admin/dashboard',
        canActivate: [roleGuard(['Admin'])],
        loadComponent: () => import('./features/admin/admin-dashboard/admin-dashboard').then(m => m.AdminDashboard)
    },
    {
        path: 'agent/dashboard',
        canActivate: [roleGuard(['Agent'])],
        loadComponent: () => import('./features/agent/agent-dashboard/agent-dashboard').then(m => m.AgentDashboard)
    },
    {
        path: 'claims-officer/dashboard',
        canActivate: [roleGuard(['ClaimsOfficer'])],
        loadComponent: () => import('./features/claims-officer/claims-dashboard/claims-dashboard').then(m => m.ClaimsDashboard)
    },
    {
        path: 'error',
        loadComponent: () => import('./features/error/error-page').then(m => m.ErrorPage)
    },
    {
        path: 'plans',
        loadComponent: () => import('./features/customer/plans/plans').then(m => m.Plans)
    },
    {
        path: '**',
        redirectTo: 'error'
    }
];
