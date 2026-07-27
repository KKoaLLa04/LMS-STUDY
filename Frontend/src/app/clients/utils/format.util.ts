/** Pure formatting helpers shared by the client course pipes. Kept free of
 * Angular so they can be unit-tested and reused outside a template context. */

export function formatPrice(price: number): string {
  if (price <= 0) return 'Miễn phí';
  return `${price.toLocaleString('vi-VN')}đ`;
}

export function formatDuration(totalMinutes: number): string {
  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;
  const paddedMinutes = minutes < 10 ? `0${minutes}` : `${minutes}`;
  return hours > 0 ? `${hours}h ${paddedMinutes}p` : `${minutes}p`;
}

export function formatStudentsCount(count: number): string {
  if (count >= 1000) {
    const thousands = (count / 1000).toFixed(1).replace('.0', '');
    return `${thousands}k học viên`;
  }
  return `${count} học viên`;
}

const RELATIVE_UNITS: Array<{ unit: string; seconds: number }> = [
  { unit: 'năm', seconds: 365 * 24 * 3600 },
  { unit: 'tháng', seconds: 30 * 24 * 3600 },
  { unit: 'tuần', seconds: 7 * 24 * 3600 },
  { unit: 'ngày', seconds: 24 * 3600 },
  { unit: 'giờ', seconds: 3600 },
  { unit: 'phút', seconds: 60 },
];

export function formatRelativeDate(date: Date, now: Date = new Date()): string {
  const diffSeconds = Math.max(0, (now.getTime() - date.getTime()) / 1000);
  for (const { unit, seconds } of RELATIVE_UNITS) {
    const value = Math.floor(diffSeconds / seconds);
    if (value >= 1) return `${value} ${unit} trước`;
  }
  return 'Vừa xong';
}

export function formatChapterMeta(lessonCount: number): string {
  const estimatedMinutes = Math.round(lessonCount * 11);
  return `${lessonCount} bài · ${estimatedMinutes} phút`;
}
