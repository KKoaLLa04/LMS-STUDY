import { OcIconName } from '../../models/icon-name.type';

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

/** Mock data for now — will be swapped for a real achievements endpoint once
 * the backend tracks badge unlocks per student. Shared between the
 * achievements page and the student profile's "Thành tích đã đạt" summary. */
export const BADGES: Badge[] = [
  { id: 'b1', category: 'hoctap', name: 'Mọt sách nhỏ', desc: 'Hoàn thành 10 bài học', icon: 'book-open', unlocked: true },
  { id: 'b2', category: 'hoctap', name: 'Siêu trí tuệ', desc: 'Đạt điểm tuyệt đối 5 bài kiểm tra', icon: 'target', unlocked: true },
  { id: 'b3', category: 'hoctap', name: 'Bậc thầy kiến thức', desc: 'Hoàn thành 5 khoá học', icon: 'trophy', unlocked: false, progress: 60 },
  { id: 'b4', category: 'chuyencan', name: 'Lửa nhiệt huyết', desc: 'Học liên tục 7 ngày', icon: 'flame', unlocked: true },
  { id: 'b5', category: 'chuyencan', name: 'Kiên trì bền bỉ', desc: 'Học liên tục 30 ngày', icon: 'flame', unlocked: false, progress: 23 },
  { id: 'b6', category: 'chuyencan', name: 'Dậy sớm học bài', desc: 'Học trước 7 giờ sáng 5 lần', icon: 'target', unlocked: false, progress: 40 },
  { id: 'b7', category: 'tuongtac', name: 'Người bạn tốt', desc: 'Bình luận giúp đỡ 10 bạn học', icon: 'message-circle', unlocked: true },
  { id: 'b8', category: 'tuongtac', name: 'Trái tim vàng', desc: 'Nhận 20 lượt cảm ơn', icon: 'heart', unlocked: false, progress: 75 },
  { id: 'b9', category: 'tuongtac', name: 'Người truyền cảm hứng', desc: 'Được 100 bạn theo dõi', icon: 'message-circle', unlocked: false, progress: 12 },
];
