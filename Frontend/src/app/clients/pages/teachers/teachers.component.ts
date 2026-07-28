import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OcIconComponent } from '../../components/icon/icon.component';
import { OcIconName } from '../../models/icon-name.type';
import { SUBJECTS, TEACHERS, TeacherRecord } from './teachers.data';

interface FilterSubject {
  key: string;
  name: string;
  icon: OcIconName;
  iconBg: string;
}

type SortKey = 'rating' | 'courses' | 'name';

const SUBJECT_ALL = 'all';

const FILTER_SUBJECTS: FilterSubject[] = [
  { key: SUBJECT_ALL, name: 'Tất cả', icon: 'grid', iconBg: 'var(--oc-neutral-500)' },
  ...SUBJECTS.map((s) => ({ key: s.key, name: s.name, icon: s.icon, iconBg: s.iconBg })),
];

const SORTS: Array<{ key: SortKey; label: string }> = [
  { key: 'rating', label: 'Đánh giá cao nhất' },
  { key: 'courses', label: 'Nhiều khoá học nhất' },
  { key: 'name', label: 'Tên A-Z' },
];

const PAGE_SIZE = 6;

const AVATAR_GRADIENTS = [
  'linear-gradient(135deg, var(--oc-accent-400), var(--oc-accent-700))',
  'linear-gradient(135deg, var(--oc-accent-2-400), var(--oc-accent-2-700))',
];

function initialsOf(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean);
  const first = parts[0]?.[0] ?? '';
  const last = parts[parts.length - 1]?.[0] ?? '';
  return (first + last).toUpperCase();
}

@Component({
  selector: 'app-teachers',
  standalone: true,
  imports: [RouterLink, OcIconComponent],
  templateUrl: './teachers.component.html',
  styleUrl: './teachers.component.scss',
})
export class TeachersComponent {
  readonly subjects = FILTER_SUBJECTS;
  readonly sorts = SORTS;

  readonly activeSubject = signal(SUBJECT_ALL);
  readonly searchQuery = signal('');
  readonly sortKey = signal<SortKey>('rating');
  readonly sortOpen = signal(false);
  readonly visibleCount = signal(PAGE_SIZE);
  readonly isLoading = signal(false);

  readonly sortLabel = computed(() => this.sorts.find((s) => s.key === this.sortKey())?.label ?? '');

  readonly filteredTeachers = computed(() => {
    const subject = this.activeSubject();
    const query = this.searchQuery().trim().toLowerCase();
    const sortKey = this.sortKey();

    const filtered = TEACHERS.filter(
      (t) => (subject === SUBJECT_ALL || t.subjectKey === subject) && t.name.toLowerCase().includes(query)
    );

    return filtered.slice().sort((a, b) => {
      if (sortKey === 'rating') return b.rating - a.rating;
      if (sortKey === 'courses') return b.coursesCount - a.coursesCount;
      return a.name.localeCompare(b.name, 'vi');
    });
  });

  readonly visibleTeachers = computed(() => this.filteredTeachers().slice(0, this.visibleCount()));
  readonly hasMore = computed(() => this.visibleCount() < this.filteredTeachers().length);

  setSubject(key: string): void {
    this.activeSubject.set(key);
    this.visibleCount.set(PAGE_SIZE);
  }

  onSearchInput(event: Event): void {
    this.searchQuery.set((event.target as HTMLInputElement).value);
    this.visibleCount.set(PAGE_SIZE);
  }

  toggleSort(): void {
    this.sortOpen.update((v) => !v);
  }

  selectSort(key: SortKey): void {
    this.sortKey.set(key);
    this.sortOpen.set(false);
  }

  loadMore(): void {
    this.isLoading.set(true);
    setTimeout(() => {
      this.visibleCount.update((v) => v + PAGE_SIZE);
      this.isLoading.set(false);
    }, 600);
  }

  animationDelayFor(index: number): number {
    return (index % 6) * 0.07;
  }

  avatarGradient(index: number): string {
    return AVATAR_GRADIENTS[index % AVATAR_GRADIENTS.length];
  }

  subjectColor(subjectKey: string): string {
    return this.subjects.find((s) => s.key === subjectKey)?.iconBg ?? 'var(--oc-neutral-500)';
  }

  subjectName(t: TeacherRecord): string {
    return this.subjects.find((s) => s.key === t.subjectKey)?.name ?? t.subjectKey;
  }

  initials(name: string): string {
    return initialsOf(name);
  }
}
