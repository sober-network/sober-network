import {
  Component, Input, Output, EventEmitter, OnInit,
  HostListener, ElementRef, inject
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { PostResponse } from '@app/core/models/news.models';
import { CommentSectionComponent } from '../comment-section/comment-section.component';
import { NewsService } from '@app/core/services/news.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-post-card',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, CommentSectionComponent],
  templateUrl: './post-card.component.html',
  styleUrl: './post-card.component.scss',
})
export class PostCardComponent implements OnInit {
  @Input({ required: true }) post!: PostResponse;
  @Input() currentUserId = '';
  @Input() cardIndex = 0;
  @Input() isGroupAdmin = false;

  @Output() edit = new EventEmitter<PostResponse>();
  @Output() delete = new EventEmitter<PostResponse>();
  @Output() approve = new EventEmitter<PostResponse>();

  private readonly elRef = inject(ElementRef);
  private readonly dialog = inject(MatDialog);
  private readonly newsService = inject(NewsService);

  menuOpen = false;
  mediaError = false;
  likeCount = 0;
  isLiked = false;
  likeInFlight = false;

  ngOnInit(): void {
    this.likeCount = this.post.likeCount ?? 0;
    this.isLiked = this.post.isLikedByCurrentUser ?? false;
  }

  get canEdit(): boolean {
    return this.post.authorId === this.currentUserId;
  }

  get canDelete(): boolean {
    return this.post.authorId === this.currentUserId || this.isGroupAdmin;
  }

  get mediaFullUrl(): string | null {
    return this.post.mediaUrl ? environment.apiUrl + this.post.mediaUrl : null;
  }

  openImageViewer(imageUrl: string): void {
    import('../image-viewer-modal/image-viewer-modal.component').then(({ ImageViewerModalComponent }) => {
      this.dialog.open(ImageViewerModalComponent, {
        data: { imageUrl, post: this.post, currentUserId: this.currentUserId, isGroupAdmin: this.isGroupAdmin },
        width: '90vw',
        maxWidth: '1100px',
        height: '85vh',
        maxHeight: '90vh',
        panelClass: 'sn-image-viewer-panel',
      });
    });
  }

  get showMenu(): boolean {
    return this.canEdit || this.canDelete || this.post.needsApproval;
  }

  get relativeTime(): string {
    const diff = Date.now() - new Date(this.post.createdAt).getTime();
    const mins = Math.floor(diff / 60000);
    if (mins < 1) return 'Just now';
    if (mins < 60) return `${mins}m ago`;
    const hrs = Math.floor(mins / 60);
    if (hrs < 24) return `${hrs}h ago`;
    const days = Math.floor(hrs / 24);
    if (days < 7) return `${days}d ago`;
    return new Date(this.post.createdAt).toLocaleDateString('en-US', {
      month: 'short', day: 'numeric', year: 'numeric'
    });
  }

  get authorInitial(): string {
    return (this.post.authorDisplayName?.[0] ?? '?').toUpperCase();
  }

  toggleLike(): void {
    if (this.likeInFlight) return;
    // Optimistic update
    this.isLiked = !this.isLiked;
    this.likeCount += this.isLiked ? 1 : -1;
    this.likeInFlight = true;

    this.newsService.toggleLike(this.post.id).subscribe({
      next: result => {
        this.likeCount = result.likeCount;
        this.isLiked = result.isLiked;
        this.likeInFlight = false;
      },
      error: () => {
        // Roll back optimistic update
        this.isLiked = !this.isLiked;
        this.likeCount += this.isLiked ? 1 : -1;
        this.likeInFlight = false;
      },
    });
  }

  toggleMenu(event: MouseEvent): void {
    event.stopPropagation();
    this.menuOpen = !this.menuOpen;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elRef.nativeElement.contains(event.target)) {
      this.menuOpen = false;
    }
  }

  onEdit(): void {
    this.menuOpen = false;
    this.edit.emit(this.post);
  }

  onDelete(): void {
    this.menuOpen = false;
    this.delete.emit(this.post);
  }

  onApprove(): void {
    this.menuOpen = false;
    this.approve.emit(this.post);
  }

  onMediaError(): void {
    this.mediaError = true;
  }
}
