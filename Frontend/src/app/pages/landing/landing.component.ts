import {
  Component,
  AfterViewInit,
  OnDestroy,
  HostListener,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.scss'],
})
export class LandingComponent implements AfterViewInit, OnDestroy {
  isScrolled = false;
  mobileMenuOpen = false;

  private scrollObserver: IntersectionObserver | null = null;
  private counterObserver: IntersectionObserver | null = null;

  particlesArray = Array.from({ length: 12 }, (_, i) => i);

  features = [
    {
      icon: '📚',
      title: 'Khóa học tương tác',
      description:
        'Hàng trăm bài giảng video sinh động, bài tập tương tác thú vị phù hợp với từng lứa tuổi và cấp học.',
      color: '#4361EE',
      bg: 'rgba(67,97,238,0.08)',
    },
    {
      icon: '🏆',
      title: 'Huy hiệu & Phần thưởng',
      description:
        'Hoàn thành nhiệm vụ để mở khóa huy hiệu đặc biệt. Mỗi huy hiệu là một thành tích đáng tự hào!',
      color: '#F8961E',
      bg: 'rgba(248,150,30,0.08)',
    },
    {
      icon: '📊',
      title: 'Theo dõi tiến độ',
      description:
        'Dashboard trực quan giúp học sinh và phụ huynh dễ dàng theo dõi sự tiến bộ hằng ngày.',
      color: '#43AA8B',
      bg: 'rgba(67,170,139,0.08)',
    },
  ];

  steps = [
    {
      number: '01',
      icon: '👤',
      title: 'Đăng ký tài khoản',
      desc: 'Tạo tài khoản miễn phí trong vòng 30 giây. Không cần thẻ tín dụng!',
    },
    {
      number: '02',
      icon: '📚',
      title: 'Chọn khóa học',
      desc: 'Duyệt qua hàng trăm khóa học phù hợp với cấp lớp và sở thích của bạn.',
    },
    {
      number: '03',
      icon: '🏆',
      title: 'Học & Chinh phục',
      desc: 'Hoàn thành bài học, làm bài kiểm tra và thu thập huy hiệu thành tích!',
    },
  ];

  badges = [
    { icon: '⭐', name: 'Ngôi sao', color: '#F9C74F' },
    { icon: '🏆', name: 'Vô địch', color: '#F8961E' },
    { icon: '💎', name: 'Kim cương', color: '#4361EE' },
    { icon: '🚀', name: 'Phi hành gia', color: '#7B2FBE' },
    { icon: '🔥', name: 'Siêu nhiệt', color: '#F72585' },
    { icon: '🌟', name: 'Xuất sắc', color: '#43AA8B' },
  ];

  recentAchievements = [
    { icon: '🔥', text: 'Streak 7 ngày', xp: '+200 XP' },
    { icon: '📚', text: 'Hoàn thành 10 bài', xp: '+150 XP' },
    { icon: '💯', text: 'Điểm tuyệt đối', xp: '+500 XP' },
  ];

  questItems = [
    { icon: '✅', text: 'Học 1 bài Toán', reward: '+100 XP', done: true },
    { icon: '✅', text: 'Làm bài kiểm tra', reward: '+200 XP', done: true },
    { icon: '⭕', text: 'Hoàn thành streak', reward: '+50 XP', done: false },
  ];

  stats = [
    { icon: '👨‍🎓', value: 10000, suffix: '+', label: 'Học sinh đang học' },
    { icon: '📚', value: 500, suffix: '+', label: 'Khóa học chất lượng' },
    { icon: '🏆', value: 50000, suffix: '+', label: 'Huy hiệu đã trao' },
    { icon: '⭐', value: 98, suffix: '%', label: 'Học sinh hài lòng' },
  ];

  benefits = [
    { icon: '🎮', text: 'Học như chơi game, không bao giờ nhàm chán' },
    { icon: '🏅', text: 'Hệ thống huy hiệu kích thích động lực học tập' },
    { icon: '📱', text: 'Học mọi lúc mọi nơi trên mọi thiết bị' },
    { icon: '🤝', text: 'Cộng đồng học sinh hỗ trợ lẫn nhau' },
    { icon: '🔒', text: 'An toàn, bảo mật tuyệt đối cho trẻ em' },
    { icon: '📈', text: 'Báo cáo tiến độ chi tiết cho phụ huynh' },
  ];

  @HostListener('window:scroll', [])
  onWindowScroll() {
    this.isScrolled = window.scrollY > 60;
  }

  ngAfterViewInit() {
    this.initScrollReveal();
    this.initCounterObserver();
    this.animateProgressBars();
  }

  ngOnDestroy() {
    this.scrollObserver?.disconnect();
    this.counterObserver?.disconnect();
  }

  toggleMobileMenu() {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  scrollToSection(event: Event, sectionId: string) {
    event.preventDefault();
    const el = document.getElementById(sectionId);
    if (el) el.scrollIntoView({ behavior: 'smooth' });
    this.mobileMenuOpen = false;
  }

  // Khởi tạo IntersectionObserver cho hiệu ứng scroll reveal
  private initScrollReveal() {
    this.scrollObserver = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add('animate-in');
          }
        });
      },
      { threshold: 0.1, rootMargin: '0px 0px -40px 0px' }
    );

    setTimeout(() => {
      document.querySelectorAll('.reveal').forEach((el) => {
        this.scrollObserver!.observe(el);
      });
    }, 150);
  }

  // Observer kích hoạt counter khi stats section hiển thị
  private initCounterObserver() {
    this.counterObserver = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            this.runCounters();
            this.counterObserver?.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.3 }
    );

    setTimeout(() => {
      const statsSection = document.querySelector('.stats-section');
      if (statsSection) this.counterObserver!.observe(statsSection);
    }, 150);
  }

  // Animation đếm số cho stats
  private runCounters() {
    const duration = 2200;

    document.querySelectorAll('.counter-value').forEach((el) => {
      const target = parseInt(el.getAttribute('data-target') || '0', 10);
      const startTime = performance.now();

      const tick = (now: number) => {
        const elapsed = now - startTime;
        const progress = Math.min(elapsed / duration, 1);
        // Easing: easeOutQuart
        const eased = 1 - Math.pow(1 - progress, 4);
        const current = Math.round(eased * target);
        el.textContent = this.formatCount(current);
        if (progress < 1) requestAnimationFrame(tick);
        else el.textContent = this.formatCount(target);
      };

      requestAnimationFrame(tick);
    });
  }

  private formatCount(n: number): string {
    if (n >= 1000) return (n / 1000).toFixed(n % 1000 === 0 ? 0 : 1) + 'k';
    return n.toString();
  }

  // Progress bars dùng scaleX (GPU compositor) thay vì width (layout trigger)
  // double-rAF: frame 1 đặt transition, frame 2 đổi transform → browser chắc chắn animate
  private animateProgressBars() {
    setTimeout(() => {
      document.querySelectorAll<HTMLElement>('.progress-fill').forEach((el) => {
        const raw = el.dataset['target'] ?? '0%';
        const pct  = parseFloat(raw) / 100;   // '85%' → 0.85

        // Reset về scaleX(0) trước khi animate
        el.style.transformOrigin = 'left center';
        el.style.transform       = 'scaleX(0)';

        requestAnimationFrame(() => {
          el.style.transition = 'transform 1.5s cubic-bezier(0.22, 1, 0.36, 1)';
          requestAnimationFrame(() => {
            el.style.transform = `scaleX(${pct})`;
          });
        });
      });
    }, 500);
  }
}
