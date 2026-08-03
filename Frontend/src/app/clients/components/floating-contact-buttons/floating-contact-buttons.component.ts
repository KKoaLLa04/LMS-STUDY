import { Component, Input, signal } from '@angular/core';
import { animate, query, stagger, style, transition, trigger } from '@angular/animations';

/** Cụm nút liên hệ nổi (Zalo / Hotline / Facebook) cố định góc dưới-phải,
 * chỉ được chèn vào layout của khu vực client (xem app-oc-shell). */
@Component({
  selector: 'app-oc-floating-contact',
  standalone: true,
  templateUrl: './floating-contact-buttons.component.html',
  styleUrl: './floating-contact-buttons.component.scss',
  animations: [
    trigger('fabStagger', [
      transition('closed => open', [
        query(
          '.oc-fab__item',
          [
            style({ opacity: 0, transform: 'translateY(14px) scale(0.4)' }),
            stagger(90, [
              animate(
                '320ms cubic-bezier(0.34, 1.56, 0.64, 1)',
                style({ opacity: 1, transform: 'translateY(0) scale(1)' })
              ),
            ]),
          ],
          { optional: true }
        ),
      ]),
      transition('open => closed', [
        query(
          '.oc-fab__item',
          [
            stagger(-60, [
              animate(
                '160ms ease-in',
                style({ opacity: 0, transform: 'translateY(14px) scale(0.4)' })
              ),
            ]),
          ],
          { optional: true }
        ),
      ]),
    ]),
  ],
})
export class FloatingContactButtonsComponent {
  @Input() phone = '036.803.1178';
  @Input() zaloLink = 'https://zalo.me/0368031178';
  @Input() facebookLink = 'https://www.facebook.com/kien.nguyenduy.169067';

  protected readonly open = signal(false);

  protected get phoneHref(): string {
    return 'tel:' + this.phone.replace(/\D/g, '');
  }

  protected toggle(): void {
    this.open.update((value) => !value);
  }

  protected close(): void {
    this.open.set(false);
  }
}
