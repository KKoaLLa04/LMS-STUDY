import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { KhoiHocService } from '../../services/khoi-hoc.service';
import { KhoiHoc } from '../../models/khoi-hoc.model';
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-khoi-hoc-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './khoi-hoc-list.component.html'
})
export class KhoiHocListComponent implements OnInit {
  khoiHocs: KhoiHoc[] = [];
  loading = false;
  deleting: number | null = null;
  errorMessage = '';

  constructor(
    private khoiHocService: KhoiHocService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.loadKhoiHocs();
  }

  loadKhoiHocs(): void {
    this.loading = true;
    this.errorMessage = '';
    this.khoiHocService.getKhoiHocs().subscribe({
      next: (res) => {
        this.khoiHocs = res.data ?? [];
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Không thể tải danh sách khối học. Vui lòng thử lại.';
        this.loading = false;
      }
    });
  }

  onDelete(id: number, name: string): void {
    if (!confirm(`Bạn có chắc muốn xóa khối học "${name}"?`)) return;
    this.deleting = id;
    this.khoiHocService.deleteKhoiHoc(id).subscribe({
      next: () => {
        this.toast.success(`Đã xóa khối học "${name}" thành công.`);
        this.deleting = null;
        this.loadKhoiHocs();
      },
      error: (err) => {
        this.toast.error(err?.error?.message || 'Xóa khối học thất bại. Vui lòng thử lại.');
        this.deleting = null;
      }
    });
  }
}
