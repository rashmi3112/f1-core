import { Component, inject, Input } from '@angular/core';
import { Product } from '../../../shared/models/product';
import { MatCardModule } from '@angular/material/card';
import { CurrencyPipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-product-item',
  standalone: true,
  imports: [
    MatCardModule,
    CurrencyPipe,
    MatIconModule,
    RouterLink,
    MatButtonModule
  ],
  templateUrl: './product-item.component.html',
  styleUrl: './product-item.component.scss'
})
export class ProductItemComponent {
  @Input() product?: Product;
  cartService = inject(CartService);
}
