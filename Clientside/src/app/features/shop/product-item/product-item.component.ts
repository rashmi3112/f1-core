import { Component, Input } from '@angular/core';
import { Product } from '../../../shared/models/product';
import { MatCardModule } from '@angular/material/card';
import { CurrencyPipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-product-item',
  standalone: true,
  imports: [
    MatCardModule,
    CurrencyPipe,
    MatIconModule
  ],
  templateUrl: './product-item.component.html',
  styleUrl: './product-item.component.scss'
})
export class ProductItemComponent {
  @Input() product?: Product;
}
