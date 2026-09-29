import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CandidatesService } from '../../core/services/candidates.service';
import { Candidate } from '../../core/models/candidate.model';

@Component({
  selector: 'app-candidate-details',
  imports: [
    CommonModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './candidate-details.html',
  styleUrl: './candidate-details.scss',
})
export class CandidateDetails implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly candidatesService = inject(CandidatesService);

  readonly candidate = signal<Candidate | null>(null);
  readonly loading = signal(true);
  readonly notFound = signal(false);
  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.notFound.set(true);
      this.loading.set(false);
      return;
    }

    this.candidatesService.getById(id).subscribe({
      next: (candidate) => {
        this.candidate.set(candidate);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        if (err.status === 404) {
          this.notFound.set(true);
        } else {
          this.errorMessage.set('Não foi possível carregar os dados do candidato.');
        }
      },
    });
  }
}
