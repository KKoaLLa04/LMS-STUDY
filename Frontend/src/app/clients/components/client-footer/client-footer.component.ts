import { Component } from '@angular/core';
import { OcIconComponent } from '../icon/icon.component';

interface FooterSocial {
  key: 'fb' | 'yt' | 'ig' | 'tt';
  label: string;
  href: string;
  bg: string;
}

interface FooterLink {
  label: string;
  href: string;
}

@Component({
  selector: 'app-oc-footer',
  standalone: true,
  imports: [OcIconComponent],
  templateUrl: './client-footer.component.html',
  styleUrl: './client-footer.component.scss',
})
export class ClientFooterComponent {
  readonly socials: FooterSocial[] = [
    { key: 'fb', label: 'Facebook', href: '#', bg: 'var(--oc-accent-500)' },
    { key: 'yt', label: 'YouTube', href: '#', bg: 'var(--oc-accent-2-500)' },
    { key: 'ig', label: 'Instagram', href: '#', bg: 'var(--oc-accent-600)' },
    { key: 'tt', label: 'TikTok', href: '#', bg: 'var(--oc-accent-2-600)' },
  ];

  readonly categoryLinks: FooterLink[] = ['Khoá học', 'Giáo viên', 'Bảng xếp hạng', 'Thành tích'].map((label) => ({
    label,
    href: '#',
  }));

  readonly supportLinks: FooterLink[] = ['FAQ', 'Liên hệ', 'Chính sách', 'Điều khoản sử dụng'].map((label) => ({
    label,
    href: '#',
  }));

  readonly paymentBadges = ['VISA', 'Momo', 'ZaloPay', 'VNPay'];
}
