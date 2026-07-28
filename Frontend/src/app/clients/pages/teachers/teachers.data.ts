import { OcIconName } from '../../models/icon-name.type';

export type SubjectKey = 'math' | 'physics' | 'chem' | 'lit' | 'eng' | 'bio';

export interface SubjectMeta {
  key: SubjectKey;
  name: string;
  icon: OcIconName;
  iconBg: string;
  /** Secondary hero tag shown next to the subject on the teacher's profile. */
  specialty: string;
  teachingStyle: string;
  /** Short topic suffixes used to synthesize this teacher's course titles. */
  courseTopics: [string, string];
}

export const SUBJECTS: SubjectMeta[] = [
  {
    key: 'math',
    name: 'Toán',
    icon: 'math',
    iconBg: 'var(--oc-accent-500)',
    specialty: 'Luyện thi HSG',
    teachingStyle: 'Dễ hiểu, gần gũi, nhiều ví dụ thực tế',
    courseTopics: ['Đại số & Hình học', 'Luyện thi học sinh giỏi'],
  },
  {
    key: 'physics',
    name: 'Vật Lý',
    icon: 'atom',
    iconBg: 'var(--oc-accent-2-500)',
    specialty: 'Ôn thi THPT',
    teachingStyle: 'Trực quan, gắn liền thí nghiệm thực tế',
    courseTopics: ['Điện học & Từ trường', 'Cơ học nâng cao'],
  },
  {
    key: 'chem',
    name: 'Hóa Học',
    icon: 'flask',
    iconBg: 'var(--oc-accent-600)',
    specialty: 'Thí nghiệm thực hành',
    teachingStyle: 'Minh hoạ bằng thí nghiệm, dễ nhớ lâu',
    courseTopics: ['Hoá học vô cơ', 'Hoá học hữu cơ'],
  },
  {
    key: 'lit',
    name: 'Ngữ Văn',
    icon: 'book-open',
    iconBg: 'var(--oc-accent-2-600)',
    specialty: 'Luyện viết văn hay',
    teachingStyle: 'Truyền cảm hứng, giàu cảm xúc',
    courseTopics: ['Nghị luận xã hội', 'Cảm thụ văn học'],
  },
  {
    key: 'eng',
    name: 'Tiếng Anh',
    icon: 'globe',
    iconBg: 'var(--oc-accent-2-400)',
    specialty: 'Giao tiếp tự nhiên',
    teachingStyle: 'Tương tác nhiều, phát âm chuẩn',
    courseTopics: ['Giao tiếp cơ bản', 'Luyện thi chứng chỉ quốc tế'],
  },
  {
    key: 'bio',
    name: 'Sinh Học',
    icon: 'leaf',
    iconBg: 'var(--oc-accent-400)',
    specialty: 'Khám phá tự nhiên',
    teachingStyle: 'Trực quan, liên hệ đời sống thực tế',
    courseTopics: ['Tế bào & Di truyền', 'Sinh thái học'],
  },
];

export function subjectMeta(key: SubjectKey): SubjectMeta {
  return SUBJECTS.find((s) => s.key === key) ?? SUBJECTS[0];
}

export interface TeacherRecord {
  id: number;
  name: string;
  honorific: 'Thầy' | 'Cô';
  subjectKey: SubjectKey;
  rating: number;
  coursesCount: number;
  experienceYears: number;
}

/** Mock data for now — will be swapped for a real teachers endpoint once the
 * backend exposes an instructors list. */
