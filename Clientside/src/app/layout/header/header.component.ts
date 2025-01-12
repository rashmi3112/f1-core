import { Component, inject } from '@angular/core';
import {MatIconModule} from '@angular/material/icon';
import {MatButtonModule} from '@angular/material/button';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { BusyService } from '../../core/services/busy.service';
import { MatProgressSpinnerModule} from '@angular/material/progress-spinner'
import { MatBadgeModule } from '@angular/material/badge'
import { CartService } from '../../core/services/cart.service';
import { AccountService } from '../../core/services/account.service';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    MatIconModule,
    MatButtonModule,
    RouterLink,
    RouterLinkActive,
    MatProgressSpinnerModule,
    MatBadgeModule,
    MatMenuModule,
    MatDividerModule

  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {
  busyService = inject(BusyService);
  cartService = inject(CartService);
  accountService = inject(AccountService);
  private router = inject(Router);

  logout() {
    this.accountService.logout().subscribe({
      next: () => {
        this.accountService.currentUser.set(null);
        this.router.navigateByUrl('/');
      }
    })
  }

}
