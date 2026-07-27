import { Pipe, PipeTransform } from '@angular/core';
import { formatPrice } from '../utils/format.util';

@Pipe({ name: 'ocPrice', standalone: true })
export class CoursePricePipe implements PipeTransform {
  transform(price: number): string {
    return formatPrice(price);
  }
}
