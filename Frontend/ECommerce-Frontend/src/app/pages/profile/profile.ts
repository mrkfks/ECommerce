import { CommonModule } from '@angular/common';
import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ChangePasswordRequest, User, UserProfileUpdateRequest } from '../../core/models';
import { AuthService } from '../../core/services';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private router = inject(Router);

  user: User | null = null;
  isEditing = false;
  isSaving = false;
  activeTab = 'profile';

  editForm = {
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    address: '',
    city: '',       // İl
    district: '',   // İlçe
    postalCode: '',
    country: ''
  };

  passwordForm = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.authService.getCurrentUser().pipe(takeUntil(this.destroy$)).subscribe({
      next: (user) => {
        this.user = user;
        this.resetForm();
      },
      error: () => {
        // Fallback or redirect if token is invalid but guard didn't catch it
        // Should rely on Interceptors/Guard mostly.
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  resetForm(): void {
    if (this.user) {
      this.editForm = {
        firstName: this.user.firstName,
        lastName: this.user.lastName,
        email: this.user.email,
        phone: this.user.phone || '',
        address: this.user.address || '',
        city: this.user.city || '',
        district: this.user.state || '',  // İlçe
        postalCode: this.user.postalCode || '',
        country: this.user.country || ''
      };
    }
  }

  toggleEdit(): void {
    if (this.isEditing) {
      this.resetForm();
    }
    this.isEditing = !this.isEditing;
  }

  saveProfile(): void {
    if (!this.user) return;
    this.isSaving = true;

    const request: UserProfileUpdateRequest = {
      firstName: this.editForm.firstName,
      lastName: this.editForm.lastName,
      email: this.editForm.email,
      username: this.user.username,
      phone: this.editForm.phone,
      address: this.editForm.address,
      city: this.editForm.city,          // İl
      state: this.editForm.district,      // İlçe
      postalCode: this.editForm.postalCode,
      country: this.editForm.country || 'Türkiye'
    };

    this.authService.updateProfile(request).pipe(takeUntil(this.destroy$)).subscribe({
      next: (updatedUser) => {
        this.user = updatedUser;
        this.isSaving = false;
        this.isEditing = false;
        alert('Profil bilgileriniz güncellendi.');
      },
      error: (err) => {
        this.isSaving = false;
        alert('Güncelleme başarısız: ' + (err.error?.message || 'Bilinmeyen hata'));
      }
    });
  }

  changePassword(): void {
    if (this.passwordForm.newPassword !== this.passwordForm.confirmPassword) {
      alert('Şifreler eşleşmiyor!');
      return;
    }

    const request: ChangePasswordRequest = {
      currentPassword: this.passwordForm.currentPassword,
      newPassword: this.passwordForm.newPassword,
      confirmPassword: this.passwordForm.confirmPassword
    };

    this.authService.changePassword(request).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        alert('Şifreniz başarıyla değiştirildi.');
        this.passwordForm = { currentPassword: '', newPassword: '', confirmPassword: '' };
      },
      error: (err) => {
        alert('Şifre değiştirme başarısız: ' + (err.error?.message || 'Bilinmeyen hata'));
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/home']);
  }
}
