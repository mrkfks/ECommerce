import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../core/services';
import { User } from '../../core/models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit {
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
    phone: ''
  };

  passwordForm = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  ngOnInit(): void {
    this.user = this.authService.currentUserValue;
    if (!this.user) {
      // Mock user for demo
      this.user = {
        id: 1,
        email: 'demo@example.com',
        firstName: 'Demo',
        lastName: 'Kullanıcı',
        role: 'user',
        createdAt: new Date()
      };
    }
    this.resetForm();
  }

  resetForm(): void {
    if (this.user) {
      this.editForm = {
        firstName: this.user.firstName,
        lastName: this.user.lastName,
        email: this.user.email,
        phone: ''
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
    this.isSaving = true;
    // Simülasyon
    setTimeout(() => {
      if (this.user) {
        this.user.firstName = this.editForm.firstName;
        this.user.lastName = this.editForm.lastName;
        this.user.email = this.editForm.email;
      }
      this.isSaving = false;
      this.isEditing = false;
    }, 1000);
  }

  changePassword(): void {
    if (this.passwordForm.newPassword !== this.passwordForm.confirmPassword) {
      alert('Şifreler eşleşmiyor!');
      return;
    }
    // Şifre değiştirme işlemi
    alert('Şifre başarıyla değiştirildi!');
    this.passwordForm = { currentPassword: '', newPassword: '', confirmPassword: '' };
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/home']);
  }
}
