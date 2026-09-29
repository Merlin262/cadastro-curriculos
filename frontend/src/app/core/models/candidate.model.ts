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
}

export interface CreateCandidateRequest {
  fullName: string;
  email: string;
  phone?: string | null;
  areaOfInterest?: string | null;
  professionalSummary?: string | null;
  source?: CandidateSource;
  resumeFileName?: string | null;
}

export interface ExtractedResumeData {
  textExtracted: boolean;
  fullName: string | null;
  email: string | null;
  phone: string | null;
  warnings: string[];
}
