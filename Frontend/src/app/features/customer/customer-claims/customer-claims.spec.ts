import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CustomerClaims } from './customer-claims';
import { ApiService } from '../../../core/services/api.service';
import { of, throwError } from 'rxjs';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';

import { RouterTestingModule } from '@angular/router/testing';

describe('CustomerClaims', () => {
    let component: CustomerClaims;
    let fixture: ComponentFixture<CustomerClaims>;
    let apiServiceSpy: jasmine.SpyObj<ApiService>;

    beforeEach(async () => {
        apiServiceSpy = jasmine.createSpyObj('ApiService', ['getMyClaims']);
        apiServiceSpy.getMyClaims.and.returnValue(of([]));

        await TestBed.configureTestingModule({
            imports: [CustomerClaims, CommonModule, RouterTestingModule],
            providers: [
                { provide: ApiService, useValue: apiServiceSpy },
                CurrencyPipe,
                DatePipe
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(CustomerClaims);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load claims on init', () => {
        const mockClaims = [{ claimId: 1, amount: 500 }];
        apiServiceSpy.getMyClaims.and.returnValue(of(mockClaims));

        component.ngOnInit();

        expect(apiServiceSpy.getMyClaims).toHaveBeenCalled();
        expect(component.claims).toEqual(mockClaims);
        expect(component.loading).toBeFalse();
    });

    it('should handle error when loading claims', () => {
        apiServiceSpy.getMyClaims.and.returnValue(throwError(() => new Error('Error')));

        component.ngOnInit();

        expect(component.errorMessage).toBe('Failed to load your claims. Please try again later.');
        expect(component.loading).toBeFalse();
    });
});
