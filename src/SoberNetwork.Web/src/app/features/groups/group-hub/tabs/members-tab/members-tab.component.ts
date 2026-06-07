import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
  inject,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { MemberResponse, MemberSortBy } from '@app/core/models';
import { GroupService } from '@app/core/services/group.service';

@Component({
  selector: 'app-members-tab',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, FormsModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './members-tab.component.html',
  styleUrl: './members-tab.component.scss',
})
export class MembersTabComponent implements OnInit, OnChanges {
  private readonly groupService = inject(GroupService);
  private readonly cdr = inject(ChangeDetectorRef);

  @Input({ required: true }) slug!: string;
  @Input() isAdmin = false;
  @Input() memberCount = 0;

  allMembers: MemberResponse[] = [];
  filteredMembers: MemberResponse[] = [];
  loading = true;
  error = '';

  searchQuery = '';
  sortBy: MemberSortBy = 'Name';
  sortDescending = false;

  readonly sortOptions = [
    { label: 'Name A→Z', sortBy: 'Name' as MemberSortBy, sortDescending: false },
    { label: 'Name Z→A', sortBy: 'Name' as MemberSortBy, sortDescending: true },
    { label: 'Joined (oldest)', sortBy: 'JoinedAt' as MemberSortBy, sortDescending: false },
    { label: 'Joined (newest)', sortBy: 'JoinedAt' as MemberSortBy, sortDescending: true },
    { label: 'Sobriety date', sortBy: 'SobrietyDate' as MemberSortBy, sortDescending: false },
    { label: 'Role', sortBy: 'Role' as MemberSortBy, sortDescending: false },
  ];

  ngOnInit(): void {
    this.loadMembers();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['slug'] && !changes['slug'].firstChange) {
      this.loadMembers();
    }
  }

  onSearch(): void {
    this.applyFilter();
    this.cdr.markForCheck();
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.applyFilter();
    this.cdr.markForCheck();
  }

  applySortOption(label: string): void {
    const option = this.sortOptions.find(opt => opt.label === label);
    if (!option) {
      return;
    }

    this.sortBy = option.sortBy;
    this.sortDescending = option.sortDescending;
    this.loadMembers();
  }

  selectedSortLabel(): string {
    return this.sortOptions.find(opt => opt.sortBy === this.sortBy && opt.sortDescending === this.sortDescending)?.label
      ?? this.sortOptions[0].label;
  }

  memberInitial(member: MemberResponse): string {
    return (member.displayName[0] ?? '?').toUpperCase();
  }

  roleClass(role: string): string {
    return role === 'GroupAdmin' ? 'role-admin' : 'role-member';
  }

  roleLabel(role: string): string {
    return role === 'GroupAdmin' ? 'Admin' : 'Member';
  }

  joinedDate(member: MemberResponse): string {
    return new Date(member.joinedAt).toLocaleDateString(undefined, { dateStyle: 'medium' });
  }

  emailHref(email: string): string {
    return `mailto:${email}`;
  }

  phoneHref(phone: string): string | null {
    const normalized = phone.replace(/[^\d+]/g, '');
    return normalized ? `tel:${normalized}` : null;
  }

  trackById(_: number, member: MemberResponse): string {
    return member.userId;
  }

  private loadMembers(): void {
    if (!this.slug) {
      return;
    }

    this.loading = true;
    this.error = '';
    this.cdr.markForCheck();

    this.groupService.getMembers(this.slug, 1, 200, undefined, this.sortBy, this.sortDescending).pipe(
      finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      })
    ).subscribe({
      next: response => {
        this.allMembers = response.items;
        this.applyFilter();
      },
      error: () => {
        this.error = 'Could not load members right now.';
        this.allMembers = [];
        this.filteredMembers = [];
      },
    });
  }

  private applyFilter(): void {
    const query = this.searchQuery.trim().toLowerCase();
    this.filteredMembers = query
      ? this.allMembers.filter(member => member.displayName.toLowerCase().includes(query))
      : [...this.allMembers];
  }
}
