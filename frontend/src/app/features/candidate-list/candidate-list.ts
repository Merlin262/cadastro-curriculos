import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { CandidatesService } from '../../core/services/candidates.service';
import { CandidateListItem } from '../../core/models/candidate.model';

@Component({
  selector: 'app-candidate-list',
  imports: [
    CommonModule,
    RouterLink,
    MatButtonModule,
    MatChipsModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
  ],
  templateUrl: './candidate-list.html',
  styleUrl: './candidate-list.scss',
})
export class CandidateList implements OnInit {
  private readonly candidatesService = inject(CandidatesService);

  readonly columns = ['fullName', 'email', 'phone', 'areaOfInterest', 'source', 'createdAtUtc', 'actions'];
  readonly candidates = signal<CandidateListItem[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadCandidates();
  }

  loadCandidates(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.candidatesService.getAll().subscribe({
      next: (candidates) => {
        this.candidates.set(candidates);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set(
          'Não foi possível carregar os candidatos. Verifique se a API está em execução e tente novamente.',
        );
        this.loading.set(false);
      },
    });
  }
}
