/** Shape returned by the backend (Backend/DTOs/AchievementDto.cs). */
export interface AchievementApi {
  id: number;
  name: string;
  description: string;
  /** "HocTap" | "ChuyenCan" | "TuongTac" — see Backend/Models/Achievement.cs. */
  category: string;
  iconKey: string;
  orderNumber: number;
  points: number;
}

/** Per-student unlock status (Backend/DTOs/AchievementDto.cs — MyAchievementDto),
 * returned by GET /api/achievements/me — the only source of unlock status
 * (per-student, via UserAchievement); the catalogue endpoint has no unlock flag. */
export interface MyAchievementApi {
  id: number;
  name: string;
  description: string;
  category: string;
  iconKey: string;
  orderNumber: number;
  points: number;
  unlockedByMe: boolean;
  unlockedAt?: string;
}
