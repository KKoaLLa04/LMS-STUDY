import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OcIconComponent } from '../../components/icon/icon.component';
import { StudentDocumentService } from '../../services/student-document.service';
import { StudentDocumentApi } from '../../models/document-api.model';

@Component({
  selector: 'app-document-list',
  standalone: true,
  imports: [RouterLink, OcIconComponent],
  templateUrl: './document-list.component.html',
  styleUrl: './document-list.component.scss',
})
export class DocumentListComponent implements OnInit {
  private readonly documentService = inject(StudentDocumentService);

  readonly documents = signal<StudentDocumentApi[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.documentService.getAll().subscribe((items) => {
      this.documents.set(items);
      this.loading.set(false);
    });
  }

  animationDelayFor(index: number): number {
    return (index % 6) * 0.07;
  }
}
