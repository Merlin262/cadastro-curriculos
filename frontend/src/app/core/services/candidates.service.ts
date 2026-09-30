import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Candidate,
  CandidatesQueryParams,
  CreateCandidateRequest,
  ExtractedResumeData,
  PagedResult,
  CandidateListItem,
} from '../models/candidate.model';

@Injectable({ providedIn: 'root' })
export class CandidatesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/candidates`;

  getAll(query: CandidatesQueryParams): Observable<PagedResult<CandidateListItem>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);
    if (query.search) {
      params = params.set('search', query.search);
    }
    return this.http.get<PagedResult<CandidateListItem>>(this.baseUrl, { params });
  }

  getById(id: string): Observable<Candidate> {
    return this.http.get<Candidate>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateCandidateRequest, resumeFile: File | null): Observable<Candidate> {
    const formData = new FormData();
    formData.append('fullName', request.fullName);
    formData.append('email', request.email);
    if (request.phone) formData.append('phone', request.phone);
    if (request.areaOfInterest) formData.append('areaOfInterest', request.areaOfInterest);
    if (request.professionalSummary) formData.append('professionalSummary', request.professionalSummary);
    formData.append('source', request.source ?? 'Manual');
    if (resumeFile) {
      formData.append('resumeFile', resumeFile, resumeFile.name);
    }
    return this.http.post<Candidate>(this.baseUrl, formData);
  }

  extractResume(file: File): Observable<ExtractedResumeData> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ExtractedResumeData>(`${this.baseUrl}/extract-resume`, formData);
  }

  checkEmailExists(email: string): Observable<{ exists: boolean }> {
    const params = new HttpParams().set('email', email);
    return this.http.get<{ exists: boolean }>(`${this.baseUrl}/check-email`, { params });
  }

  downloadResume(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${id}/resume`, { responseType: 'blob' });
  }
}
