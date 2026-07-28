import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild, computed, signal } from '@angular/core';
import { OcIconComponent } from '../../components/icon/icon.component';
import { BADGES, Badge, BadgeCategory } from './achievements.data';

type TabKey = 'all' | BadgeCategory;

const TABS: Array<{ key: TabKey; label: string }> = [
  { key: 'all', label: 'Tất cả' },
  { key: 'hoctap', label: 'Học tập' },
  { key: 'chuyencan', label: 'Chuyên cần' },
  { key: 'tuongtac', label: 'Tương tác' },
];

@Component({
  selector: 'app-achievements',
  standalone: true,
  imports: [OcIconComponent],
  templateUrl: './achievements.component.html',
  styleUrl: './achievements.component.scss',
})
export class AchievementsComponent implements AfterViewInit, OnDestroy {
  @ViewChild('summarySection') summarySectionRef?: ElementRef<HTMLElement>;

  readonly tabs = TABS;
  readonly activeTab = signal<TabKey>('all');

  readonly totalBadges = BADGES.length;
  readonly unlockedCount = BADGES.filter((b) => b.unlocked).length;
  readonly latestBadge = BADGES.filter((b) => b.unlocked).slice(-1)[0] ?? BADGES[0];

  readonly counterDisplay = signal(0);

  readonly filteredBadges = computed(() => {
    const tab = this.activeTab();
    return BADGES.filter((b) => tab === 'all' || b.category === tab);
  });

  private observer?: IntersectionObserver;
  private counterRaf?: number;

  ngAfterViewInit(): void {
    const el = this.summarySectionRef?.nativeElement;
    if (!el) return;
    this.observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((e) => e.isIntersecting)) {
          this.startCounter();
          this.observer?.disconnect();
        }
      },
      { threshold: 0.3 }
    );
    this.observer.observe(el);
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
    if (this.counterRaf) cancelAnimationFrame(this.counterRaf);
  }

  setTab(tab: TabKey): void {
    this.activeTab.set(tab);
  }

  animationDelayFor(index: number): number {
    return (index % 6) * 0.08;
  }

  medalGradient(badge: Badge): string {
    return badge.unlocked
      ? 'linear-gradient(135deg, #ffd76a, #ff9d4d)'
      : 'linear-gradient(135deg, var(--oc-neutral-400), var(--oc-neutral-500))';
  }

  private startCounter(): void {
    const target = this.unlockedCount;
    const duration = 1000;
    const start = performance.now();
    const step = (now: number) => {
      const t = Math.min(1, (now - start) / duration);
      const eased = 1 - Math.pow(1 - t, 3);
      this.counterDisplay.set(Math.round(target * eased));
      if (t < 1) this.counterRaf = requestAnimationFrame(step);
    };
    this.counterRaf = requestAnimationFrame(step);
  }
}
