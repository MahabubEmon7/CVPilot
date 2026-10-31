import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { JobDescriptionService } from '../../core/services/job-description.service';
import { JobDescription, JobDescriptionSummary } from '../../core/models/job-description.model';

@Component({
  selector: 'app-job-analyzer',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './job-analyzer.html',
  styleUrls: ['./job-analyzer.scss']
})
export class JobAnalyzerComponent implements OnInit {
  private fb = inject(FormBuilder);
  private jobService = inject(JobDescriptionService);
  private router = inject(Router);

  activeTab = signal<'analyze' | 'history'>('analyze');
  isAnalyzing = signal(false);
  isSaving = signal(false);
  isLoadingHistory = signal(false);

  currentJob = signal<JobDescription | null>(null);
  savedJobs = signal<JobDescriptionSummary[]>([]);
  notification = signal<{ type: 'success' | 'danger'; message: string } | null>(null);

  // Input form
  inputForm: FormGroup = this.fb.group({
    rawContent: ['', [Validators.required, Validators.minLength(50)]],
    jobTitle: [''],
    companyName: ['']
  });

  // Review & Edit form
  reviewForm: FormGroup = this.fb.group({
    jobTitle: ['', [Validators.required]],
    companyName: [''],
    experienceRequirement: ['']
  });

  // Arrays for review stage
  requiredSkills = signal<string[]>([]);
  preferredSkills = signal<string[]>([]);
  responsibilities = signal<string[]>([]);
  educationRequirements = signal<string[]>([]);
  keywords = signal<string[]>([]);

  // New item inputs
  newReqSkill = signal('');
  newPrefSkill = signal('');
  newResp = signal('');

  ngOnInit(): void {
    this.loadHistory();
  }

  loadHistory(): void {
    this.isLoadingHistory.set(true);
    this.jobService.getAll().subscribe({
      next: (res) => {
        this.isLoadingHistory.set(false);
        if (res.success && res.data) {
          this.savedJobs.set(res.data);
        }
      },
      error: () => {
        this.isLoadingHistory.set(false);
      }
    });
  }

  onAnalyze(): void {
    if (this.inputForm.invalid) {
      this.inputForm.markAllAsTouched();
      return;
    }

    this.isAnalyzing.set(true);
    const val = this.inputForm.value;

    this.jobService.createAndAnalyze({
      rawContent: val.rawContent,
      jobTitle: val.jobTitle || undefined,
      companyName: val.companyName || undefined
    }).subscribe({
      next: (res) => {
        this.isAnalyzing.set(false);
        if (res.success && res.data) {
          this.populateReviewData(res.data);
          this.loadHistory();
          this.showToast('success', 'Job circular analyzed successfully! You can now review and refine the requirements.');
        } else {
          this.showToast('danger', res.message || 'Failed to analyze job circular.');
        }
      },
      error: (err) => {
        this.isAnalyzing.set(false);
        const msg = err.error?.message || err.error?.errors?.[0] || 'Error analyzing job circular. Please check your text.';
        this.showToast('danger', msg);
      }
    });
  }

  populateReviewData(job: JobDescription): void {
    this.currentJob.set(job);
    this.reviewForm.patchValue({
      jobTitle: job.jobTitle,
      companyName: job.companyName || '',
      experienceRequirement: job.experienceRequirement || ''
    });

    this.requiredSkills.set([...(job.requiredSkills || [])]);
    this.preferredSkills.set([...(job.preferredSkills || [])]);
    this.responsibilities.set([...(job.responsibilities || [])]);
    this.educationRequirements.set([...(job.educationRequirements || [])]);
    this.keywords.set([...(job.keywords || [])]);
  }

