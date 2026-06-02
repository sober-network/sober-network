import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  dangerous?: boolean;
  confirmationText?: string;
  confirmationPlaceholder?: string;
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon *ngIf="data.dangerous" color="warn" style="vertical-align: middle; margin-right: 6px;">warning</mat-icon>
      {{ data.title }}
    </h2>
    <mat-dialog-content>
      <p>{{ data.message }}</p>

      <mat-form-field appearance="outline" class="full-width" *ngIf="requiresTypedConfirmation">
        <mat-label>{{ data.confirmationPlaceholder ?? 'Type to confirm' }}</mat-label>
        <input matInput [formControl]="confirmationControl" />
        <mat-hint>Enter {{ data.confirmationText }} to continue.</mat-hint>
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button [mat-dialog-close]="false">
        {{ data.cancelLabel ?? 'Cancel' }}
      </button>
      <button
        mat-raised-button
        [color]="data.dangerous ? 'warn' : 'primary'"
        [mat-dialog-close]="true"
        [disabled]="!canConfirm"
      >
        {{ data.confirmLabel ?? 'Confirm' }}
      </button>
    </mat-dialog-actions>
  `,
})
export class ConfirmDialogComponent {
  readonly confirmationControl = new FormControl('');

  constructor(
    public dialogRef: MatDialogRef<ConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogData
  ) {}

  get requiresTypedConfirmation(): boolean {
    return !!this.data.confirmationText;
  }

  get canConfirm(): boolean {
    if (!this.requiresTypedConfirmation) {
      return true;
    }

    return this.confirmationControl.value?.trim() === this.data.confirmationText;
  }
}
