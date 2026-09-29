import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CandidatesService } from '../../core/services/candidates.service';
import { CandidateSource } from '../../core/models/candidate.model';

const MAX_FILE_SIZE_BYTES = 5 * 1024 * 1024;

@Component({
  selector: 'app-candidate-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './candidate-form.html',
  styleUrl: './candidate-form.scss',
})
export class CandidateForm {
  private readonly fb = inject(FormBuilder);
  private readonly candidatesService = inject(CandidatesService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  readonly form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.maxLength(200)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(200)]],
    phone: ['', [Validators.maxLength(30)]],
    areaOfInterest: ['', [Validators.maxLength(150)]],
    professionalSummary: ['', [Validators.maxLength(4000)]],
  });

  readonly selectedFileName = signal<string | null>(null);
  readonly fileError = signal<string | null>(null);
  readonly extracting = signal(false);
  readonly extractionDone = signal(false);
  readonly extractionWarnings = signal<string[]>([]);
  readonly submitting = signal(false);
  readonly submitError = signal<string | null>(null);

  private selectedFile: File | null = null;
  private source: CandidateSource = 'Manual';
  private resumeFileName: string | null = null;

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.fileError.set(null);
    this.extractionWarnings.set([]);
    this.extractionDone.set(false);

    if (!file) {
      this.selectedFile = null;
      this.selectedFileName.set(null);
      return;
    }

    if (!file.name.toLowerCase().endsWith('.pdf')) {
      this.fileError.set('Apenas arquivos PDF são aceitos.');
      this.selectedFile = null;
      this.selectedFileName.set(null);
      input.value = '';
      return;
    }

    if (file.size > MAX_FILE_SIZE_BYTES) {
      this.fileError.set('O arquivo deve ter no máximo 5 MB.');
      this.selectedFile = null;
      this.selectedFileName.set(null);
      input.value = '';
      return;
    }

    this.selectedFile = file;
    this.selectedFileName.set(file.name);
  }

  extractFromPdf(): void {
    if (!this.selectedFile) {
      return;
    }

    this.extracting.set(true);
    this.fileError.set(null);
    this.extractionWarnings.set([]);

    const file = this.selectedFile;

    this.candidatesService.extractResume(file).subscribe({
      next: (result) => {
        this.extracting.set(false);
        this.extractionDone.set(true);
        this.extractionWarnings.set(result.warnings);
        this.source = 'Pdf';
        this.resumeFileName = file.name;

        if (result.fullName) {
          this.form.controls.fullName.setValue(result.fullName);
        }
        if (result.email) {
          this.form.controls.email.setValue(result.email);
        }
        if (result.phone) {
          this.form.controls.phone.setValue(result.phone);
        }
      },
      error: (err) => {
        this.extracting.set(false);
        this.fileError.set(this.extractServerMessage(err) ?? 'Não foi possível processar o PDF. Você ainda pode preencher o formulário manualmente.');
      },
    });
  }

  removeFile(): void {
    this.selectedFile = null;
    this.selectedFileName.set(null);
    this.fileError.set(null);
    this.extractionWarnings.set([]);
    this.extractionDone.set(false);
    this.source = 'Manual';
    this.resumeFileName = null;
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.submitError.set(null);

    const value = this.form.getRawValue();

    this.candidatesService
      .create({
        fullName: value.fullName,
        email: value.email,
        phone: value.phone || null,
        areaOfInterest: value.areaOfInterest || null,
        professionalSummary: value.professionalSummary || null,
        source: this.source,
        resumeFileName: this.resumeFileName,
      })
      .subscribe({
        next: (candidate) => {
          this.submitting.set(false);
          this.snackBar.open('Cadastro salvo com sucesso!', 'Fechar', { duration: 4000 });
          this.router.navigate(['/candidatos', candidate.id]);
        },
        error: (err) => {
          this.submitting.set(false);
          if (err.status === 400 && err.error?.errors) {
            this.applyServerValidationErrors(err.error.errors);
            this.submitError.set('Verifique os campos destacados e tente novamente.');
          } else {
            this.submitError.set('Ocorreu um erro ao salvar o cadastro. Tente novamente em alguns instantes.');
          }
        },
      });
  }

  getErrorMessage(controlName: 'fullName' | 'email' | 'phone' | 'areaOfInterest' | 'professionalSummary'): string {
    const control = this.form.get(controlName);
    if (!control || !control.errors) {
      return '';
    }

    if (control.errors['required']) {
      return 'Campo obrigatório.';
    }
    if (control.errors['email']) {
      return 'Informe um e-mail em um formato válido.';
    }
    if (control.errors['maxlength']) {
      return `Máximo de ${control.errors['maxlength'].requiredLength} caracteres.`;
    }
    if (control.errors['server']) {
      return control.errors['server'];
    }
    return 'Campo inválido.';
  }

  private applyServerValidationErrors(errors: Record<string, string[]>): void {
    for (const [field, messages] of Object.entries(errors)) {
      const controlName = field.charAt(0).toLowerCase() + field.slice(1);
      const control = this.form.get(controlName);
      if (control) {
        control.setErrors({ server: messages.join(' ') });
      }
    }
  }

  private extractServerMessage(err: unknown): string | null {
    const httpError = err as { error?: { errors?: Record<string, string[]>; message?: string } };
    if (httpError?.error?.errors) {
      return Object.values(httpError.error.errors).flat().join(' ');
    }
    if (httpError?.error?.message) {
      return httpError.error.message;
    }
    return null;
  }
}
