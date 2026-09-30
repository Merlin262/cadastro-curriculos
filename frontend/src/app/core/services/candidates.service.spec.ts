import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { CandidatesService } from './candidates.service';
import { environment } from '../../../environments/environment';

describe('CandidatesService', () => {
  let service: CandidatesService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(CandidatesService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('lists candidates from GET /candidates with paging params', () => {
    let result: unknown;
    service.getAll({ page: 1, pageSize: 10 }).subscribe((res) => (result = res));

    const req = httpMock.expectOne(
      (r) => r.url === `${environment.apiUrl}/candidates` && r.params.get('page') === '1' && r.params.get('pageSize') === '10',
    );
    expect(req.request.method).toBe('GET');
    const paged = { items: [{ id: '1', fullName: 'Maria' }], totalCount: 1, page: 1, pageSize: 10 };
    req.flush(paged);

    expect(result).toEqual(paged);
  });

  it('includes the search param when provided', () => {
    service.getAll({ search: 'maria', page: 1, pageSize: 10 }).subscribe();

    const req = httpMock.expectOne((r) => r.params.get('search') === 'maria');
    expect(req.request.method).toBe('GET');
    req.flush({ items: [], totalCount: 0, page: 1, pageSize: 10 });
  });

  it('fetches a single candidate by id', () => {
    service.getById('abc-123').subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/candidates/abc-123`);
    expect(req.request.method).toBe('GET');
    req.flush({ id: 'abc-123' });
  });

  it('creates a candidate as multipart form data, including the optional resume file', () => {
    const payload = { fullName: 'Maria', email: 'maria@example.com' };
    const file = new File(['%PDF-1.4'], 'curriculo.pdf', { type: 'application/pdf' });
    service.create(payload, file).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/candidates`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body instanceof FormData).toBe(true);
    const body = req.request.body as FormData;
    expect(body.get('fullName')).toBe('Maria');
    expect(body.get('email')).toBe('maria@example.com');
    expect(body.get('resumeFile')).toBeTruthy();
    req.flush({ id: '1', ...payload });
  });

  it('creates a candidate without a resume file when none is provided', () => {
    const payload = { fullName: 'Maria', email: 'maria@example.com' };
    service.create(payload, null).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/candidates`);
    const body = req.request.body as FormData;
    expect(body.get('resumeFile')).toBeNull();
    req.flush({ id: '1', ...payload });
  });

  it('sends the PDF file as multipart form data to extract-resume', () => {
    const file = new File(['%PDF-1.4'], 'curriculo.pdf', { type: 'application/pdf' });
    service.extractResume(file).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/candidates/extract-resume`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body instanceof FormData).toBe(true);
    req.flush({ textExtracted: true, fullName: null, email: null, phone: null, warnings: [] });
  });

  it('checks whether an e-mail already exists', () => {
    let result: { exists: boolean } | undefined;
    service.checkEmailExists('maria@example.com').subscribe((res) => (result = res));

    const req = httpMock.expectOne((r) => r.url === `${environment.apiUrl}/candidates/check-email`);
    expect(req.request.params.get('email')).toBe('maria@example.com');
    req.flush({ exists: true });

    expect(result).toEqual({ exists: true });
  });

  it('downloads the resume as a blob', () => {
    service.downloadResume('abc-123').subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/candidates/abc-123/resume`);
    expect(req.request.method).toBe('GET');
    expect(req.request.responseType).toBe('blob');
    req.flush(new Blob(['%PDF-1.4']));
  });
});
