import { Pipe, PipeTransform } from '@angular/core';
import { formatStudentsCount } from '../utils/format.util';

@Pipe({ name: 'ocStudentsCount', standalone: true })
export class StudentsCountPipe implements PipeTransform {
  transform(count: number): string {
    return formatStudentsCount(count);
  }
}
