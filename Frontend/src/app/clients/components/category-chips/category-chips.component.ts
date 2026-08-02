import {
  AfterViewInit,
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnChanges,
  OnDestroy,
  Output,
  ViewChild,
  signal,
} from '@angular/core';
import { OcIconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-oc-category-chips',
  standalone: true,
  imports: [OcIconComponent],
  templateUrl: './category-chips.component.html',
  styleUrl: './category-chips.component.scss',
})
export class CategoryChipsComponent implements AfterViewInit, OnChanges, OnDestroy {
  @Input({ required: true }) categories: string[] = [];
  @Input() activeCategory = '';
  @Output() activeCategoryChange = new EventEmitter<string>();

  @ViewChild('track') private readonly trackRef?: ElementRef<HTMLDivElement>;

  readonly canScrollLeft = signal(false);
  readonly canScrollRight = signal(false);

  private resizeObserver?: ResizeObserver;

  ngAfterViewInit(): void {
    this.updateScrollState();
    this.resizeObserver = new ResizeObserver(() => this.updateScrollState());
    this.resizeObserver.observe(this.trackRef!.nativeElement);
  }

  ngOnChanges(): void {
    // Track chưa render lại kịp khi categories đổi, chờ 1 tick rồi mới đo scrollWidth.
    setTimeout(() => this.updateScrollState());
  }

  ngOnDestroy(): void {
    this.resizeObserver?.disconnect();
  }

  scrollByPage(direction: 1 | -1): void {
    const el = this.trackRef?.nativeElement;
    if (!el) return;
    el.scrollBy({ left: direction * el.clientWidth * 0.7, behavior: 'smooth' });
  }

  onTrackScroll(): void {
    this.updateScrollState();
  }

  private updateScrollState(): void {
    const el = this.trackRef?.nativeElement;
    if (!el) return;
    this.canScrollLeft.set(el.scrollLeft > 2);
    this.canScrollRight.set(el.scrollLeft + el.clientWidth < el.scrollWidth - 2);
  }
}
