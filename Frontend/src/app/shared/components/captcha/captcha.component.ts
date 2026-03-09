import { Component, EventEmitter, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'app-captcha',
    standalone: true,
    imports: [CommonModule, FormsModule],
    template: `
    <div class="captcha-container p-4 bg-gray-50 rounded-xl border border-gray-100 mb-4 select-none">
      <div class="flex items-center justify-between mb-2">
        <span class="text-xs font-bold text-primary-plum uppercase tracking-wider">Security Verification</span>
        <button (click)="generateCaptcha()" type="button" class="text-primary-plum hover:rotate-180 transition-transform duration-500">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"></path>
          </svg>
        </button>
      </div>
      
      <div class="flex items-center gap-4">
        <div class="captcha-box flex-grow-0 bg-primary-plum text-white font-mono text-xl font-black italic tracking-[0.3em] px-4 py-2 rounded-lg shadow-inner pointer-events-none skew-x-[-10deg]">
          {{ captchaCode }}
        </div>
        <div class="flex-grow">
          <input 
            type="text" 
            [(ngModel)]="userInput" 
            (ngModelChange)="validate()"
            placeholder="Type code..." 
            class="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:ring-2 focus:ring-primary-plum/20 focus:border-primary-plum outline-none transition-all"
          >
        </div>
      </div>
      <p *ngIf="userInput && !isValid" class="text-[10px] text-red-500 mt-1 font-bold">Code doesn't match</p>
      <p *ngIf="isValid" class="text-[10px] text-green-600 mt-1 font-bold flex items-center gap-1">
        <svg class="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
          <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"></path>
        </svg>
        Verified
      </p>
    </div>
  `,
    styles: [`
    .captcha-box {
      text-shadow: 2px 2px 0px rgba(0,0,0,0.2);
      background-image: linear-gradient(45deg, rgba(255,255,255,0.1) 25%, transparent 25%, transparent 50%, rgba(255,255,255,0.1) 50%, rgba(255,255,255,0.1) 75%, transparent 75%, transparent);
      background-size: 10px 10px;
    }
  `]
})
export class CaptchaComponent implements OnInit {
    captchaCode: string = '';
    userInput: string = '';
    isValid: boolean = false;

    @Output() onVerified = new EventEmitter<boolean>();

    ngOnInit() {
        this.generateCaptcha();
    }

    generateCaptcha() {
        const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789'; // Avoid ambiguous chars
        let result = '';
        for (let i = 0; i < 6; i++) {
            result += chars.charAt(Math.floor(Math.random() * chars.length));
        }
        this.captchaCode = result;
        this.userInput = '';
        this.isValid = false;
        this.onVerified.emit(false);
    }

    validate() {
        this.isValid = this.userInput.toUpperCase() === this.captchaCode;
        this.onVerified.emit(this.isValid);
    }
}