  selectSavedJob(id: string): void {
    this.jobService.getById(id).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.populateReviewData(res.data);
          this.activeTab.set('analyze');
          window.scrollTo({ top: 0, behavior: 'smooth' });
        }
      },
      error: () => this.showToast('danger', 'Failed to load selected job details.')
    });
  }

  onSaveEdits(): void {
    const job = this.currentJob();
    if (!job || this.reviewForm.invalid) return;

    this.isSaving.set(true);
    const formVal = this.reviewForm.value;

    this.jobService.update(job.id, {
      jobTitle: formVal.jobTitle,
      companyName: formVal.companyName,
      experienceRequirement: formVal.experienceRequirement,
      requiredSkills: this.requiredSkills(),
      preferredSkills: this.preferredSkills(),
      responsibilities: this.responsibilities(),
      educationRequirements: this.educationRequirements(),
      keywords: this.keywords()
    }).subscribe({
      next: (res) => {
        this.isSaving.set(false);
        if (res.success && res.data) {
          this.currentJob.set(res.data);
          this.loadHistory();
          this.showToast('success', 'Job requirements updated and saved!');
        }
      },
      error: () => {
        this.isSaving.set(false);
        this.showToast('danger', 'Failed to save changes.');
      }
    });
  }

  onReanalyze(): void {
    const job = this.currentJob();
    if (!job) return;

    this.isAnalyzing.set(true);
    this.jobService.reanalyze(job.id).subscribe({
      next: (res) => {
        this.isAnalyzing.set(false);
        if (res.success && res.data) {
          this.populateReviewData(res.data);
          this.showToast('success', 'Job circular re-analyzed with latest models.');
        }
      },
      error: () => {
        this.isAnalyzing.set(false);
        this.showToast('danger', 'Failed to re-analyze.');
      }
    });
  }

  onDelete(id: string, event: Event): void {
    event.stopPropagation();
    if (!confirm('Are you sure you want to delete this job circular?')) return;

    this.jobService.delete(id).subscribe({
      next: () => {
        if (this.currentJob()?.id === id) {
          this.currentJob.set(null);
        }
        this.loadHistory();
        this.showToast('success', 'Job circular deleted.');
      },
      error: () => this.showToast('danger', 'Failed to delete job circular.')
    });
  }

  // Tag & List management
  addRequiredSkill(event?: Event): void {
    event?.preventDefault();
    const val = this.newReqSkill().trim();
    if (val && !this.requiredSkills().includes(val)) {
      this.requiredSkills.set([...this.requiredSkills(), val]);
      this.newReqSkill.set('');
    }
  }

  removeRequiredSkill(index: number): void {
    const list = [...this.requiredSkills()];
    list.splice(index, 1);
    this.requiredSkills.set(list);
  }

  addPreferredSkill(event?: Event): void {
    event?.preventDefault();
    const val = this.newPrefSkill().trim();
    if (val && !this.preferredSkills().includes(val)) {
      this.preferredSkills.set([...this.preferredSkills(), val]);
      this.newPrefSkill.set('');
    }
  }

  removePreferredSkill(index: number): void {
    const list = [...this.preferredSkills()];
    list.splice(index, 1);
    this.preferredSkills.set(list);
  }

  addResponsibility(event?: Event): void {
    event?.preventDefault();
    const val = this.newResp().trim();
    if (val) {
      this.responsibilities.set([...this.responsibilities(), val]);
      this.newResp.set('');
    }
  }

  removeResponsibility(index: number): void {
    const list = [...this.responsibilities()];
    list.splice(index, 1);
    this.responsibilities.set(list);
  }

  loadSampleCircular(): void {
    this.inputForm.patchValue({
      jobTitle: 'Senior ASP.NET Core & Angular Engineer',
      companyName: 'Enosis Solutions',
      rawContent: `We are looking for a Senior Full-Stack Software Engineer with proven expertise in C#, ASP.NET Core, and Angular.

Key Responsibilities:
- Design and develop robust RESTful APIs and clean architecture microservices using .NET 10.
- Build responsive, reactive frontends using Angular standalone components, RxJS, and TypeScript.
- Optimize database queries and schema design in Microsoft SQL Server.
- Implement automated unit tests and participate in CI/CD pipeline deployments with Docker.
- Collaborate with cross-functional product teams to deliver high-quality scalable software.

Required Skills & Qualifications:
- 3+ years of professional software engineering experience.
- Strong proficiency in C#, ASP.NET Core, Entity Framework Core, and SQL Server.
- Hands-on experience building web applications using Angular, TypeScript, HTML5, and SCSS.
- Familiarity with Git version control, Clean Architecture principles, and REST API standards.
- Bachelor's degree in Computer Science, Software Engineering, or equivalent practical experience.

Preferred Qualifications:
- Experience with Docker, Kubernetes, or cloud deployment on Azure or AWS.
- Knowledge of Microservices architecture and asynchronous messaging patterns.`
    });
  }

  startNewAnalysis(): void {
    this.currentJob.set(null);
    this.inputForm.reset();
  }

  showToast(type: 'success' | 'danger', message: string): void {
    this.notification.set({ type, message });
    setTimeout(() => this.notification.set(null), 4000);
  }
}
