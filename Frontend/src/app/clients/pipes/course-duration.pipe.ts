import { Pipe, PipeTransform } from '@angular/core';
import { formatDuration } from '../utils/format.util';

@Pipe({ name: 'ocDuration', standalone: true })
export class CourseDurationPipe implements PipeTransform {
  transform(totalMinutes: number): string {
    return formatDuration(totalMinutes);
  }
}
