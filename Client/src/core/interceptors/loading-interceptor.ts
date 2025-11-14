import { HttpEvent, HttpInterceptorFn, HttpParams } from '@angular/common/http';
import { inject } from '@angular/core';
import { BusyService } from '../services/busy-service';
import { delay, finalize, of, tap } from 'rxjs';

// We want to cache GET requests. The string here will be the request URL
const cache = new Map<string, HttpEvent<unknown>>();

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const busyService = inject(BusyService);

  const generateCacheKey = (url: string, params: HttpParams): string => {
    const paramString = params
      .keys()
      .map((key) => `${key}=${params.get(key)}`)
      .join('&');
    return paramString ? `${url}?${paramString}` : url;
  };

  const cacheKey = generateCacheKey(req.url, req.params);

  // On each request here we are checking if it is a GET request. If so, we check if we have something in the cache and if we do we return the data in the cache and exit without calling to the server for data
  if (req.method === 'GET') {
    const cachedResponse = cache.get(cacheKey);
    if (cachedResponse) {
      // We need to return an Observable because this is an HttpInterceptor for HTTP requests made via HttpClient
      return of(cachedResponse);
    }
  }

  busyService.busy();
  return next(req).pipe(
    delay(500),
    tap((response) => {
      cache.set(cacheKey, response); // The response here is of type HttpEvent
    }),
    finalize(() => {
      busyService.idle();
    })
  );
};
