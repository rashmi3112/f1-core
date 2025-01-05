import { Component, inject, OnInit } from '@angular/core';
import { ShopService } from '../../core/services/shop.service';
import { Product } from '../../shared/models/product';
import { MatCardModule } from '@angular/material/card'
import { ProductItemComponent } from "./product-item/product-item.component";
import { MatDialog,MatDialogModule} from '@angular/material/dialog'
import { FiltersDialogComponent } from './filters-dialog/filters-dialog.component';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule, MatMenuTrigger } from '@angular/material/menu';
import { MatListModule, MatSelectionList, MatSelectionListChange } from '@angular/material/list';
import { ShopParams } from '../../shared/models/shopParams';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { Pagination } from '../../shared/models/pagination';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-shop',
  standalone: true,
  imports: [
    MatCardModule,
    ProductItemComponent,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatListModule,
    MatPaginatorModule,
    FormsModule
],
  templateUrl: './shop.component.html',
  styleUrl: './shop.component.scss'
})
export class ShopComponent implements OnInit{
  private shopService = inject(ShopService)
  private dialogService = inject(MatDialog)
  products?: Pagination<Product>;
  sortOptions = [
    {name: 'Alphbetical', value:'name'},
    {name: 'Price: Low-High', value:'priceAsc'},
    {name: 'Price: High-Low', value:'priceDesc'}
  ]

  shopParams = new ShopParams();
  pageSizeOptions = [5,10,15,20];


  ngOnInit(): void {
    this.initializeShop();
  }

  initializeShop() {
    this.shopService.getBrands();
    this.shopService.getTypes();
    this.getProducts();
  }

  getProducts() {
    this.shopService.getProducts(this.shopParams).subscribe({
      next: response => this.products = response,
      error: error => console.log(error)
    })
  }

  onSearchChange() {
    this.shopParams.pageNumber = 1;
    this.getProducts();
  }

  handlePageEvent(event: PageEvent) {
    this.shopParams.pageNumber = event.pageIndex + 1;
    this.shopParams.pageSize = event.pageSize;
    this.getProducts();
  }

  onSortChange(event: MatSelectionListChange) {
    const selectedOption = event.options[0];
    if (selectedOption) {
      this.shopParams.sort = selectedOption.value;
      this.shopParams.pageNumber = 1;
      this.getProducts();
    }
  }

  openFiltersDialog() {
    const dialogRef = this.dialogService.open(FiltersDialogComponent, {
      minWidth: '500px',
      data: {
        selectedBrands: this.shopParams.brands,
        selectedTypes: this.shopParams.types
      }
    });
    dialogRef.afterClosed().subscribe({
      next: result => {
        if (result) {
          console.log(result);
          this.shopParams.brands = result.selectedBrands;
          this.shopParams.types = result.selectedTypes;
          this.shopParams.pageNumber = 1;
          this.getProducts();
        }
      }
    })
  }
}