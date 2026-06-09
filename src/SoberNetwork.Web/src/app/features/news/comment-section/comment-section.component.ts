import {
  Component, Input, OnDestroy, inject,
  ElementRef, ViewChild, HostListener,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject, takeUntil } from 'rxjs';
import { NewsService } from '@app/core/services/news.service';
import { CommentResponse, CommentNode } from '@app/core/models/news.models';
import { EmojiPickerComponent } from '@app/shared/components/emoji-picker/emoji-picker.component';

/** Per-input link metadata (new comment, reply, edit). */
interface MediaFields {
  linkUrl: string;
  linkTitle: string;
  showOptional: boolean;
}

function emptyMedia(): MediaFields {
  return { linkUrl: '', linkTitle: '', showOptional: false };
}

@Component({
  selector: 'app-comment-section',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatProgressSpinnerModule, EmojiPickerComponent],
  templateUrl: './comment-section.component.html',
  styleUrl: './comment-section.component.scss',
})
export class CommentSectionComponent implements OnDestroy {
  @Input({ required: true }) postId!: string;
  @Input() currentUserId = '';
  @Input() isGroupAdmin = false;
  @Input() initialCommentCount = 0;

  @ViewChild('newCommentInput') newCommentInputRef?: ElementRef<HTMLTextAreaElement>;
  @ViewChild('replyInputEl') replyInputRef?: ElementRef<HTMLTextAreaElement>;

  private readonly newsService = inject(NewsService);
  private readonly destroy$ = new Subject<void>();

  // Feed state
  expanded = false;
  loading = false;
  commentTree: CommentNode[] = [];
  localCount = 0;

  // New top-level comment
  newBody = '';
  newMedia: MediaFields = emptyMedia();
  newSubmitting = false;
  showNewEmoji = false;

  // Reply state
  replyingToId: string | null = null;
  replyBody = '';
  replyMedia: MediaFields = emptyMedia();
  replySubmitting = false;
  showReplyEmoji = false;

  // Edit state
  editingCommentId: string | null = null;
  editBody = '';
  editMedia: MediaFields = emptyMedia();
  editSubmitting = false;
  showEditEmoji = false;

