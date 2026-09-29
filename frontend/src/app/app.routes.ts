import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/candidate-list/candidate-list').then((m) => m.CandidateList),
  },
  {
    path: 'novo',
    loadComponent: () =>
      import('./features/candidate-form/candidate-form').then((m) => m.CandidateForm),
  },
  {
    path: 'candidatos/:id',
    loadComponent: () =>
      import('./features/candidate-details/candidate-details').then((m) => m.CandidateDetails),
  },
  { path: '**', redirectTo: '' },
];
