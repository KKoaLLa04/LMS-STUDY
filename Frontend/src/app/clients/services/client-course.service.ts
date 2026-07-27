import { Injectable } from '@angular/core';
import { Observable, delay, map, of } from 'rxjs';
import { Course, CourseMaterial, RatingBreakdownItem, Review, ThumbnailVariant } from '../models/course.model';
import { buildChapters } from '../utils/chapters.util';

export const CATEGORY_ALL = 'Tất cả';
export const CATEGORY_LIST = [CATEGORY_ALL, 'Toán', 'Vật Lý', 'Hóa Học', 'Ngữ Văn', 'Sinh Học', 'Tiếng Anh', 'Lịch Sử'];

interface RawCourse {
  id: number;
  subject: string;
  title: string;
  teacher: string;
  initials: string;
  rating: number;
  ratingCount: number;
  students: number;
  lessons: number;
  durationMinutes: number;
  price: number;
  purchased: boolean;
  progress: number;
  featured: boolean;
  desc: string;
}

const RAW_COURSES: RawCourse[] = [
  { id: 1, subject: 'Toán', title: 'Toán học lớp 10 - Đại số & Hình học', teacher: 'Thầy Nguyễn Văn An', initials: 'NA', rating: 4.8, ratingCount: 342, students: 1240, lessons: 32, durationMinutes: 18 * 60 + 20, price: 299000, purchased: true, progress: 64, featured: true, desc: 'Nắm vững kiến thức đại số và hình học lớp 10 qua các bài giảng trực quan, dễ hiểu.' },
  { id: 2, subject: 'Vật Lý', title: 'Vật Lý 11 - Điện học & Từ trường', teacher: 'Cô Trần Thị Bích', initials: 'TB', rating: 4.6, ratingCount: 198, students: 860, lessons: 28, durationMinutes: 15 * 60 + 40, price: 279000, purchased: false, progress: 0, featured: true, desc: 'Chinh phục điện học và từ trường với phương pháp giải nhanh, dễ nhớ.' },
  { id: 3, subject: 'Hóa Học', title: 'Hóa Học Vô Cơ 12 - Ôn thi THPT', teacher: 'Thầy Lê Minh Khoa', initials: 'LK', rating: 4.9, ratingCount: 512, students: 2100, lessons: 40, durationMinutes: 22 * 60 + 10, price: 349000, purchased: true, progress: 88, featured: true, desc: 'Tổng ôn hóa vô cơ trọng tâm, bám sát đề thi THPT Quốc gia.' },
  { id: 4, subject: 'Ngữ Văn', title: 'Ngữ Văn 9 - Luyện viết nghị luận xã hội', teacher: 'Cô Phạm Thu Hà', initials: 'PH', rating: 4.7, ratingCount: 266, students: 980, lessons: 24, durationMinutes: 12 * 60 + 30, price: 249000, purchased: false, progress: 0, featured: false, desc: 'Rèn kỹ năng viết văn nghị luận xã hội mạch lạc, giàu ý tưởng.' },
  { id: 5, subject: 'Sinh Học', title: 'Sinh Học 10 - Tế bào & Di truyền', teacher: 'Thầy Đỗ Quang Huy', initials: 'DH', rating: 4.5, ratingCount: 150, students: 610, lessons: 20, durationMinutes: 10 * 60 + 15, price: 0, purchased: true, progress: 40, featured: false, desc: 'Khám phá thế giới tế bào và các quy luật di truyền cơ bản.' },
  { id: 6, subject: 'Tiếng Anh', title: 'Tiếng Anh Giao Tiếp Cho Học Sinh THCS', teacher: 'Cô Vũ Ngọc Lan', initials: 'VL', rating: 4.9, ratingCount: 730, students: 3400, lessons: 36, durationMinutes: 19 * 60, price: 329000, purchased: false, progress: 0, featured: false, desc: 'Tự tin giao tiếp tiếng Anh mỗi ngày với các tình huống thực tế.' },
  { id: 7, subject: 'Lịch Sử', title: 'Lịch Sử Việt Nam Cận Đại', teacher: 'Thầy Hoàng Anh Tuấn', initials: 'HT', rating: 4.4, ratingCount: 98, students: 410, lessons: 18, durationMinutes: 9 * 60 + 45, price: 199000, purchased: false, progress: 0, featured: false, desc: 'Hành trình lịch sử Việt Nam từ thế kỷ 19 đến đầu thế kỷ 20.' },
  { id: 8, subject: 'Toán', title: 'Luyện Thi Học Sinh Giỏi Toán 9', teacher: 'Thầy Nguyễn Văn An', initials: 'NA', rating: 5.0, ratingCount: 87, students: 320, lessons: 45, durationMinutes: 26 * 60, price: 399000, purchased: false, progress: 0, featured: false, desc: 'Bộ đề chuyên sâu luyện thi học sinh giỏi Toán lớp 9.' },
  { id: 9, subject: 'Hóa Học', title: 'Hóa Học Hữu Cơ Cơ Bản', teacher: 'Thầy Lê Minh Khoa', initials: 'LK', rating: 4.6, ratingCount: 210, students: 740, lessons: 22, durationMinutes: 11 * 60 + 50, price: 0, purchased: true, progress: 100, featured: false, desc: 'Làm chủ kiến thức hóa hữu cơ nền tảng, dễ tiếp cận.' },
];

