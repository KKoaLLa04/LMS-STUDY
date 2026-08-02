import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/auth/auth.service';
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  username = '';
  password = '';
  loading = false;
  errorMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private toast: ToastService
  ) {}

  onSubmit(): void {
    if (!this.username.trim() || !this.password) {
      this.errorMessage = 'Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.authService.login(this.username.trim(), this.password).subscribe({
      next: (res) => {
        this.loading = false;
        this.toast.success(res.message || 'Đăng nhập thành công');

        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        if (returnUrl) {
          this.router.navigateByUrl(returnUrl);
          return;
        }

        const role = res.data?.role?.toLowerCase();
        this.router.navigateByUrl(role === 'admin' || role === 'teacher' ? '/dashboard' : '/');
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Tên đăng nhập hoặc mật khẩu không đúng.';
      }
    });
  }
}
