import { OcIconName } from './icon-name.type';

export type BadgeCategory = 'hoctap' | 'chuyencan' | 'tuongtac';

export interface Badge {
  id: string;
  category: BadgeCategory;
  name: string;
  desc: string;
  icon: OcIconName;
  unlocked: boolean;
  progress?: number;
}
