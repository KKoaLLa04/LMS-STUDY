import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild, computed, inject, signal } from '@angular/core';
import { OcIconComponent } from '../../components/icon/icon.component';
import { Badge, BadgeCategory } from '../../models/badge.model';
import { AchievementService } from '../../services/achievement.service';

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
export class AchievementsComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('summarySection') summarySectionRef?: ElementRef<HTMLElement>;

  private readonly achievementService = inject(AchievementService);

  readonly tabs = TABS;
  readonly activeTab = signal<TabKey>('all');

  readonly badges = signal<Badge[]>([]);

  readonly totalBadges = computed(() => this.badges().length);
  readonly unlockedCount = computed(() => this.badges().filter((b) => b.unlocked).length);
  readonly latestBadge = computed(() => this.badges().filter((b) => b.unlocked).slice(-1)[0] ?? this.badges()[0]);

  readonly counterDisplay = signal(0);

  readonly filteredBadges = computed(() => {
    const tab = this.activeTab();
    return this.badges().filter((b) => tab === 'all' || b.category === tab);
  });

  private observer?: IntersectionObserver;
  private counterRaf?: number;

  ngOnInit(): void {
    this.achievementService.getMyAchievements().subscribe((badges) => this.badges.set(badges));
  }

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
    const target = this.unlockedCount();
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
