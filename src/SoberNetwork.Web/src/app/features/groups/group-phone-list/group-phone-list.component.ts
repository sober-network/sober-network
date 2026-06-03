import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { catchError, finalize, forkJoin, map, of, switchMap } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MemberDetailResponse, MemberProfileResponse, PhoneListEntryResponse } from '@app/core/models';
import { AuthService } from '@app/core/services/auth.service';
import { GroupService } from '@app/core/services/group.service';
import { MemberService } from '@app/core/services/member.service';

interface PhoneListView extends PhoneListEntryResponse {
  sobrietyDate: string | null;
  isSelf: boolean;
}

@Component({
  selector: 'app-group-phone-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
  ],
  templateUrl: './group-phone-list.component.html',
  styleUrl: './group-phone-list.component.scss',
})
export class GroupPhoneListComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly groupService = inject(GroupService);
  private readonly memberService = inject(MemberService);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);

  slug = '';
  groupName = '';
  entries: PhoneListView[] = [];
  loading = true;
  saving = false;
  error = '';
  saveError = '';
  saveSuccess = '';
  profile: MemberProfileResponse | null = null;

  readonly phoneForm = this.fb.group({
    isPhoneShared: [false, { nonNullable: true }],
    phoneNumber: [''],
  });

  get currentUserId(): string {
    return this.auth.currentUser?.userId ?? '';
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.slug = params.get('slug') ?? '';
      this.loadPhoneList();
    });
  }

  savePhoneSettings(): void {
    const isPhoneShared = this.phoneForm.controls.isPhoneShared.value === true;
    const phoneNumber = this.phoneForm.controls.phoneNumber.value?.trim() ?? '';
    const hasStoredPhone = this.profile?.hasPhone ?? false;

    this.saveError = '';
    this.saveSuccess = '';

    if (isPhoneShared && !phoneNumber && !hasStoredPhone) {
      this.saveError = 'Add a phone number before sharing it with the group.';
      return;
    }

    this.saving = true;

    const savePhone$ = phoneNumber
      ? this.memberService.setPhone({ phoneNumber })
      : of(void 0);

    savePhone$.pipe(
      switchMap(() => this.groupService.setPhoneVisibility(this.slug, { isPhoneShared })),
      finalize(() => {
        this.saving = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: () => {
        this.saveSuccess = isPhoneShared
          ? 'Your phone settings have been shared with the group.'
          : 'Your phone number is now hidden from this group.';
        this.loadPhoneList();
      },
      error: err => {
        this.saveError = this.getErrorMessage(err, 'We could not save your phone settings right now.');
      },
    });
  }

  trackByUserId(_: number, entry: PhoneListView): string {
    return entry.userId;
  }

  private loadPhoneList(): void {
    this.loading = true;
    this.error = '';

    forkJoin({
      group: this.groupService.getGroup(this.slug),
      phoneList: this.groupService.getPhoneList(this.slug),
      profile: this.memberService.getMyProfile().pipe(catchError(() => of(null))),
    }).pipe(
      switchMap(({ group, phoneList, profile }) => {
        this.groupName = group.name;
        this.profile = profile;

        const myEntry = phoneList.find(entry => entry.userId === this.currentUserId);
        this.phoneForm.reset({
          isPhoneShared: !!myEntry,
          phoneNumber: myEntry?.phoneNumber ?? '',
        });

        if (phoneList.length === 0) {
          return of({
            phoneList,
            details: [] as (MemberDetailResponse | null)[],
          });
        }

        return forkJoin(
          phoneList.map(entry => this.groupService.getMemberDetail(this.slug, entry.userId).pipe(
            catchError(() => of(null))
          ))
        ).pipe(
          map(details => ({ phoneList, details }))
        );
      }),
      finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: ({ phoneList, details }) => {
        this.entries = phoneList.map((entry, index) => ({
          ...entry,
          sobrietyDate: details[index]?.sobriety?.sobrietyDate ?? null,
          isSelf: entry.userId === this.currentUserId,
        }));
      },
      error: err => {
        this.entries = [];
        this.error = this.getErrorMessage(err, 'We could not load the phone list right now.');
      },
    });
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    return (error as { error?: { message?: string } })?.error?.message ?? fallback;
  }
}
