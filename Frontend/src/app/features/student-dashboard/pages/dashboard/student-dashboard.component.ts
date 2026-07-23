import { Component } from '@angular/core';
import { trigger, transition, style, animate, query, stagger } from '@angular/animations';
import { StatCardComponent } from '../../components/stat-card/stat-card.component';
import { CourseRowComponent } from '../../components/course-row/course-row.component';
import { DeadlineItemComponent } from '../../components/deadline-item/deadline-item.component';
import { ActivityItemComponent } from '../../components/activity-item/activity-item.component';
import { IconComponent } from '../../components/icon/icon.component';
import { Activity, Course, Deadline, Stat } from '../../models/student-dashboard.model';
import { MOCK_STUDENT } from '../../data/mock-student.data';

const listStagger = trigger('listStagger', [
  transition('* => *', [
    query(
      ':enter',
      [
        style({ opacity: 0, transform: 'translateY(6px)' }),
        stagger('60ms', [animate('220ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))])
      ],
      { optional: true }
    )
  ])
]);

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [
    StatCardComponent,
    CourseRowComponent,
    DeadlineItemComponent,
    ActivityItemComponent,
    IconComponent
  ],
  templateUrl: './student-dashboard.component.html',
  styleUrl: './student-dashboard.component.scss',
  animations: [listStagger]
})
export class StudentDashboardComponent {
  readonly student = MOCK_STUDENT;
  readonly greeting = `${this.greetingPrefix(new Date())}, ${this.student.name}! 👋`;

  readonly stats: Stat[] = [
    {
      icon: 'book',
      value: '6',
      label: 'Khóa học đang học',
      note: '+2 mới',
      gradientFrom: '#67E8F9',
      gradientTo: '#0891B2',
      noteColor: '#0891B2',
      noteBg: '#ECFEFF'
    },
    {
      icon: 'droplet',
      value: `${this.student.streakDays} ngày`,
      label: 'Chuỗi ngày học',
      note: 'Tuyệt vời!',
      gradientFrom: '#FDBA74',
      gradientTo: '#EA580C',
      noteColor: '#C2410C',
      noteBg: '#FFF7ED'
    },
    {
      icon: 'star',
      value: '2,450',
      label: 'Điểm thưởng (XP)',
      note: `Cấp ${this.student.level}`,
      gradientFrom: '#A78BFA',
      gradientTo: '#6366F1',
      noteColor: '#4338CA',
      noteBg: '#EEF2FF'
    },
    {
      icon: 'award',
      value: '9',
      label: 'Huy hiệu đạt được',
      note: '2 mới',
      gradientFrom: '#6EE7B7',
      gradientTo: '#059669',
      noteColor: '#047857',
      noteBg: '#ECFDF5'
    }
  ];

  readonly courses: Course[] = [
    {
      emoji: '🔢',
      subjectBg: '#EEF2FF',
      name: 'Toán học - Số và phép tính',
      teacher: 'Nguyễn Thị Lan',
      nextLesson: 'Bài 5: Phép nhân',
      progress: 78,
      progressColor: '#6366F1',
      status: 'active',
      statusLabel: 'Đang học',
      statusBg: '#ECFDF5',
      statusColor: '#047857',
      actionLabel: 'Tiếp tục',
      actionBg: '#EEF2FF',
      actionColor: '#4338CA'
    },
    {
      emoji: '📖',
      subjectBg: '#FEF3C7',
      name: 'Tiếng Việt - Tập đọc',
      teacher: 'Trần Văn Minh',
      nextLesson: 'Bài 8: Ông lão đánh cá',
      progress: 45,
      progressColor: '#F59E0B',
      status: 'active',
      statusLabel: 'Đang học',
      statusBg: '#ECFDF5',
      statusColor: '#047857',
      actionLabel: 'Tiếp tục',
      actionBg: '#EEF2FF',
      actionColor: '#4338CA'
    },
    {
      emoji: '🌱',
      subjectBg: '#ECFDF5',
      name: 'Khoa học - Khám phá thiên nhiên',
      teacher: 'Lê Thị Hoa',
      nextLesson: 'Chương 2: Cây xanh',
      progress: 15,
      progressColor: '#10B981',
      status: 'upcoming',
      statusLabel: 'Sắp tới',
      statusBg: '#FEF9C3',
      statusColor: '#A16207',
      actionLabel: 'Xem trước',
      actionBg: '#FEF9C3',
      actionColor: '#A16207'
    },
    {
      emoji: '🗣️',
      subjectBg: '#FEE2E2',
      name: 'Tiếng Anh vui - Unit 3',
      teacher: 'Sarah Johnson',
      nextLesson: 'Ôn tập cuối chương',
      progress: 92,
      progressColor: '#F43F5E',
      status: 'active',
      statusLabel: 'Đang học',
      statusBg: '#ECFDF5',
      statusColor: '#047857',
      actionLabel: 'Tiếp tục',
      actionBg: '#EEF2FF',
      actionColor: '#4338CA'
    },
    {
      emoji: '🎨',
      subjectBg: '#ECFEFF',
      name: 'Nghệ thuật - Vẽ và tô màu',
      teacher: 'Phạm Thu An',
      nextLesson: 'Chưa bắt đầu',
      progress: 0,
      progressColor: '#94A3B8',
      status: 'draft',
      statusLabel: 'Bản nháp',
      statusBg: '#F1F5F9',
      statusColor: '#64748B',
      actionLabel: 'Bắt đầu',
      actionBg: '#F1F5F9',
      actionColor: '#64748B'
    }
  ];

  readonly deadlines: Deadline[] = [
    { title: 'Bài tập Toán: Ôn tập phép nhân', due: 'Ngày mai, 17:00', dotColor: '#F43F5E' },
    { title: 'Bài kiểm tra Tiếng Việt', due: 'Thứ 6, 08:00', dotColor: '#F59E0B' },
    { title: 'Nộp bài vẽ tranh', due: 'Tuần sau', dotColor: '#0891B2' }
  ];

  readonly activities: Activity[] = [
    {
      icon: 'check',
      iconColor: '#059669',
      iconBg: '#ECFDF5',
      iconStrokeWidth: 2.4,
      text: `${this.student.name} đã hoàn thành Bài 5 Toán học`,
      time: '5 phút trước'
    },
    {
      icon: 'award',
      iconColor: '#EA580C',
      iconBg: '#FFF7ED',
      iconStrokeWidth: 2,
      text: 'Nhận được huy hiệu "Ngôi sao chăm chỉ"',
      time: '1 giờ trước'
    },
    {
      icon: 'message-circle',
      iconColor: '#0891B2',
      iconBg: '#ECFEFF',
      iconStrokeWidth: 2,
      text: 'Cô Lan đã nhận xét bài tập của bạn',
      time: '3 giờ trước'
    },
    {
      icon: 'book',
      iconColor: '#4338CA',
      iconBg: '#EEF2FF',
      iconStrokeWidth: 2,
      text: 'Bạn đã bắt đầu khóa Nghệ thuật',
      time: 'Hôm qua'
    }
  ];

  private greetingPrefix(date: Date): string {
    const hour = date.getHours();
    if (hour < 12) return 'Chào buổi sáng';
    if (hour < 18) return 'Chào buổi chiều';
    return 'Chào buổi tối';
  }
}
