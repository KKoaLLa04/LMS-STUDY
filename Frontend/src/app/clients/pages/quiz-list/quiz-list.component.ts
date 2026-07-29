import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OcIconComponent } from '../../components/icon/icon.component';
import { StudentQuizLibraryService } from '../../services/student-quiz-library.service';
import { StudentQuizApi } from '../../models/quiz-api.model';

@Component({
  selector: 'app-quiz-list',
  standalone: true,
  imports: [RouterLink, OcIconComponent],
  templateUrl: './quiz-list.component.html',
  styleUrl: './quiz-list.component.scss',
})
export class QuizListComponent implements OnInit {
  private readonly quizLibraryService = inject(StudentQuizLibraryService);

  readonly quizzes = signal<StudentQuizApi[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.quizLibraryService.getAll().subscribe((items) => {
      this.quizzes.set(items);
      this.loading.set(false);
    });
  }

  animationDelayFor(index: number): number {
    return (index % 6) * 0.07;
  }
}
