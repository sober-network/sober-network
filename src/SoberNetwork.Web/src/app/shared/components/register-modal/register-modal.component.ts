import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { MatDialogRef, MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '@app/core/services/auth.service';
import { BaseFormModalComponent } from '@app/shared/components/base-form-modal/base-form-modal.component';

function passwordsMatch(control: AbstractControl): ValidationErrors | null {
  const pw      = control.get('password')?.value;
  const confirm = control.get('confirmPassword')?.value;
  return pw && confirm && pw !== confirm ? { passwordsMismatch: true } : null;
}

@Component({
  selector: 'app-register-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    BaseFormModalComponent,
  ],
  templateUrl: './register-modal.component.html',
  styleUrl:    './register-modal.component.scss',
})
export class RegisterModalComponent {
  private readonly fb     = inject(FormBuilder);
  private readonly auth   = inject(AuthService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  readonly dialogRef      = inject(MatDialogRef<RegisterModalComponent>);

  title = 'Create your account';
  subtitle = 'Join Sober Network — anonymous & free';
  icon = 'person_add';
  useRainbowRing = true;

  form = this.fb.group({
    displayName:     ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
    email:           ['', [Validators.required, Validators.email]],
    password:        ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required],
  }, { validators: passwordsMatch });

  loading      = false;
  error        = '';
  success      = false;
  hidePassword = true;
  hideConfirm  = true;

  submit(): void {
   if (this.form.invalid) return;
   this.loading = true;
   this.error   = '';
   const { displayName, email, password } = this.form.getRawValue();
   this.auth.register({ displayName: displayName!, email: email!, password: password! }).subscribe({
     next: () => { this.success = true; this.loading = false; },
     error: err => {
       this.error   = err?.error?.message ?? 'Registration failed. Please try again.';
       this.loading = false;
     },
   });
  }

  async openSignInModal(): Promise<void> {
   this.dialogRef.close();
   const { LoginModalComponent } = await import('../login-modal/login-modal.component');
   this.dialog.open(LoginModalComponent, {
     panelClass: ['sn-modal-panel', 'sn-login-panel'],
     maxWidth:   '100vw',
     autoFocus:  'first-tabbable',
   });
  }

  navigateTo(path: string): void {
   this.dialogRef.close();
   this.router.navigate([path]);
  }
}
