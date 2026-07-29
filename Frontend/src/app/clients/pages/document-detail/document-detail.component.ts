import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OcIconComponent } from '../../components/icon/icon.component';
import { StudentDocumentService } from '../../services/student-document.service';
import { StudentDocumentApi } from '../../models/document-api.model';

@Component({
  selector: 'app-document-detail',
  standalone: true,
  imports: [RouterLink, OcIconComponent],
  templateUrl: './document-detail.component.html',
  styleUrl: './document-detail.component.scss',
})
export class DocumentDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly documentService = inject(StudentDocumentService);

  readonly document = signal<StudentDocumentApi | null>(null);
  readonly loading = signal(true);
  readonly notFound = signal(false);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.documentService.getById(id).subscribe((doc) => {
      this.document.set(doc);
      this.notFound.set(!doc);
      this.loading.set(false);
    });
  }
}
