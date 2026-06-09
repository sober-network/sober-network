import {
  Component, Input, OnInit, OnDestroy, inject,
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

  @ViewChild('newCommentInput') newCommentInput?: ElementRef<HTMLTextAreaElement>;
  @ViewChild('replyInputRef') replyInputRef?: ElementRef<HTMLTextAreaElement>;

  private readonly newsService = inject(NewsService);
  private readonly destroy$ = new Subject<void>();

  // State
  expanded = false;
  loading = false;
  commentTree: CommentNode[] = [];
  localCount = 0;

  // New comment input
  newCommentBody = '';
  submitting = false;
  showEmojiPicker = false;

  // Reply state
  replyingToId: string | null = null;
  replyBody = '';
  replySubmitting = false;
  showReplyEmojiPicker = false;

  // Edit state
  editingCommentId: string | null = null;
  editBody = '';
  editSubmitting = false;
  showEditEmojiPicker = false;

  // Open menus
  openMenuId: string | null = null;

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onEnterComment(e: KeyboardEvent): void {
    if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); this.submitComment(); }
  }

  onEnterReply(e: KeyboardEvent): void {
    if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); this.submitReply(); }
  }

  get displayCount(): number {
    return this.expanded ? this.commentTree.flatMap(c => [c, ...c.replies]).length : this.localCount;
  }

  toggle(): void {
    if (this.expanded) {
      this.expanded = false;
      return;
    }
    this.expanded = true;
    if (this.commentTree.length === 0) {
      this.load();
    }
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

  submitComment(): void {
    const body = this.newCommentBody.trim();
    if (!body || this.submitting) return;
    this.submitting = true;
    this.showEmojiPicker = false;

    this.newsService.createComment(this.postId, { body })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: comment => {
          this.commentTree = [...this.commentTree, toNode(comment)];
          this.localCount++;
          this.newCommentBody = '';
          this.submitting = false;
        },
        error: () => { this.submitting = false; },
      });
  }

  insertEmoji(emoji: string): void {
    this.newCommentBody = insertAtCursor(
      this.newCommentInput?.nativeElement, this.newCommentBody, emoji);
    this.showEmojiPicker = false;
  }

  // ── Replies ────────────────────────────────────────────────────────────────

  startReply(commentId: string): void {
    this.replyingToId = commentId;
    this.replyBody = '';
    this.editingCommentId = null;
    setTimeout(() => this.replyInputRef?.nativeElement?.focus());
  }

  cancelReply(): void {
    this.replyingToId = null;
    this.replyBody = '';
    this.showReplyEmojiPicker = false;
  }

  submitReply(): void {
    const body = this.replyBody.trim();
    if (!body || this.replySubmitting || !this.replyingToId) return;
    this.replySubmitting = true;
    this.showReplyEmojiPicker = false;

    this.newsService.createComment(this.postId, { body, parentCommentId: this.replyingToId })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: comment => {
          this.commentTree = this.commentTree.map(node =>
            node.id === this.replyingToId
              ? { ...node, replies: [...node.replies, toNode(comment)] }
              : node
          );
          this.localCount++;
          this.replyingToId = null;
          this.replyBody = '';
          this.replySubmitting = false;
        },
        error: () => { this.replySubmitting = false; },
      });
  }

  insertReplyEmoji(emoji: string): void {
    this.replyBody = insertAtCursor(
      this.replyInputRef?.nativeElement, this.replyBody, emoji);
    this.showReplyEmojiPicker = false;
  }

  // ── Edit ───────────────────────────────────────────────────────────────────

  startEdit(comment: CommentResponse): void {
    this.editingCommentId = comment.id;
    this.editBody = comment.body;
    this.replyingToId = null;
    this.openMenuId = null;
  }

  cancelEdit(): void {
    this.editingCommentId = null;
    this.editBody = '';
    this.showEditEmojiPicker = false;
  }

  submitEdit(comment: CommentResponse): void {
    const body = this.editBody.trim();
    if (!body || this.editSubmitting) return;
    this.editSubmitting = true;

    this.newsService.updateComment(this.postId, comment.id, { body })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: updated => {
          this.commentTree = patchComment(this.commentTree, comment.id, updated);
          this.editingCommentId = null;
          this.editBody = '';
          this.editSubmitting = false;
        },
        error: () => { this.editSubmitting = false; },
      });
  }

  // ── Delete ─────────────────────────────────────────────────────────────────

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

  // ── Menu ────────────────────────────────────────────────────────────────────

  toggleMenu(id: string, e: MouseEvent): void {
    e.stopPropagation();
    this.openMenuId = this.openMenuId === id ? null : id;
  }

  @HostListener('document:click')
  closeMenus(): void { this.openMenuId = null; }

  // ── Helpers ─────────────────────────────────────────────────────────────────

  canEdit(comment: CommentResponse): boolean {
    return comment.authorId === this.currentUserId;
  }

  canDelete(comment: CommentResponse): boolean {
    return comment.authorId === this.currentUserId || this.isGroupAdmin;
  }

  authorInitial(name: string): string {
    return (name?.[0] ?? '?').toUpperCase();
  }

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

// ── Pure helpers ─────────────────────────────────────────────────────────────

function toNode(c: CommentResponse): CommentNode {
  return { ...c, replies: [] };
}

function buildTree(flat: CommentResponse[]): CommentNode[] {
  const map = new Map<string, CommentNode>();
  const roots: CommentNode[] = [];
  for (const c of flat) {
    map.set(c.id, toNode(c));
  }
  for (const node of map.values()) {
    if (node.parentCommentId) {
      const parent = map.get(node.parentCommentId);
      if (parent) { parent.replies.push(node); continue; }
    }
    roots.push(node);
  }
  return roots;
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
  const start = el.selectionStart ?? current.length;
  const end = el.selectionEnd ?? current.length;
  const updated = current.slice(0, start) + emoji + current.slice(end);
  setTimeout(() => {
    el.selectionStart = start + emoji.length;
    el.selectionEnd = start + emoji.length;
    el.focus();
  });
  return updated;
}
