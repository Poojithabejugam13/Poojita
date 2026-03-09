import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';

@Component({
    selector: 'app-customer-claims',
    standalone: true,
    imports: [CommonModule, RouterLink, CurrencyPipe, DatePipe],
    templateUrl: './customer-claims.html'
})
export class CustomerClaims implements OnInit {
    private apiService = inject(ApiService);

    claims: any[] = [];
    loading = true;
    errorMessage = '';

    ngOnInit(): void {
        this.apiService.getMyClaims().subscribe({
            next: (claims) => {
                this.claims = claims;
                this.loading = false;
            },
            error: () => {
                this.errorMessage = 'Failed to load your claims. Please try again later.';
                this.loading = false;
            }
        });
    }
}

