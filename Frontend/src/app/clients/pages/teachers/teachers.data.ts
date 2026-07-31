import { OcIconName } from '../../models/icon-name.type';
import { Course } from '../../models/course.model';

/** Minimal shape returned by the real backend (Backend/DTOs/UserDto.cs → PublicUserDto). */
export interface PublicTeacher {
  id: number;
  fullName: string;
  avatarUrl?: string;
  gender?: string;
  /** Real fields, admin-entered via Edit User form — null until an admin fills them in. */
  subject?: string;
  experienceYears?: number;
  bio?: string;
}

export interface TeacherRecord {
  id: number;
  name: string;
  honorific: 'Thầy' | 'Cô';
  subject: string | null;
  experienceYears: number | null;
  bio: string | null;
  /** Weighted average of this teacher's real Course.rating (by ratingCount), 0 if none rated yet. */
  rating: number;
  ratingCount: number;
  coursesCount: number;
  studentsCount: number;
}

export interface SubjectIconMeta {
  icon: OcIconName;
  bg: string;
}

// Icon/màu tra theo tên môn dạy thật (Subject là chuỗi tự do admin nhập, không phải enum) —
// thuần là lựa chọn trình bày, môn chưa có trong bảng tra dùng icon mặc định.
const SUBJECT_ICONS: Record<string, SubjectIconMeta> = {
  'Toán': { icon: 'math', bg: 'var(--oc-accent-500)' },
  'Vật Lý': { icon: 'atom', bg: 'var(--oc-accent-2-500)' },
  'Hóa Học': { icon: 'flask', bg: 'var(--oc-accent-600)' },
  'Ngữ Văn': { icon: 'book-open', bg: 'var(--oc-accent-2-600)' },
  'Sinh Học': { icon: 'leaf', bg: 'var(--oc-accent-400)' },
  'Tiếng Anh': { icon: 'globe', bg: 'var(--oc-accent-2-400)' },
  'Lịch Sử': { icon: 'clock', bg: 'var(--oc-accent-700)' },
  'Địa Lý': { icon: 'map-pin', bg: 'var(--oc-accent-2-700)' },
};
const DEFAULT_SUBJECT_ICON: SubjectIconMeta = { icon: 'graduation-cap', bg: 'var(--oc-neutral-500)' };

export function subjectIcon(subject?: string | null): SubjectIconMeta {
  return (subject ? SUBJECT_ICONS[subject] : undefined) ?? DEFAULT_SUBJECT_ICON;
}

/** Real courses taught by this teacher, matched by Course.Teacher === PublicTeacher.fullName
 * (backend doesn't have a Course→Teacher foreign key, only the free-text name column). */
export function coursesOfTeacher(fullName: string, courses: Course[]): Course[] {
  return courses.filter((c) => c.teacher === fullName);
}

/** Builds a teacher profile from the real PublicUser fields plus real Course stats —
 * rating/coursesCount/studentsCount are computed from the teacher's actual courses
 * (already fetched via ClientCourseService), not derived/fabricated. */
export function buildTeacherRecord(pu: PublicTeacher, courses: Course[]): TeacherRecord {
  const honorific: 'Thầy' | 'Cô' =
    pu.gender === 'Male' ? 'Thầy' : pu.gender === 'Female' ? 'Cô' : pu.id % 2 === 0 ? 'Thầy' : 'Cô';

  const own = coursesOfTeacher(pu.fullName, courses);
  const studentsCount = own.reduce((sum, c) => sum + c.studentsCount, 0);
  const ratingCount = own.reduce((sum, c) => sum + c.ratingCount, 0);
  const weightedSum = own.reduce((sum, c) => sum + c.rating * c.ratingCount, 0);
  const rating = ratingCount > 0 ? Math.round((weightedSum / ratingCount) * 10) / 10 : 0;

  return {
    id: pu.id,
    name: pu.fullName,
    honorific,
    subject: pu.subject ?? null,
    experienceYears: pu.experienceYears ?? null,
    bio: pu.bio ?? null,
    rating,
    ratingCount,
    coursesCount: own.length,
    studentsCount,
  };
}

export function fullNameOf(t: TeacherRecord): string {
  return `${t.honorific} ${t.name}`;
}
