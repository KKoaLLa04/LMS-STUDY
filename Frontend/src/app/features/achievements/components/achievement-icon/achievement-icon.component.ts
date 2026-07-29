import { Component, Input } from '@angular/core';

// Danh sách icon cho phép chọn khi tạo/sửa huy hiệu — khớp với ICON_OPTIONS trong
// achievement-list.component.ts và OcIconName (Frontend/src/app/clients/models/icon-name.type.ts)
// mà trang "Huy hiệu của tôi" phía học sinh dùng để hiển thị badge.icon.
export type AchievementIconKey =
  | 'book-open'
  | 'target'
  | 'trophy'
  | 'flame'
  | 'message-circle'
  | 'heart'
  | 'star'
  | 'crown'
  | 'users'
  | 'graduation-cap'
  | 'layers'
  | 'check';

/** Inline-SVG preview cho icon huy hiệu ở trang quản trị (độc lập với app-oc-icon phía client). */
@Component({
  selector: 'app-achievement-icon',
  standalone: true,
  templateUrl: './achievement-icon.component.html',
})
export class AchievementIconComponent {
  @Input() name!: AchievementIconKey | string;
  @Input() size = 18;
  @Input() color = 'currentColor';
}
