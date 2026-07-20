import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { KhoiHocService } from '../../services/khoi-hoc.service';
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-khoi-hoc-edit',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './khoi-hoc-edit.component.html'
})
export class KhoiHocEditComponent implements OnInit {
  form: FormGroup;
  khoiHocId!: number;
  loading = true;
  submitting = false;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private khoiHocService: KhoiHocService,
    private router: Router,
    private toast: ToastService
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(255)]],
      code: ['', [Validators.required, Validators.maxLength(50)]],
      orderNumber: [0, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.khoiHocId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadKhoiHoc();
  }

  loadKhoiHoc(): void {
    this.loading = true;
    this.khoiHocService.getKhoiHocById(this.khoiHocId).subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.toast.error(res.message || 'Không tìm thấy khối học');
          this.router.navigate(['/khoi-hoc']);
          return;
        }
        this.form.patchValue({
          name: res.data.name,
          code: res.data.code,
          orderNumber: res.data.orderNumber
        });
        this.loading = false;
      },
      error: () => {
        this.toast.error('Không thể tải thông tin khối học.');
        this.router.navigate(['/khoi-hoc']);
      }
    });
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.toast.error('Vui lòng điền đầy đủ các trường bắt buộc.');
      return;
    }

    this.submitting = true;
    const v = this.form.value;

    this.khoiHocService
      .updateKhoiHoc(this.khoiHocId, {
        name: v.name.trim(),
        code: v.code.trim(),
        orderNumber: v.orderNumber
      })
      .subscribe({
        next: (res) => {
          if (!res.success) {
            this.toast.error(res.message || 'Cập nhật khối học thất bại');
            this.submitting = false;
            return;
          }
          this.toast.success(`Cập nhật khối học "${v.name}" thành công!`);
          this.router.navigate(['/khoi-hoc']);
        },
        error: (err) => {
          this.toast.error(err?.error?.message || 'Đã xảy ra lỗi. Vui lòng thử lại.');
          this.submitting = false;
        }
      });
  }
}