const MATERIAL_TEMPLATES: Array<{ name: string; sizeLabel: string }> = [
  { name: 'Tài liệu tổng hợp lý thuyết.pdf', sizeLabel: '2.4 MB' },
  { name: 'Bộ đề bài tập tự luyện.pdf', sizeLabel: '1.1 MB' },
  { name: 'Slide bài giảng.pptx', sizeLabel: '3.8 MB' },
];

const REVIEW_TEMPLATES: Array<{ name: string; initials: string; rating: number; daysAgo: number; comment: string }> = [
  { name: 'Minh Anh', initials: 'MA', rating: 5, daysAgo: 14, comment: 'Bài giảng rất dễ hiểu, thầy cô giảng chậm và rõ ràng, em theo kịp ngay từ buổi đầu.' },
  { name: 'Gia Bảo', initials: 'GB', rating: 5, daysAgo: 30, comment: 'Khóa học có nhiều bài tập thực hành, giúp em nắm chắc kiến thức hơn hẳn.' },
  { name: 'Thảo Vy', initials: 'TV', rating: 4, daysAgo: 32, comment: 'Nội dung hay, chỉ mong có thêm phần tóm tắt cuối mỗi chương.' },
];

const RATING_BREAKDOWN: RatingBreakdownItem[] = [
  { stars: 5, percent: 68 },
  { stars: 4, percent: 22 },
  { stars: 3, percent: 7 },
  { stars: 2, percent: 2 },
  { stars: 1, percent: 1 },
];

function toCourse(raw: RawCourse, index: number): Course {
  const now = Date.now();
  const materials: CourseMaterial[] = MATERIAL_TEMPLATES.map((m, i) => ({ id: i + 1, ...m }));
  const reviews: Review[] = REVIEW_TEMPLATES.map((r, i) => ({
    id: i + 1,
    studentName: r.name,
    initials: r.initials,
    rating: r.rating,
    createdAt: new Date(now - r.daysAgo * 24 * 3600 * 1000),
    comment: r.comment,
  }));

  return {
    id: raw.id,
    subject: raw.subject,
    title: raw.title,
    description: raw.desc,
    teacher: raw.teacher,
    teacherInitials: raw.initials,
    rating: raw.rating,
    ratingCount: raw.ratingCount,
    studentsCount: raw.students,
    lessonsCount: raw.lessons,
    durationMinutes: raw.durationMinutes,
    price: raw.price,
    purchased: raw.purchased,
    progressPercent: raw.progress,
    featured: raw.featured,
    thumbnailVariant: (index % 2 === 0 ? 'accent' : 'accent-2') as ThumbnailVariant,
    chapters: buildChapters(raw.lessons, raw.progress, raw.purchased),
    materials,
    reviews,
    ratingBreakdown: RATING_BREAKDOWN,
  };
}

const COURSES: Course[] = RAW_COURSES.map(toCourse);

/**
 * Client-facing course catalogue. Mock data for now — swap the bodies below
 * for HttpClient calls once the student-facing API exists; callers only see
 * Observable<Course[] | Course>, so nothing downstream needs to change.
 */
@Injectable({ providedIn: 'root' })
export class ClientCourseService {
  getCategories(): string[] {
    return CATEGORY_LIST;
  }

  getCourses(): Observable<Course[]> {
    return of(COURSES).pipe(delay(150));
  }

  getCourseById(id: number): Observable<Course | undefined> {
    return this.getCourses().pipe(map((courses) => courses.find((c) => c.id === id)));
  }
}
