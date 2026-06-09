import { Component, inject, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { finalize } from 'rxjs';
import { BaseFormModalComponent } from '@app/shared/components/base-form-modal/base-form-modal.component';
import { EmojiPickerComponent } from '@app/shared/components/emoji-picker/emoji-picker.component';
import { NewsService } from '@app/core/services/news.service';
import { PostResponse } from '@app/core/models/news.models';
import { environment } from '../../../../environments/environment';

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
  imports: [CommonModule, ReactiveFormsModule, MatProgressSpinnerModule, MatProgressBarModule, BaseFormModalComponent, EmojiPickerComponent],
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

  @ViewChild('bodyTextarea') bodyTextareaRef?: ElementRef<HTMLTextAreaElement>;
  @ViewChild('fileInput') fileInputRef?: ElementRef<HTMLInputElement>;

  saving = false;
  uploading = false;
  uploadProgress = 0;
  error = '';
  showOptional = false;
  showEmojiPicker = false;
  emojiPickerStyle: Record<string, string> = {};

  openEmojiPicker(btn: HTMLButtonElement, event: MouseEvent): void {
    event.stopPropagation();
    this.showEmojiPicker = !this.showEmojiPicker;
    if (this.showEmojiPicker) {
      const rect = btn.getBoundingClientRect();
      this.emojiPickerStyle = {
        position: 'fixed',
        top: `${rect.bottom + 6}px`,
        right: `${window.innerWidth - rect.right}px`,
        'z-index': '9999',
      };
    }
  }

  selectedFile: File | null = null;
  previewUrl: string | null = this.data.post?.mediaUrl
    ? environment.apiUrl + this.data.post.mediaUrl
    : null;
  mediaType: 'image' | 'video' | null = (this.data.post?.mediaType as 'image' | 'video') ?? null;
  isDragOver = false;

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
    mediaId: [this.data.post?.mediaId ?? null],
    linkUrl: [this.data.post?.linkUrl ?? '', [Validators.maxLength(500)]],
    linkTitle: [this.data.post?.linkTitle ?? '', [Validators.maxLength(200)]],
  });

  get bodyLength(): number { return this.form.value.body?.length ?? 0; }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.selectFile(input.files[0]);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = true;
  }

  onDragLeave(): void {
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
    if (event.dataTransfer?.files?.length) {
      this.selectFile(event.dataTransfer.files[0]);
    }
  }

  private selectFile(file: File): void {
    const maxImageSize = 5 * 1024 * 1024;  // 5 MB
    const maxVideoSize = 50 * 1024 * 1024; // 50 MB
    const imageTypes = ['image/jpeg', 'image/png'];
    const videoTypes = ['video/mp4'];

    if (imageTypes.includes(file.type)) {
      if (file.size > maxImageSize) {
        this.error = 'Image size must be less than 5 MB.';
        return;
      }
      this.mediaType = 'image';
    } else if (videoTypes.includes(file.type)) {
      if (file.size > maxVideoSize) {
        this.error = 'Video size must be less than 50 MB.';
        return;
      }
      this.mediaType = 'video';
    } else {
      this.error = 'Only JPEG, PNG (images) and MP4 (videos) are supported.';
      return;
    }

    this.selectedFile = file;
    this.error = '';
    this.uploadMedia();
  }

  private uploadMedia(): void {
    if (!this.selectedFile) return;

    this.uploading = true;
    this.uploadProgress = 0;

    const formData = new FormData();
    formData.append('file', this.selectedFile);

    // Get group slug from form
    const groupSlug = this.form.value.groupSlug || this.data.post?.groupSlug;

    this.newsService.uploadMedia(formData, groupSlug)
      .pipe(finalize(() => (this.uploading = false)))
      .subscribe({
        next: result => {
          this.form.controls.mediaId.setValue(result.mediaId);
          this.previewUrl = environment.apiUrl + result.mediaUrl;
          this.mediaType = result.mediaType as 'image' | 'video';
          this.selectedFile = null;
        },
        error: err => {
          this.error = (err as any)?.error?.message ?? 'Failed to upload media. Please try again.';
          this.selectedFile = null;
        },
      });
  }

  clearMedia(): void {
    this.selectedFile = null;
    this.previewUrl = null;
    this.mediaType = null;
    this.form.controls.mediaId.setValue(null);
  }

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
          mediaId: v.mediaId || null,
          linkUrl: v.linkUrl?.trim() || null,
          linkTitle: v.linkTitle?.trim() || null,
        })
      : this.newsService.createPost({
          groupSlug: v.groupSlug!,
          subject: v.subject!.trim(),
          body: v.body!.trim(),
          mediaId: v.mediaId || null,
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

  insertEmoji(emoji: string): void {
    this.showEmojiPicker = false;
    const el = this.bodyTextareaRef?.nativeElement;
    const current = this.form.controls.body.value ?? '';
    if (!el) {
      this.form.controls.body.setValue(current + emoji);
      return;
    }
    const start = el.selectionStart ?? current.length;
    const end = el.selectionEnd ?? current.length;
    const updated = current.slice(0, start) + emoji + current.slice(end);
    this.form.controls.body.setValue(updated);
    setTimeout(() => {
      el.selectionStart = start + emoji.length;
      el.selectionEnd = start + emoji.length;
      el.focus();
    });
  }

  cancel(): void { this.dialogRef.close(); }
}

