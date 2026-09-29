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

  it('lists candidates from GET /candidates', () => {
    let result: unknown;
    service.getAll().subscribe((res) => (result = res));

    const req = httpMock.expectOne(`${environment.apiUrl}/candidates`);
    expect(req.request.method).toBe('GET');
    req.flush([{ id: '1', fullName: 'Maria' }]);

    expect(result).toEqual([{ id: '1', fullName: 'Maria' }]);
  });

  it('fetches a single candidate by id', () => {
    service.getById('abc-123').subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/candidates/abc-123`);
    expect(req.request.method).toBe('GET');
    req.flush({ id: 'abc-123' });
  });

  it('creates a candidate with POST /candidates', () => {
    const payload = { fullName: 'Maria', email: 'maria@example.com' };
    service.create(payload).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/candidates`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
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
});
