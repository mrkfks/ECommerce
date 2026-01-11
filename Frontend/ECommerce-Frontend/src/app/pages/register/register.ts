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

  formData: RegisterRequest = {
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
    username: '',
    phoneNumber: ''
  };

  isLoading = false;
  error: string | null = null;
  showPassword = false;
  acceptTerms = false;
  emailError: string | null = null;
  emailValid = false;
  emailChecking = false;
  usernameError: string | null = null;
  usernameValid = false;
  usernameChecking = false;

  onSubmit(): void {
    if (!this.formData.email || !this.formData.password || !this.formData.firstName || !this.formData.lastName || !this.formData.username || !this.formData.phoneNumber) {
      this.error = 'Lütfen tüm alanları doldurun.';
      return;
    }

    // Regex validations
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.formData.email)) {
      this.error = 'Geçerli bir email adresi girin.';
      return;
    }

    const usernameRegex = /^[a-zA-Z0-9_]{3,}$/;
    if (!usernameRegex.test(this.formData.username)) {
      this.error = 'Kullanıcı adı en az 3 karakter olmalı ve sadece harf, rakam ve alt çizgi içerebilir.';
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

    console.log('Submitting form data:', this.formData);
    this.authService.register(this.formData).subscribe({
      next: () => {
        console.log('Register successful');
        this.router.navigate(['/login']); // Redirect to login after registration
      },
      error: (err) => {
        console.error('Register error:', err);
        this.isLoading = false;

        // Backend returns GlobalExceptionHandlerMiddleware format: { status, title, detail, ... }
        // For 409 Conflict, typically means email or username already exists
        let errorMessage = 'Kayıt başarısız. Lütfen tekrar deneyin.';

        if (err.status === 409) {
          // Backend message usually is specific enough, e.g. "Bu email adresi zaten kullanılıyor."
          errorMessage = err.error?.detail || err.error?.message || 'Bu email adresi veya kullanıcı adı zaten kullanılıyor.';
        } else {
          errorMessage = err.error?.detail || err.error?.message || errorMessage;
        }

        this.error = errorMessage;
      }
    });
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  onEmailBlur(): void {
    if (!this.formData.email) {
      this.emailError = null;
      this.emailValid = false;
      return;
    }

    // Email format validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.formData.email)) {
      this.emailError = 'Geçerli bir email adresi girin';
      this.emailValid = false;
      return;
    }

    this.emailChecking = true;
    this.authService.checkEmailAvailable(this.formData.email).subscribe({
      next: (response) => {
        this.emailChecking = false;
        if (response.isAvailable) {
          this.emailError = null;
          this.emailValid = true;
        } else {
          this.emailError = 'Bu email adresi zaten kayıtlı. Başka bir email kullanın.';
          this.emailValid = false;
        }
      },
      error: () => {
        this.emailChecking = false;
        this.emailError = 'Email kontrol sırasında hata oluştu';
        this.emailValid = false;
      }
    });
  }

  onUsernameBlur(): void {
    if (!this.formData.username) {
      this.usernameError = null;
      this.usernameValid = false;
      return;
    }

    // Username format validation (alphanumeric and underscore only)
    const usernameRegex = /^[a-zA-Z0-9_]{3,}$/;
    if (!usernameRegex.test(this.formData.username)) {
      this.usernameError = 'Kullanıcı adı 3+ karakter olmalı (harf, rakam, alt çizgi)';
      this.usernameValid = false;
      return;
    }

    this.usernameChecking = true;
    this.authService.checkUsernameAvailable(this.formData.username).subscribe({
      next: (response) => {
        this.usernameChecking = false;
        if (response.isAvailable) {
          this.usernameError = null;
          this.usernameValid = true;
        } else {
          this.usernameError = 'Bu kullanıcı adı zaten kayıtlı. Başka bir ad seçin.';
          this.usernameValid = false;
        }
      },
      error: () => {
        this.usernameChecking = false;
        this.usernameError = 'Kullanıcı adı kontrol sırasında hata oluştu';
        this.usernameValid = false;
      }
    });
  }
}
