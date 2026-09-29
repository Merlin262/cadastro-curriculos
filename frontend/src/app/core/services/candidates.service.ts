import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Candidate,
  CandidateListItem,
  CreateCandidateRequest,
  ExtractedResumeData,
} from '../models/candidate.model';

@Injectable({ providedIn: 'root' })
export class CandidatesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/candidates`;

  getAll(): Observable<CandidateListItem[]> {
    return this.http.get<CandidateListItem[]>(this.baseUrl);
  }

  getById(id: string): Observable<Candidate> {
    return this.http.get<Candidate>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateCandidateRequest): Observable<Candidate> {
    return this.http.post<Candidate>(this.baseUrl, request);
  }

  extractResume(file: File): Observable<ExtractedResumeData> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ExtractedResumeData>(`${this.baseUrl}/extract-resume`, formData);
  }
}
