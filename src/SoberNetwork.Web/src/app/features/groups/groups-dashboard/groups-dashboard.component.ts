import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { GroupResponse } from '@app/core/models';
import { GroupService } from '@app/core/services/group.service';
import { ClientLogService } from '@app/core/services/client-log.service';

@Component({
  selector: 'app-groups-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
  ],
  templateUrl: './groups-dashboard.component.html',
  styleUrl: './groups-dashboard.component.scss',
})
export class GroupsDashboardComponent implements OnInit {
  private readonly groupService = inject(GroupService);
  private readonly dialog = inject(MatDialog);
  private readonly fb = inject(FormBuilder);
  private readonly log = inject(ClientLogService);
  private readonly cdr = inject(ChangeDetectorRef);

  @ViewChild('createGroupDialog') private createGroupDialog?: TemplateRef<unknown>;

  groups: GroupResponse[] = [];
  loading = true;
  creating = false;
  error = '';
  createError = '';

  private slugEdited = false;

  readonly createForm = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(80)]],
    slug: ['', [Validators.required, Validators.pattern(/^[a-z0-9]+(?:-[a-z0-9]+)*$/)]],
    description: ['', [Validators.maxLength(500)]],
    isPublic: [true, { nonNullable: true }],
    requiresApproval: [true, { nonNullable: true }],
  });

  constructor() {
    this.createForm.controls.name.valueChanges.subscribe(name => {
      if (!this.slugEdited) {
        this.createForm.controls.slug.setValue(this.slugify(name ?? ''), { emitEvent: false });
      }
    });
  }

  ngOnInit(): void {
    this.log.info('GroupsDashboard.ngOnInit');
    this.loadGroups();
  }

  openCreateGroupDialog(): void {
    if (!this.createGroupDialog) {
      return;
    }

    this.slugEdited = false;
    this.createError = '';
    this.createForm.reset({
      name: '',
      slug: '',
      description: '',
      isPublic: true,
      requiresApproval: true,
    });

    this.dialog.open(this.createGroupDialog, {
      width: '640px',
      maxWidth: '95vw',
    });
  }

  markSlugEdited(): void {
    this.slugEdited = true;
  }

  submitCreate(dialogRef: MatDialogRef<unknown>): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    const { name, slug, description, isPublic, requiresApproval } = this.createForm.getRawValue();
    const isGroupPublic = isPublic === true;
    const approvalRequired = requiresApproval === true;
    this.creating = true;
    this.createError = '';

    this.groupService.createGroup({
      name: name!.trim(),
      slug: slug!.trim(),
      description: description?.trim() || undefined,
      isPublic: isGroupPublic,
      requiresApproval: approvalRequired,
    }).pipe(
      finalize(() => {
        this.creating = false;
      })
    ).subscribe({
      next: () => {
        dialogRef.close();
        this.loadGroups();
      },
      error: err => {
        this.createError = this.getErrorMessage(err, 'We could not create your group just yet. Please try again.');
      },
    });
  }

  trackBySlug(_: number, group: GroupResponse): string {
    return group.slug;
  }

  descriptionPreview(group: GroupResponse): string {
    if (!group.description) {
      return 'A welcoming place to stay connected between meetings.';
    }

    return group.description;
  }

  private loadGroups(): void {
    this.loading = true;
    this.error = '';
    this.log.info('GroupsDashboard.loadGroups: subscribing');

    this.groupService.getMyGroups().pipe(
      finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
        this.log.info('GroupsDashboard.loadGroups: finalize', { loading: false });
      })
    ).subscribe({
      next: groups => {
        this.log.info('GroupsDashboard.loadGroups: next', { count: groups.length });
        this.groups = groups;
      },
      error: err => {
        this.log.error('GroupsDashboard.loadGroups: error', { message: String(err) });
        this.groups = [];
        this.error = this.getErrorMessage(err, 'We could not load your groups right now.');
      },
    });
  }

  private slugify(value: string): string {
    return value
      .toLowerCase()
      .trim()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '')
      .replace(/-{2,}/g, '-');
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    return (error as { error?: { message?: string } })?.error?.message ?? fallback;
  }
}
