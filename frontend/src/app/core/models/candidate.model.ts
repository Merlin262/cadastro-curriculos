export type CandidateSource = 'Manual' | 'Pdf';

export interface CandidateListItem {
  id: string;
  fullName: string;
  email: string;
  phone: string | null;
  areaOfInterest: string | null;
  source: CandidateSource;
  createdAtUtc: string;
}

export interface Candidate extends CandidateListItem {
  professionalSummary: string | null;
  resumeFileName: string | null;
  hasResumeFile: boolean;
}

export interface CreateCandidateRequest {
  fullName: string;
  email: string;
  phone?: string | null;
  areaOfInterest?: string | null;
  professionalSummary?: string | null;
  source?: CandidateSource;
}

export interface ExtractedResumeData {
  textExtracted: boolean;
  fullName: string | null;
  email: string | null;
  phone: string | null;
  warnings: string[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CandidatesQueryParams {
  search?: string;
  page: number;
  pageSize: number;
}
