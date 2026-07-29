import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { OcIconComponent } from '../../components/icon/icon.component';
import { AuthService } from '../../../core/auth/auth.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [FormsModule, RouterLink, OcIconComponent],
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.scss',
})
export class ChangePasswordComponent {
  private readonly authService = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  currentPassword = '';
  newPassword = '';
  confirmPassword = '';

  readonly loading = signal(false);
  readonly errorMessage = signal('');

  onSubmit(): void {
    this.errorMessage.set('');

    if (!this.currentPassword || !this.newPassword || !this.confirmPassword) {
      this.errorMessage.set('Vui lòng nhập đầy đủ thông tin.');
      return;
    }
    if (this.newPassword.length < 6) {
      this.errorMessage.set('Mật khẩu mới phải có ít nhất 6 ký tự.');
      return;
    }
    if (this.newPassword !== this.confirmPassword) {
      this.errorMessage.set('Mật khẩu xác nhận không khớp.');
      return;
    }

    this.loading.set(true);
    this.authService.changePassword(this.currentPassword, this.newPassword).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.toast.success(res.message || 'Đổi mật khẩu thành công');
        this.router.navigate(['/client/ho-so']);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message || 'Đã xảy ra lỗi khi đổi mật khẩu.');
      },
    });
  }
}
