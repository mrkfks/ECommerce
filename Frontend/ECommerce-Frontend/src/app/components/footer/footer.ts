import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DesignService } from '../../core/services';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './footer.html',
  styleUrl: './footer.css',
})
export class Footer {
  designService = inject(DesignService);
  currentYear = new Date().getFullYear();
}
