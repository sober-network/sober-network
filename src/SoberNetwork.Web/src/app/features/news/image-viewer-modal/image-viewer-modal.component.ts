import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { PostResponse } from '@app/core/models/news.models';
import { CommentSectionComponent } from '../comment-section/comment-section.component';

export interface ImageViewerModalData {
  imageUrl?: string | null;
  post: PostResponse;
  currentUserId: string;
  isGroupAdmin: boolean;
}

@Component({
  selector: 'app-image-viewer-modal',
  standalone: true,
  imports: [CommonModule, MatIconModule, CommentSectionComponent],
  templateUrl: './image-viewer-modal.component.html',
  styleUrl: './image-viewer-modal.component.scss',
})
export class ImageViewerModalComponent {
  readonly data = inject<ImageViewerModalData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject(MatDialogRef<ImageViewerModalComponent>);

  get authorInitial(): string {
    return (this.data.post.authorDisplayName?.[0] ?? '?').toUpperCase();
  }

  close(): void { this.dialogRef.close(); }
}
