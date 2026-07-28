/** Shape returned by the backend (Backend/DTOs/AchievementDto.cs). */
export interface AchievementApi {
  id: number;
  name: string;
  description: string;
  /** "HocTap" | "ChuyenCan" | "TuongTac" — see Backend/Models/Achievement.cs. */
  category: string;
  iconKey: string;
  orderNumber: number;
  isUnlocked: boolean;
  progressPercent: number;
}