export const TEACHERS: TeacherRecord[] = [
  { id: 1, name: 'Nguyễn Văn An', honorific: 'Thầy', subjectKey: 'math', rating: 4.8, coursesCount: 18, experienceYears: 12 },
  { id: 2, name: 'Trần Thị Bích', honorific: 'Cô', subjectKey: 'physics', rating: 4.6, coursesCount: 12, experienceYears: 9 },
  { id: 3, name: 'Lê Minh Khoa', honorific: 'Thầy', subjectKey: 'chem', rating: 4.9, coursesCount: 21, experienceYears: 14 },
  { id: 4, name: 'Phạm Thu Hà', honorific: 'Cô', subjectKey: 'lit', rating: 4.7, coursesCount: 9, experienceYears: 7 },
  { id: 5, name: 'Vũ Ngọc Lan', honorific: 'Cô', subjectKey: 'eng', rating: 4.9, coursesCount: 26, experienceYears: 11 },
  { id: 6, name: 'Đỗ Quang Huy', honorific: 'Thầy', subjectKey: 'bio', rating: 4.5, coursesCount: 8, experienceYears: 6 },
  { id: 7, name: 'Hoàng Anh Tuấn', honorific: 'Thầy', subjectKey: 'math', rating: 4.7, coursesCount: 14, experienceYears: 10 },
  { id: 8, name: 'Ngô Bảo Châu', honorific: 'Thầy', subjectKey: 'physics', rating: 4.8, coursesCount: 11, experienceYears: 9 },
  { id: 9, name: 'Trịnh Thu Trang', honorific: 'Cô', subjectKey: 'chem', rating: 4.6, coursesCount: 10, experienceYears: 8 },
  { id: 10, name: 'Bùi Gia Bảo', honorific: 'Thầy', subjectKey: 'lit', rating: 4.8, coursesCount: 13, experienceYears: 7 },
  { id: 11, name: 'Lý Khánh Linh', honorific: 'Cô', subjectKey: 'eng', rating: 4.9, coursesCount: 22, experienceYears: 12 },
  { id: 12, name: 'Đặng Việt Hoàng', honorific: 'Thầy', subjectKey: 'bio', rating: 4.5, coursesCount: 7, experienceYears: 6 },
];

export function findTeacher(id: number): TeacherRecord | undefined {
  return TEACHERS.find((t) => t.id === id);
}

/** Students and reviews aren't tracked by the backend yet, so these are
 * derived from rating/course count to stay roughly proportionate. */
export function studentsCountOf(t: TeacherRecord): number {
  return Math.round(t.coursesCount * 650 + t.rating * 200);
}

export function reviewsCountOf(t: TeacherRecord): number {
  return Math.round(t.coursesCount * 19 + t.rating * 5);
}

export function fullNameOf(t: TeacherRecord): string {
  return `${t.honorific} ${t.name}`;
}

export function degreeOf(t: TeacherRecord): string {
  return `Thạc sĩ Sư phạm ${subjectMeta(t.subjectKey).name}`;
}

export function bioOf(t: TeacherRecord): string {
  const subject = subjectMeta(t.subjectKey);
  return `${fullNameOf(t)} có hơn ${t.experienceYears} năm kinh nghiệm giảng dạy ${subject.name}, giúp hàng nghìn học sinh chinh phục các kỳ thi quan trọng với phương pháp học ${subject.teachingStyle.toLowerCase()}.`;
}

export interface TaughtCourse {
  title: string;
  rating: number;
  price: number;
}

export function coursesOf(t: TeacherRecord): TaughtCourse[] {
  const subject = subjectMeta(t.subjectKey);
  const basePrice = 249000 + (t.id % 5) * 20000;
  return subject.courseTopics.map((topic, i) => ({
    title: `${subject.name} - ${topic}`,
    rating: Math.min(5, Math.round((t.rating - i * 0.1) * 10) / 10),
    price: basePrice + i * 50000,
  }));
}

export interface TeacherReview {
  name: string;
  date: string;
  rating: number;
  comment: string;
}

const REVIEW_POOL: TeacherReview[] = [
  { name: 'Minh Anh', date: '2 tuần trước', rating: 5, comment: 'Thầy/cô giảng rất dễ hiểu, em tiến bộ rõ rệt chỉ sau vài tuần học.' },
  { name: 'Gia Bảo', date: '1 tháng trước', rating: 5, comment: 'Phương pháp dạy gần gũi, luôn kiên nhẫn giải đáp thắc mắc.' },
  { name: 'Thảo Vy', date: '1 tháng trước', rating: 4, comment: 'Nội dung khoá học rất chất lượng, mong ra thêm nhiều bài tập.' },
  { name: 'Đức Huy', date: '2 tháng trước', rating: 5, comment: 'Bài giảng sinh động, dễ theo dõi, không hề nhàm chán.' },
  { name: 'Ngọc Hân', date: '3 tuần trước', rating: 5, comment: 'Em rất thích cách giảng dạy, học xong hiểu bài ngay.' },
  { name: 'Quang Minh', date: '2 tháng trước', rating: 4, comment: 'Giảng viên nhiệt tình, tài liệu ôn tập rất đầy đủ.' },
];

export function reviewsOf(t: TeacherRecord): TeacherReview[] {
  return [0, 1, 2].map((offset) => REVIEW_POOL[(t.id + offset) % REVIEW_POOL.length]);
}
