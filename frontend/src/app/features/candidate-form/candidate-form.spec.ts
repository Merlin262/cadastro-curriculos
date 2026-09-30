import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router, provideRouter } from '@angular/router';
import { vi } from 'vitest';
import { CandidateForm } from './candidate-form';
import { environment } from '../../../environments/environment';

describe('CandidateForm', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CandidateForm],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  function createComponent() {
    const fixture = TestBed.createComponent(CandidateForm);
    fixture.detectChanges();
    return fixture;
  }

  it('marks the form invalid when required fields are empty', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;

    expect(component.form.valid).toBe(false);
    expect(component.form.controls.fullName.hasError('required')).toBe(true);
    expect(component.form.controls.email.hasError('required')).toBe(true);
  });

  it('rejects an invalid e-mail format', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;

    component.form.controls.fullName.setValue('Maria Oliveira');
    component.form.controls.email.setValue('nao-e-um-email');

    expect(component.form.valid).toBe(false);
    expect(component.form.controls.email.hasError('email')).toBe(true);
  });

  it('is valid once the required fields are filled correctly', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;

    component.form.controls.fullName.setValue('Maria Oliveira');
    component.form.controls.email.setValue('maria@example.com');

    expect(component.form.valid).toBe(true);
  });

  it('rejects a selected file that is not a PDF', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;

    const file = new File(['conteudo'], 'curriculo.docx', { type: 'application/msword' });
    const input = document.createElement('input');
    input.type = 'file';
    Object.defineProperty(input, 'files', { value: [file] });

    component.onFileSelected({ target: input } as unknown as Event);

    expect(component.fileError()).toContain('PDF');
    expect(component.selectedFileName()).toBeNull();
  });

  it('rejects a PDF file larger than 5 MB', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;

    const oversizedContent = new Uint8Array(5 * 1024 * 1024 + 1);
    const file = new File([oversizedContent], 'curriculo.pdf', { type: 'application/pdf' });
    const input = document.createElement('input');
    input.type = 'file';
    Object.defineProperty(input, 'files', { value: [file] });

    component.onFileSelected({ target: input } as unknown as Event);

    expect(component.fileError()).toContain('5 MB');
    expect(component.selectedFileName()).toBeNull();
  });

  it('accepts a valid PDF within the size limit', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;

    const file = new File(['%PDF-1.4'], 'curriculo.pdf', { type: 'application/pdf' });
    const input = document.createElement('input');
    input.type = 'file';
    Object.defineProperty(input, 'files', { value: [file] });

    component.onFileSelected({ target: input } as unknown as Event);

    expect(component.fileError()).toBeNull();
    expect(component.selectedFileName()).toBe('curriculo.pdf');
  });

  it('warns (without blocking) when the e-mail is already registered', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;

    component.form.controls.email.setValue('maria@example.com');
    component.onEmailBlur();

    const req = httpMock.expectOne((r) => r.url === `${environment.apiUrl}/candidates/check-email`);
    req.flush({ exists: true });

    expect(component.emailAlreadyExists()).toBe(true);
    component.form.controls.fullName.setValue('Maria Oliveira');
    expect(component.form.valid).toBe(true);
  });

  it('clears the duplicate-e-mail warning once the e-mail is edited again', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;

    component.form.controls.email.setValue('maria@example.com');
    component.onEmailBlur();
    httpMock.expectOne((r) => r.url === `${environment.apiUrl}/candidates/check-email`).flush({ exists: true });
    expect(component.emailAlreadyExists()).toBe(true);

    component.form.controls.email.setValue('outra@example.com');

    expect(component.emailAlreadyExists()).toBe(false);
  });

  it('sends the selected resume file when saving after a PDF import', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;
    vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    const file = new File(['%PDF-1.4'], 'curriculo.pdf', { type: 'application/pdf' });
    const input = document.createElement('input');
    input.type = 'file';
    Object.defineProperty(input, 'files', { value: [file] });
    component.onFileSelected({ target: input } as unknown as Event);

    component.form.controls.fullName.setValue('Maria Oliveira');
    component.form.controls.email.setValue('maria@example.com');
    component.submit();

    const req = httpMock.expectOne(`${environment.apiUrl}/candidates`);
    const body = req.request.body as FormData;
    expect(body.get('resumeFile')).toBeTruthy();
    req.flush({ id: '1' });
  });
});