  // Open menus
  openMenuId: string | null = null;

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onEnterNew(e: KeyboardEvent): void {
    if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); this.submitNew(); }
  }

  onEnterReply(e: KeyboardEvent): void {
    if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); this.submitReply(); }
  }

  get displayCount(): number {
    return this.localCount || this.initialCommentCount;
  }

  clampDepth(depth: number): number {
    return Math.min(depth, 4);
  }

  // ── Expand / collapse ─────────────────────────────────────────────────────

  toggle(): void {
    if (this.expanded) { this.expanded = false; return; }
    this.expanded = true;
    if (this.commentTree.length === 0 && !this.loading) this.load();
  }

  private load(): void {
    this.loading = true;
    this.newsService.getComments(this.postId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: flat => {
          this.commentTree = buildTree(flat);
          this.localCount = flat.length;
          this.loading = false;
        },
        error: () => { this.loading = false; },
      });
  }

  // ── New top-level comment ─────────────────────────────────────────────────

  submitNew(): void {
    const body = this.newBody.trim();
    if (!body || this.newSubmitting) return;
    this.newSubmitting = true;
    this.showNewEmoji = false;

    this.newsService.createComment(this.postId, {
      body,
      linkUrl: this.newMedia.linkUrl.trim() || null,
      linkTitle: this.newMedia.linkTitle.trim() || null,
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: c => {
        this.commentTree = [...this.commentTree, toNode(c)];
        this.localCount++;
        this.newBody = '';
        this.newMedia = emptyMedia();
        this.newSubmitting = false;
      },
      error: () => { this.newSubmitting = false; },
    });
  }

  insertNewEmoji(emoji: string): void {
    this.newBody = insertAtCursor(this.newCommentInputRef?.nativeElement, this.newBody, emoji);
    this.showNewEmoji = false;
  }

  // ── Replies (any depth) ───────────────────────────────────────────────────

  startReply(commentId: string): void {
    this.replyingToId = commentId;
    this.replyBody = '';
    this.replyMedia = emptyMedia();
    this.editingCommentId = null;
    setTimeout(() => this.replyInputRef?.nativeElement?.focus());
  }

  cancelReply(): void {
    this.replyingToId = null;
    this.replyBody = '';
    this.replyMedia = emptyMedia();
    this.showReplyEmoji = false;
  }

  submitReply(): void {
    const body = this.replyBody.trim();
    if (!body || this.replySubmitting || !this.replyingToId) return;
    this.replySubmitting = true;
    this.showReplyEmoji = false;
    const parentId = this.replyingToId;

    this.newsService.createComment(this.postId, {
      body,
      parentCommentId: parentId,
      linkUrl: this.replyMedia.linkUrl.trim() || null,
      linkTitle: this.replyMedia.linkTitle.trim() || null,
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: c => {
        this.commentTree = insertReply(this.commentTree, parentId, toNode(c));
        this.localCount++;
        this.replyingToId = null;
        this.replyBody = '';
        this.replyMedia = emptyMedia();
        this.replySubmitting = false;
      },
      error: () => { this.replySubmitting = false; },
    });
  }

  insertReplyEmoji(emoji: string): void {
    this.replyBody = insertAtCursor(this.replyInputRef?.nativeElement, this.replyBody, emoji);
    this.showReplyEmoji = false;
  }

  // ── Edit ──────────────────────────────────────────────────────────────────

  startEdit(comment: CommentResponse): void {
    this.editingCommentId = comment.id;
    this.editBody = comment.body;
    this.editMedia = {
      linkUrl: comment.linkUrl ?? '',
      linkTitle: comment.linkTitle ?? '',
      showOptional: !!comment.linkUrl,
    };
    this.replyingToId = null;
    this.openMenuId = null;
  }

  cancelEdit(): void {
    this.editingCommentId = null;
    this.editBody = '';
    this.editMedia = emptyMedia();
    this.showEditEmoji = false;
  }

  submitEdit(comment: CommentResponse): void {
    const body = this.editBody.trim();
    if (!body || this.editSubmitting) return;
    this.editSubmitting = true;

    this.newsService.updateComment(this.postId, comment.id, {
      body,
      linkUrl: this.editMedia.linkUrl.trim() || null,
      linkTitle: this.editMedia.linkTitle.trim() || null,
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: updated => {
        this.commentTree = patchComment(this.commentTree, comment.id, updated);
        this.editingCommentId = null;
        this.editBody = '';
        this.editMedia = emptyMedia();
        this.editSubmitting = false;
      },
      error: () => { this.editSubmitting = false; },
    });
  }

  insertEditEmoji(emoji: string): void {
    this.editBody = this.editBody + emoji;
    this.showEditEmoji = false;
  }

  // ── Delete ────────────────────────────────────────────────────────────────

  deleteComment(comment: CommentResponse): void {
    this.openMenuId = null;
    if (!confirm('Delete this comment?')) return;

    this.newsService.deleteComment(this.postId, comment.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.commentTree = removeComment(this.commentTree, comment.id);
          this.localCount = Math.max(0, this.localCount - 1);
        },
        error: () => alert('Could not delete comment.'),
      });
  }

  // ── Menu ──────────────────────────────────────────────────────────────────

  toggleMenu(id: string, e: MouseEvent): void {
    e.stopPropagation();
    this.openMenuId = this.openMenuId === id ? null : id;
  }

  @HostListener('document:click')
  closeMenus(): void { this.openMenuId = null; }

  // ── Helpers ───────────────────────────────────────────────────────────────

  canEdit(c: CommentResponse): boolean { return c.authorId === this.currentUserId; }
  canDelete(c: CommentResponse): boolean { return c.authorId === this.currentUserId || this.isGroupAdmin; }
  authorInitial(name: string): string { return (name?.[0] ?? '?').toUpperCase(); }

  relativeTime(iso: string): string {
    const diff = Date.now() - new Date(iso).getTime();
    const mins = Math.floor(diff / 60000);
    if (mins < 1) return 'Just now';
    if (mins < 60) return `${mins}m`;
    const hrs = Math.floor(mins / 60);
    if (hrs < 24) return `${hrs}h`;
    const days = Math.floor(hrs / 24);
    if (days < 7) return `${days}d`;
    return new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }
}

// ── Pure tree helpers ─────────────────────────────────────────────────────────

function toNode(c: CommentResponse): CommentNode {
  return { ...c, replies: [] };
}

function buildTree(flat: CommentResponse[]): CommentNode[] {
  const map = new Map<string, CommentNode>(flat.map(c => [c.id, toNode(c)]));
  const roots: CommentNode[] = [];
  for (const node of map.values()) {
    if (node.parentCommentId && map.has(node.parentCommentId)) {
      map.get(node.parentCommentId)!.replies.push(node);
    } else {
      roots.push(node);
    }
  }
  return roots;
}

/** Recursively inserts a new reply under the parent node at any depth. */
function insertReply(tree: CommentNode[], parentId: string, reply: CommentNode): CommentNode[] {
  return tree.map(n => {
    if (n.id === parentId) return { ...n, replies: [...n.replies, reply] };
    if (n.replies.length) return { ...n, replies: insertReply(n.replies, parentId, reply) };
    return n;
  });
}

function patchComment(tree: CommentNode[], id: string, updated: CommentResponse): CommentNode[] {
  return tree.map(n => {
    if (n.id === id) return { ...updated, replies: n.replies };
    return { ...n, replies: patchComment(n.replies, id, updated) };
  });
}

function removeComment(tree: CommentNode[], id: string): CommentNode[] {
  return tree
    .filter(n => n.id !== id)
    .map(n => ({ ...n, replies: removeComment(n.replies, id) }));
}

function insertAtCursor(el: HTMLTextAreaElement | undefined, current: string, emoji: string): string {
  if (!el) return current + emoji;
  const s = el.selectionStart ?? current.length;
  const e = el.selectionEnd ?? current.length;
  const updated = current.slice(0, s) + emoji + current.slice(e);
  setTimeout(() => { el.selectionStart = s + emoji.length; el.selectionEnd = s + emoji.length; el.focus(); });
  return updated;
}
