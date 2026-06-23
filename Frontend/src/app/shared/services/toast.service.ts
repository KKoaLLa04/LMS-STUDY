import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface Toast {
  id: number;
  message: string;
  type: ToastType;
  duration: number;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private _counter = 0;
  readonly toasts$ = new Subject<Toast>();

  show(message: string, type: ToastType = 'success', duration = 3500): void {
    this.toasts$.next({ id: ++this._counter, message, type, duration });
  }

  success(message: string, duration?: number): void { this.show(message, 'success', duration); }
  error(message: string, duration?: number): void   { this.show(message, 'error',   duration); }
  warning(message: string, duration?: number): void { this.show(message, 'warning', duration); }
  info(message: string, duration?: number): void    { this.show(message, 'info',    duration); }
}
