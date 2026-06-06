import { Component, Input } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

/**
 * Base modal component for all forms (auth, group settings, etc).
 * Provides standard styling: icon, heading, optional subtitle, and form container.
 * Used by composition: child modals wrap their form in <app-base-form-modal> (they do NOT extend it).
 */
@Component({
  selector: 'app-base-form-modal',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './base-form-modal.component.html',
  styleUrl: './base-form-modal.component.scss',
})
export class BaseFormModalComponent {
  @Input() title = 'Form';
  @Input() subtitle = '';
  @Input() icon = 'lock';
  @Input() useRainbowRing = true;

  constructor(public dialogRef: MatDialogRef<BaseFormModalComponent>) {}
}
