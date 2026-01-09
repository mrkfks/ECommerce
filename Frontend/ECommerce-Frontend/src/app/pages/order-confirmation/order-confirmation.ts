import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'app-order-confirmation',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './order-confirmation.html',
  styleUrl: './order-confirmation.css',
})
export class OrderConfirmation implements OnInit {
  private route = inject(ActivatedRoute);
  
  orderId: string = '';
  orderDate: Date = new Date();
  estimatedDelivery: Date = new Date();

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.orderId = params['orderId'] || 'ORD-' + Math.floor(Math.random() * 1000000);
    });
    
    this.estimatedDelivery.setDate(this.estimatedDelivery.getDate() + 3);
  }
}
