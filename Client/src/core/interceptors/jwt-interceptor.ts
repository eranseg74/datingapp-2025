import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AccountService } from '../services/account-service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const accountService = inject(AccountService);
  // When assigning from a signal we lose the reactivity and the assigned propery is no longer a signal but rather the returned object (could be also null). Here the currentUser() is a signal but the user will be a User or null
  const user = accountService.currentUser();

  // Modifying a clone of the request and sending it to the next function
  if (user) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${user.token}`,
      },
    });
  }
  return next(req);
};
