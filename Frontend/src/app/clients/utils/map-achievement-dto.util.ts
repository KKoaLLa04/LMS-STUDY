import { AchievementApi, MyAchievementApi } from '../models/achievement-api.model';
import { Badge, BadgeCategory } from '../models/badge.model';
import { OcIconName } from '../models/icon-name.type';

const CATEGORY_MAP: Record<string, BadgeCategory> = {
  HocTap: 'hoctap',
  ChuyenCan: 'chuyencan',
  TuongTac: 'tuongtac',
};

export function mapAchievementDtoToBadge(dto: AchievementApi): Badge {
  return {
    id: String(dto.id),
    category: CATEGORY_MAP[dto.category] ?? 'hoctap',
    name: dto.name,
    desc: dto.description,
    icon: dto.iconKey as OcIconName,
    unlocked: dto.isUnlocked,
    progress: dto.isUnlocked ? undefined : dto.progressPercent,
  };
}

// Per-student real unlock status has no progress percentage yet (Achievement unlock is
// binary, granted by Admin via UserAchievement) — progress stays undefined either way.
export function mapMyAchievementDtoToBadge(dto: MyAchievementApi): Badge {
  return {
    id: String(dto.id),
    category: CATEGORY_MAP[dto.category] ?? 'hoctap',
    name: dto.name,
    desc: dto.description,
    icon: dto.iconKey as OcIconName,
    unlocked: dto.unlockedByMe,
  };
}
