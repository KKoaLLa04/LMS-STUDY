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
  isFollowing: boolean;
  khoiHocId?: number;
  gradeName?: string;
  /** Rank at the equivalent previous period (previous week/month, or 7 days ago for "all") —
   * computed live from PointTransactions history, not a stored snapshot. Null if this user had
   * no point activity in the previous window (nothing meaningful to compare against). */
  previousRank?: number;
}

export interface LeaderboardResponseApi {
  items: PagedResult<RankingEntryApi>;
  me?: RankingEntryApi;
}
