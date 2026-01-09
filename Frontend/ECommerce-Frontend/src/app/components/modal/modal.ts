import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './modal.html',
  styleUrl: './modal.css',
})
export class Modal {
  @Input() title: string = 'Modal Başlığı';
  @Input() size: 'sm' | 'md' | 'lg' | 'xl' = 'md';
  @Input() showFooter: boolean = true;
  @Input() confirmText: string = 'Onayla';
  @Input() cancelText: string = 'İptal';
  @Input() confirmButtonClass: string = 'btn-primary';
  
  @Output() onConfirm = new EventEmitter<void>();
  @Output() onCancel = new EventEmitter<void>();
  @Output() onClose = new EventEmitter<void>();

  isOpen: boolean = false;

  open(): void {
    this.isOpen = true;
    document.body.classList.add('modal-open');
  }

  close(): void {
    this.isOpen = false;
    document.body.classList.remove('modal-open');
    this.onClose.emit();
  }

  confirm(): void {
    this.onConfirm.emit();
    this.close();
  }

  cancel(): void {
    this.onCancel.emit();
    this.close();
  }

  onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal')) {
      this.cancel();
    }
  }

  get modalSizeClass(): string {
    const sizes: Record<string, string> = {
      'sm': 'modal-sm',
      'md': '',
      'lg': 'modal-lg',
      'xl': 'modal-xl'
    };
    return sizes[this.size] || '';
  }
}
