import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CourseService } from '../../services/course.service';
import { CourseListItem } from '../../models/course.model';

@Component({
  selector: 'app-course-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './course-list.component.html'
})
export class CourseListComponent implements OnInit {
  courses: CourseListItem[] = [];
  loading = false;
  deleting: number | null = null;
  errorMessage = '';
  successMessage = '';

  page = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  searchInput = '';
  keyword = '';

  constructor(private courseService: CourseService) {}

  ngOnInit(): void {
    this.loadCourses();
  }

  loadCourses(): void {
    this.loading = true;
    this.errorMessage = '';
    this.courseService
      .getCourses(this.page, this.pageSize, this.keyword || undefined)
      .subscribe({
        next: (res) => {
          this.courses = res.data?.items ?? [];
          this.totalCount = res.data?.totalCount ?? 0;
          this.totalPages = res.data?.totalPages ?? 0;
          this.loading = false;
        },
        error: () => {
          this.errorMessage = 'Không thể tải danh sách khóa học. Vui lòng thử lại.';
          this.loading = false;
        }
      });
  }

  onSearch(): void {
    this.keyword = this.searchInput.trim();
    this.page = 1;
    this.loadCourses();
  }

  onPageChange(newPage: number): void {
    if (newPage < 1 || newPage > this.totalPages) return;
    this.page = newPage;
    this.loadCourses();
  }

  onDelete(id: number, title: string): void {
    if (!confirm(`Bạn có chắc muốn xóa khóa học "${title}"?\nThao tác này sẽ xóa toàn bộ chương và bài học bên trong.`)) return;
    this.deleting = id;
    this.successMessage = '';
    this.errorMessage = '';
    this.courseService.deleteCourse(id).subscribe({
      next: () => {
        this.successMessage = `Đã xóa khóa học "${title}" thành công.`;
        this.deleting = null;
        if (this.courses.length === 1 && this.page > 1) this.page--;
        this.loadCourses();
      },
      error: () => {
        this.errorMessage = 'Xóa khóa học thất bại. Vui lòng thử lại.';
        this.deleting = null;
      }
    });
  }

  get pageNumbers(): number[] {
    const delta = 2;
    const start = Math.max(1, this.page - delta);
    const end = Math.min(this.totalPages, this.page + delta);
    return Array.from({ length: end - start + 1 }, (_, i) => start + i);
  }

  get startIndex(): number {
    return (this.page - 1) * this.pageSize + 1;
  }

  get endIndex(): number {
    return Math.min(this.page * this.pageSize, this.totalCount);
  }

  formatPrice(price: number): string {
    return price === 0 ? 'Miễn phí' : price.toLocaleString('vi-VN') + ' đ';
  }

  getStatusClass(status: string): string {
    return status === 'Published' ? 'text-bg-success' : 'text-bg-secondary';
  }

  getStatusLabel(status: string): string {
    return status === 'Published' ? 'Đã xuất bản' : 'Bản nháp';
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('vi-VN');
  }
}
