import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UiService } from '../services/ui.service';

@Component({
    selector: 'app-sidebar',
    standalone: true,
    imports: [CommonModule, RouterLink, RouterLinkActive],
    templateUrl: './sidebar.html'
})
export class SidebarComponent {
    authService = inject(AuthService);
    uiService = inject(UiService);
    private router = inject(Router);

    openAgentModal() {
        if (this.router.url.startsWith('/admin/dashboard')) {
            this.uiService.triggerModal('agent');
        } else {
            this.router.navigate(['/admin/dashboard'], { queryParams: { open: 'agent' } });
        }
    }

    openOfficerModal() {
        if (this.router.url.startsWith('/admin/dashboard')) {
            this.uiService.triggerModal('officer');
        } else {
            this.router.navigate(['/admin/dashboard'], { queryParams: { open: 'officer' } });
        }
    }
}
