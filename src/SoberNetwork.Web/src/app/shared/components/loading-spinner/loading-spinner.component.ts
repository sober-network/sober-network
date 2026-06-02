import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule, MatProgressSpinnerModule],
  template: `
    <div class="spinner-overlay" *ngIf="visible">
      <mat-spinner [diameter]="diameter" color="primary"></mat-spinner>
      <p class="spinner-label" *ngIf="label">{{ label }}</p>
    </div>
  `,
  styles: [`
    .spinner-overlay {
      position: fixed;
      inset: 0;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      background: rgba(255, 255, 255, 0.72);
      backdrop-filter: blur(2px);
      z-index: 9999;
    }
    .spinner-label {
      margin-top: 16px;
      font-size: 0.9rem;
      opacity: 0.7;
    }
  `],
})
export class LoadingSpinnerComponent {
  @Input() visible = false;
  @Input() diameter = 48;
  @Input() label = '';
}
