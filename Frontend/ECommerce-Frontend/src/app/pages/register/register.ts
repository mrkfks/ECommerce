import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services';
import { RegisterRequest } from '../../core/models';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private authService = inject(AuthService);
  private router = inject(Router);

  formData: RegisterRequest & { confirmPassword: string } = {
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
    username: ''
  };

  isLoading = false;
  error: string | null = null;
  showPassword = false;
  acceptTerms = false;

  onSubmit(): void {
    if (!this.formData.email || !this.formData.password || !this.formData.firstName || !this.formData.lastName || !this.formData.username) {
      this.error = 'Lütfen tüm alanları doldurun.';
      return;
    }

    if (this.formData.password !== this.formData.confirmPassword) {
      this.error = 'Şifreler eşleşmiyor.';
      return;
    }

    if (this.formData.password.length < 6) {
      this.error = 'Şifre en az 6 karakter olmalıdır.';
      return;
    }

    if (!this.acceptTerms) {
      this.error = 'Kullanım koşullarını kabul etmelisiniz.';
      return;
    }

    this.isLoading = true;
    this.error = null;

    const { confirmPassword, ...registerData } = this.formData;

    this.authService.register(registerData).subscribe({
      next: () => {
        this.router.navigate(['/home']);
      },
      error: (err) => {
        this.isLoading = false;
        this.error = err.error?.message || 'Kayıt başarısız. Lütfen tekrar deneyin.';
      }
    });
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }
}
