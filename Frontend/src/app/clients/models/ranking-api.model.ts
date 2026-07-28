/** Shapes returned by the backend (Backend/DTOs/RankingDto.cs). */
import { PagedResult } from './course-api.model';

export type RankPeriod = 'week' | 'month' | 'all';

export interface RankingEntryApi {
  rank: number;
  userId: number;
  fullName: string;
  avatarUrl?: string;
  totalPoints: number;
  isMe: boolean;
}

export interface LeaderboardResponseApi {
  items: PagedResult<RankingEntryApi>;
  me?: RankingEntryApi;
}
