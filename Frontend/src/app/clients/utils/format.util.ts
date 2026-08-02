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

export function formatChapterMeta(lessons: { durationMinutes: number }[]): string {
  const totalMinutes = lessons.reduce((sum, l) => sum + l.durationMinutes, 0);
  return `${lessons.length} bài · ${formatDuration(totalMinutes)}`;
}

/** "Thầy Nguyễn Văn An" -> "NA" — strips the honorific, then takes the first
 * letter of the first and last remaining words. */
export function formatTeacherInitials(name?: string): string {
  if (!name) return '?';
  const stripped = name.replace(/^(Thầy|Cô)\s+/i, '').trim();
  const parts = stripped.split(/\s+/).filter(Boolean);
  if (parts.length === 0) return '?';
  const first = parts[0][0] ?? '';
  const last = parts[parts.length - 1][0] ?? '';
  return (first + last).toUpperCase();
}
