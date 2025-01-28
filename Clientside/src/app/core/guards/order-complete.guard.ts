import { CanActivateFn, Router } from '@angular/router';
import { OrdersService } from '../services/orders.service';
import { inject } from '@angular/core';

export const orderCompleteGuard: CanActivateFn = (route, state) => {
  const orderService = inject(OrdersService);
  const router = inject(Router);

  if (orderService.orderComplete) {
    return true;
  } else {
    router.navigateByUrl('/shop');
    return false;
  }
};
