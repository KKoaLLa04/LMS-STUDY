import { Pipe, PipeTransform } from '@angular/core';
import { formatRelativeDate } from '../utils/format.util';

@Pipe({ name: 'ocRelativeDate', standalone: true })
export class RelativeDatePipe implements PipeTransform {
  transform(date: Date): string {
    return formatRelativeDate(date);
  }
}
