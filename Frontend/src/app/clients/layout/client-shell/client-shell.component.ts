import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ClientHeaderComponent } from '../../components/client-header/client-header.component';
import { ClientFooterComponent } from '../../components/client-footer/client-footer.component';
import { ClientCourseService } from '../../services/client-course.service';

@Component({
  selector: 'app-oc-shell',
  standalone: true,
  imports: [RouterOutlet, ClientHeaderComponent, ClientFooterComponent],
  templateUrl: './client-shell.component.html',
})
export class ClientShellComponent {
  protected readonly courseService = inject(ClientCourseService);
}
