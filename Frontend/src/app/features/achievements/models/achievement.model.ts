export interface AchievementCondition {
  id: number;
  conditionType: string;
  targetValue: number;
  logicGroup: number;
}

export interface CreateAchievementConditionRequest {
  conditionType: string;
  targetValue: number;
  logicGroup: number;
}

export interface Achievement {
  id: number;
  name: string;
  description: string;
  groupId: number;
  groupName: string;
  iconKey: string;
  orderNumber: number;
  points: number;
  conditions: AchievementCondition[];
}

export interface CreateAchievementRequest {
  name: string;
  description: string;
  groupId: number;
  iconKey: string;
  orderNumber: number;
  points: number;
  conditions: CreateAchievementConditionRequest[];
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  httpStatusCode?: number;
}
