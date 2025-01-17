import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-checkout-success',
  standalone: true,
  imports: [
    MatButtonModule,
    RouterLink
  ],
  templateUrl: './checkout-success.component.html',
  styleUrl: './checkout-success.component.scss'
})
export class CheckoutSuccessComponent {

}
