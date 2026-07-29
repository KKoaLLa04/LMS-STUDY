import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OcIconComponent } from '../../components/icon/icon.component';
import { QuizPlayerComponent } from '../../components/quiz-player/quiz-player.component';
import { StudentQuizLibraryService } from '../../services/student-quiz-library.service';
import { StudentQuizApi } from '../../models/quiz-api.model';

@Component({
  selector: 'app-quiz-detail',
  standalone: true,
  imports: [RouterLink, OcIconComponent, QuizPlayerComponent],
  templateUrl: './quiz-detail.component.html',
  styleUrl: './quiz-detail.component.scss',
})
export class QuizDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly quizLibraryService = inject(StudentQuizLibraryService);

  readonly quiz = signal<StudentQuizApi | null>(null);
  readonly loading = signal(true);
  readonly notFound = signal(false);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.quizLibraryService.getById(id).subscribe((quiz) => {
      this.quiz.set(quiz);
      this.notFound.set(!quiz);
      this.loading.set(false);
    });
  }
}
