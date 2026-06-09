import {
  Component, Input, Output, EventEmitter, OnInit,
  HostListener, ElementRef, inject
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { PostResponse } from '@app/core/models/news.models';

@Component({
  selector: 'app-post-card',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule],
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

  menuOpen = false;
  imageError = false;

  get canEdit(): boolean {
    return this.post.authorId === this.currentUserId;
  }

  get canDelete(): boolean {
    return this.post.authorId === this.currentUserId || this.isGroupAdmin;
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

  ngOnInit(): void {}

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

  onImageError(): void {
    this.imageError = true;
  }
}
