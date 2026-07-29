export interface Achievement {
  id: number;
  name: string;
  description: string;
  groupId: number;
  groupName: string;
  iconKey: string;
  orderNumber: number;
  isUnlocked: boolean;
  progressPercent: number;
  points: number;
}

export interface CreateAchievementRequest {
  name: string;
  description: string;
  groupId: number;
  iconKey: string;
  orderNumber: number;
  points: number;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  httpStatusCode?: number;
}
