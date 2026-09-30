import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { CandidatesService } from '../../core/services/candidates.service';
import { CandidateListItem } from '../../core/models/candidate.model';

const SEARCH_DEBOUNCE_MS = 400;

@Component({
  selector: 'app-candidate-list',
  imports: [
    CommonModule,
    RouterLink,
    MatButtonModule,
    MatChipsModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatTableModule,
  ],
  templateUrl: './candidate-list.html',
  styleUrl: './candidate-list.scss',
})
export class CandidateList implements OnInit, OnDestroy {
  private readonly candidatesService = inject(CandidatesService);
  private searchDebounceHandle?: ReturnType<typeof setTimeout>;

  readonly columns = ['fullName', 'email', 'phone', 'areaOfInterest', 'source', 'createdAtUtc', 'actions'];
  readonly candidates = signal<CandidateListItem[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);
  readonly searchTerm = signal('');
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadCandidates();
  }

  ngOnDestroy(): void {
    clearTimeout(this.searchDebounceHandle);
  }

  onSearchInput(value: string): void {
    this.searchTerm.set(value);
    clearTimeout(this.searchDebounceHandle);
    this.searchDebounceHandle = setTimeout(() => {
      this.pageIndex.set(0);
      this.loadCandidates();
    }, SEARCH_DEBOUNCE_MS);
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.loadCandidates();
  }

  loadCandidates(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.candidatesService
      .getAll({
        search: this.searchTerm().trim() || undefined,
        page: this.pageIndex() + 1,
        pageSize: this.pageSize(),
      })
      .subscribe({
        next: (result) => {
          this.candidates.set(result.items);
          this.totalCount.set(result.totalCount);
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
