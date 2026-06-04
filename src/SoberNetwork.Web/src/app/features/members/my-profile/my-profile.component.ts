import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MemberService } from '@app/core/services/member.service';
import { AuthService } from '@app/core/services/auth.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '@app/shared/components/confirm-dialog/confirm-dialog.component';
import {
  MemberProfileResponse, SobrietyResponse,
  UpdateProfileRequest, ChangePasswordRequest,
  ChangeEmailRequest, SetSobrietyDateRequest,
  SobrietyVisibilityRequest, SetPhoneRequest, DeleteAccountRequest
} from '@app/core/models';
import { MailingAddressResponse, UpdateMailingAddressRequest } from '@app/core/models';

function passwordsMatch(c: AbstractControl): ValidationErrors | null {
  const pw = c.get('newPassword')?.value;
  const confirm = c.get('confirmPassword')?.value;
  return pw && confirm && pw !== confirm ? { passwordsMismatch: true } : null;
}

@Component({
  selector: 'app-my-profile',
  standalone: true,
  imports: [
    CommonModule, RouterModule, ReactiveFormsModule,
    MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatSlideToggleModule,
    MatDatepickerModule, MatNativeDateModule,
    MatExpansionModule, MatProgressSpinnerModule,
    MatSnackBarModule, MatDialogModule, MatDividerModule,
  ],
  templateUrl: './my-profile.component.html',
  styleUrl: './my-profile.component.scss',
})
export class MyProfileComponent implements OnInit {
  private readonly memberService = inject(MemberService);
  private readonly auth = inject(AuthService);
  private readonly snack = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);

  profile: MemberProfileResponse | null = null;
  sobriety: SobrietyResponse | null = null;
  loading = true;

  // Profile form
  profileForm = this.fb.group({
    displayName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
    timeZone:    [''],
  });

  // Sobriety form
  sobrietyForm = this.fb.group({
    sobrietyDate: [null as Date | null],
    isPublic:     [false],
  });

  // Mailing address form
  addressForm = this.fb.group({
    street:     [''],
    city:       [''],
    state:      [''],
    postalCode: [''],
    country:    [''],
  });

  // Phone form
  phoneForm = this.fb.group({
    phoneNumber: [''],
  });

  // Change password form
  passwordForm = this.fb.group({
    currentPassword: ['', Validators.required],
    newPassword:     ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required],
  }, { validators: passwordsMatch });

  // Change email form
  emailForm = this.fb.group({
    newEmail:  ['', [Validators.required, Validators.email]],
    password:  ['', Validators.required],
  });

  // Delete account form
  deleteForm = this.fb.group({
    password:     ['', Validators.required],
    confirmation: ['', [Validators.required, Validators.pattern(/^DELETE MY ACCOUNT$/)]],
  });

  savingProfile   = false;
  savingSobriety  = false;
  savingAddress   = false;
  savingPhone     = false;
  savingPassword  = false;
  savingEmail     = false;
  hideCurrentPw   = true;
  hideNewPw       = true;
  hideConfirmPw   = true;
  hideEmailPw     = true;
  hideDeletePw    = true;

  ngOnInit(): void {
    this.memberService.getMyProfile().subscribe({
      next: p => {
        this.profile = p;
        this.profileForm.patchValue({ displayName: p.displayName, timeZone: p.timeZone ?? '' });
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => { this.loading = false; this.cdr.detectChanges(); },
    });
    this.memberService.getMySobriety().subscribe({
      next: s => {
        this.sobriety = s;
        this.sobrietyForm.patchValue({
          sobrietyDate: s.sobrietyDate ? new Date(s.sobrietyDate) : null,
          isPublic:     s.isPublic,
        });
      },
    });
    this.memberService.getMailingAddress().subscribe({
      next: addr => {
        if (addr) {
          this.addressForm.patchValue({
            street:     addr.mailingStreet     ?? '',
            city:       addr.mailingCity       ?? '',
            state:      addr.mailingState      ?? '',
            postalCode: addr.mailingPostalCode ?? '',
            country:    addr.mailingCountry    ?? '',
          });
        }
      },
    });
  }

  saveProfile(): void {
    if (this.profileForm.invalid) return;
    this.savingProfile = true;
    const req: UpdateProfileRequest = {
      displayName: this.profileForm.value.displayName ?? undefined,
      timeZone:    this.profileForm.value.timeZone    || undefined,
    };
    this.memberService.updateProfile(req).subscribe({
      next: p => { this.profile = p; this.savingProfile = false; this.snack.open('Profile updated', 'OK', { duration: 3000 }); },
      error: () => { this.savingProfile = false; this.snack.open('Update failed', 'OK', { duration: 3000 }); },
    });
  }

  saveSobriety(): void {
    this.savingSobriety = true;
    const { sobrietyDate, isPublic } = this.sobrietyForm.getRawValue();
    const visReq: SobrietyVisibilityRequest = { isPublic: isPublic ?? false };

    const saveDate$ = sobrietyDate
      ? this.memberService.setSobrietyDate({
          sobrietyDate: (sobrietyDate as Date).toISOString().split('T')[0],
        })
      : this.memberService.removeSobrietyDate();

    saveDate$.subscribe({
      next: () => {
        this.memberService.setSobrietyVisibility(visReq).subscribe({
          next: () => { this.savingSobriety = false; this.snack.open('Sobriety settings saved', 'OK', { duration: 3000 }); },
          error: () => { this.savingSobriety = false; this.snack.open('Save failed', 'OK', { duration: 3000 }); },
        });
      },
      error: () => { this.savingSobriety = false; this.snack.open('Save failed', 'OK', { duration: 3000 }); },
    });
  }

  saveAddress(): void {
    this.savingAddress = true;
    const v = this.addressForm.getRawValue();
    const req: UpdateMailingAddressRequest = {
      mailingStreet:     v.street     || null,
      mailingCity:       v.city       || null,
      mailingState:      v.state      || null,
      mailingPostalCode: v.postalCode || null,
      mailingCountry:    v.country    || null,
    };
    this.memberService.updateMailingAddress(req).subscribe({
      next: () => { this.savingAddress = false; this.snack.open('Address saved', 'OK', { duration: 3000 }); },
      error: () => { this.savingAddress = false; this.snack.open('Save failed', 'OK', { duration: 3000 }); },
    });
  }

  savePhone(): void {
    this.savingPhone = true;
    const req: SetPhoneRequest = { phoneNumber: this.phoneForm.value.phoneNumber || null };
    this.memberService.setPhone(req).subscribe({
      next: () => { this.savingPhone = false; this.snack.open('Phone number saved', 'OK', { duration: 3000 }); },
      error: () => { this.savingPhone = false; this.snack.open('Save failed', 'OK', { duration: 3000 }); },
    });
  }

  savePassword(): void {
    if (this.passwordForm.invalid) return;
    this.savingPassword = true;
    const { currentPassword, newPassword } = this.passwordForm.getRawValue();
    const req: ChangePasswordRequest = {
      currentPassword: currentPassword!,
      newPassword: newPassword!,
      confirmNewPassword: newPassword!,
    };
    this.memberService.changePassword(req).subscribe({
      next: () => { this.passwordForm.reset(); this.savingPassword = false; this.snack.open('Password changed', 'OK', { duration: 3000 }); },
      error: err => { this.savingPassword = false; this.snack.open(err?.error?.message ?? 'Change failed', 'OK', { duration: 4000 }); },
    });
  }

  saveEmail(): void {
    if (this.emailForm.invalid) return;
    this.savingEmail = true;
    const { newEmail, password } = this.emailForm.getRawValue();
    const req: ChangeEmailRequest = { newEmail: newEmail!, currentPassword: password! };
    this.memberService.changeEmail(req).subscribe({
      next: () => { this.emailForm.reset(); this.savingEmail = false; this.snack.open('Confirmation sent to new address', 'OK', { duration: 4000 }); },
      error: err => { this.savingEmail = false; this.snack.open(err?.error?.message ?? 'Change failed', 'OK', { duration: 4000 }); },
    });
  }

  deleteAccount(): void {
    if (this.deleteForm.invalid) return;
    const { password, confirmation } = this.deleteForm.getRawValue();
    const data: ConfirmDialogData = {
      title: 'Delete Account',
      message: 'This will permanently delete your account and remove you from all groups. This cannot be undone.',
      confirmLabel: 'Delete My Account',
      dangerous: true,
    };
    this.dialog.open(ConfirmDialogComponent, { data }).afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      const req: DeleteAccountRequest = { password: password!, confirmation: confirmation! };
      this.memberService.deleteAccount(req).subscribe({
        next: () => this.auth.logout(),
        error: err => this.snack.open(err?.error?.message ?? 'Delete failed', 'OK', { duration: 4000 }),
      });
    });
  }

  get daysSober(): number | null { return this.sobriety?.daysSober ?? null; }
}
