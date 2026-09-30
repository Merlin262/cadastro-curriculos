import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { vi } from 'vitest';
import { CandidateList } from './candidate-list';
import { environment } from '../../../environments/environment';

describe('CandidateList', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CandidateList],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  function createComponent() {
    const fixture = TestBed.createComponent(CandidateList);
    fixture.detectChanges();
    return fixture;
  }

  it('loads the first page on init', () => {
    createComponent();

    const req = httpMock.expectOne(
      (r) => r.url === `${environment.apiUrl}/candidates` && r.params.get('page') === '1' && r.params.get('pageSize') === '10',
    );
    req.flush({ items: [{ id: '1', fullName: 'Maria' }], totalCount: 1, page: 1, pageSize: 10 });
  });

  it('debounces search input and resets to the first page', () => {
    vi.useFakeTimers();
    try {
      const fixture = createComponent();
      const component = fixture.componentInstance;
      httpMock.expectOne((r) => r.url === `${environment.apiUrl}/candidates`).flush({ items: [], totalCount: 0, page: 1, pageSize: 10 });

      component.pageIndex.set(2);
      component.onSearchInput('maria');

      httpMock.expectNone((r) => r.url === `${environment.apiUrl}/candidates`);
      vi.advanceTimersByTime(400);

      const req = httpMock.expectOne((r) => r.url === `${environment.apiUrl}/candidates`);
      expect(req.request.params.get('search')).toBe('maria');
      expect(req.request.params.get('page')).toBe('1');
      req.flush({ items: [], totalCount: 0, page: 1, pageSize: 10 });
    } finally {
      vi.useRealTimers();
    }
  });

  it('reloads with the new page/pageSize on page change', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;
    httpMock.expectOne((r) => r.url === `${environment.apiUrl}/candidates`).flush({ items: [], totalCount: 30, page: 1, pageSize: 10 });

    component.onPageChange({ pageIndex: 1, pageSize: 10, length: 30 });

    const req = httpMock.expectOne(
      (r) => r.url === `${environment.apiUrl}/candidates` && r.params.get('page') === '2',
    );
    req.flush({ items: [], totalCount: 30, page: 2, pageSize: 10 });
  });

  it('shows a friendly error message when loading fails', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;

    httpMock.expectOne((r) => r.url === `${environment.apiUrl}/candidates`).flush(
      { message: 'error' },
      { status: 500, statusText: 'Server Error' },
    );

    expect(component.errorMessage()).toContain('Não foi possível carregar');
  });
});
