import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { CandidateForm } from './candidate-form';

describe('CandidateForm', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CandidateForm],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
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
});
