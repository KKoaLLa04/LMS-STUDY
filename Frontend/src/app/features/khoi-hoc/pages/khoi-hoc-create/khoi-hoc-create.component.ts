import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { KhoiHocService } from '../../services/khoi-hoc.service';
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-khoi-hoc-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './khoi-hoc-create.component.html'
})
export class KhoiHocCreateComponent {
  form: FormGroup;
  submitting = false;

  constructor(
    private fb: FormBuilder,
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

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.toast.error('Vui lòng điền đầy đủ các trường bắt buộc.');
      return;
    }

    this.submitting = true;
    const v = this.form.value;

    this.khoiHocService
      .createKhoiHoc({
        name: v.name.trim(),
        code: v.code.trim(),
        orderNumber: v.orderNumber
      })
      .subscribe({
        next: (res) => {
          if (!res.success) {
            this.toast.error(res.message || 'Tạo khối học thất bại');
            this.submitting = false;
            return;
          }
          this.toast.success(`Tạo khối học "${v.name}" thành công!`);
          this.router.navigate(['/khoi-hoc']);
        },
        error: (err) => {
          this.toast.error(err?.error?.message || 'Đã xảy ra lỗi. Vui lòng thử lại.');
          this.submitting = false;
        }
      });
  }
}
