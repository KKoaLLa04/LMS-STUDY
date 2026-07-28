export type AchievementCategory = 'HocTap' | 'ChuyenCan' | 'TuongTac';

export interface Achievement {
  id: number;
  name: string;
  description: string;
  category: AchievementCategory;
  iconKey: string;
  orderNumber: number;
  isUnlocked: boolean;
  progressPercent: number;
}

export interface CreateAchievementRequest {
  name: string;
  description: string;
  category: AchievementCategory;
  iconKey: string;
  orderNumber: number;
  isUnlocked: boolean;
  progressPercent: number;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  httpStatusCode?: number;
}
