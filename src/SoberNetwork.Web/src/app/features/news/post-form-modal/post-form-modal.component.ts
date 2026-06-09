import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { BaseFormModalComponent } from '@app/shared/components/base-form-modal/base-form-modal.component';
import { NewsService } from '@app/core/services/news.service';
import { PostResponse } from '@app/core/models/news.models';

export interface PostFormModalData {
  /** When provided, the modal is in edit mode. */
  post?: PostResponse;
  /** The user's group slugs for the group selector (create mode only). */
  groups: { name: string; slug: string }[];
}

export interface PostFormModalResult {
  post: PostResponse;
}

@Component({
  selector: 'app-post-form-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatProgressSpinnerModule, BaseFormModalComponent],
  templateUrl: './post-form-modal.component.html',
  styleUrl: './post-form-modal.component.scss',
})
export class PostFormModalComponent {
  private readonly fb = inject(FormBuilder);
  private readonly newsService = inject(NewsService);
  readonly dialogRef = inject(MatDialogRef<PostFormModalComponent, PostFormModalResult>);
  readonly data = inject<PostFormModalData>(MAT_DIALOG_DATA);

  get isEditMode(): boolean { return !!this.data.post; }
  get title(): string { return this.isEditMode ? 'Edit Post' : 'Share an Update'; }
  get subtitle(): string {
    return this.isEditMode
      ? 'Update your post.'
      : 'Share news or announcements with your group.';
  }

  saving = false;
  error = '';
  showOptional = false;

  readonly form = this.fb.group({
    groupSlug: [
      this.data.post?.groupSlug ?? (this.data.groups[0]?.slug ?? ''),
      [Validators.required],
    ],
    subject: [
      this.data.post?.subject ?? '',
      [Validators.required, Validators.maxLength(200)],
    ],
    body: [
      this.data.post?.body ?? '',
      [Validators.required, Validators.maxLength(5000)],
    ],
    imageUrl: [this.data.post?.imageUrl ?? '', [Validators.maxLength(500)]],
    linkUrl: [this.data.post?.linkUrl ?? '', [Validators.maxLength(500)]],
    linkTitle: [this.data.post?.linkTitle ?? '', [Validators.maxLength(200)]],
  });

  get bodyLength(): number { return this.form.value.body?.length ?? 0; }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    this.saving = true;
    this.error = '';

    const obs = this.isEditMode
      ? this.newsService.updatePost(this.data.post!.id, {
          subject: v.subject?.trim(),
          body: v.body?.trim(),
          imageUrl: v.imageUrl?.trim() || null,
          linkUrl: v.linkUrl?.trim() || null,
          linkTitle: v.linkTitle?.trim() || null,
        })
      : this.newsService.createPost({
          groupSlug: v.groupSlug!,
          subject: v.subject!.trim(),
          body: v.body!.trim(),
          imageUrl: v.imageUrl?.trim() || null,
          linkUrl: v.linkUrl?.trim() || null,
          linkTitle: v.linkTitle?.trim() || null,
        });

    obs.pipe(finalize(() => (this.saving = false))).subscribe({
      next: post => this.dialogRef.close({ post }),
      error: err =>
        (this.error =
          (err as { error?: { message?: string } })?.error?.message ??
          'Could not save post. Please try again.'),
    });
  }

  cancel(): void { this.dialogRef.close(); }
}

