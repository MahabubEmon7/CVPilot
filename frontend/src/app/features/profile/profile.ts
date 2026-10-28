import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProfileService } from '../../core/services/profile.service';
import { 
  UserProfile, 
  WorkExperience, 
  Education, 
  UserSkill, 
  Project, 
  SkillCategory, 
  SkillProficiency 
} from '../../core/models/profile.model';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.html',
  styleUrls: ['./profile.scss']
})
export class ProfileComponent implements OnInit {
  private fb = inject(FormBuilder);
  private profileService = inject(ProfileService);

  activeTab = signal<'personal' | 'experience' | 'education' | 'skills' | 'projects'>('personal');
  profile = signal<UserProfile | null>(null);
  isLoading = signal(true);
  isSaving = signal(false);
  notification = signal<{ type: 'success' | 'danger'; message: string } | null>(null);

  // Modal / Sub-forms states
  showExpModal = signal(false);
  editingExpId = signal<string | null>(null);

  showEduModal = signal(false);
  editingEduId = signal<string | null>(null);

  showProjectModal = signal(false);
  editingProjectId = signal<string | null>(null);

  // Resume Import states
  showImportModal = signal(false);
  importStage = signal<'upload' | 'review'>('upload');
  selectedFile = signal<File | null>(null);
  isParsing = signal(false);
  parsedData = signal<any | null>(null);
  importedSkills = signal<any[]>([]);

  importReviewForm: FormGroup = this.fb.group({
    fullName: [''],
    headline: [''],
    summary: [''],
    phoneNumber: [''],
    locationCity: [''],
    locationCountry: ['Bangladesh'],
    linkedInUrl: [''],
    githubUrl: ['']
  });

  // Forms
  personalForm: FormGroup = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(150)]],
    headline: [''],
    summary: [''],
    phoneNumber: [''],
    locationCity: [''],
    locationCountry: ['Bangladesh'],
    postalCode: [''],
    linkedInUrl: [''],
    githubUrl: [''],
    portfolioUrl: ['']
  });

  expForm: FormGroup = this.fb.group({
    jobTitle: ['', [Validators.required]],
    companyName: ['', [Validators.required]],
    location: [''],
    startDate: ['', [Validators.required]],
    endDate: [''],
    isCurrent: [false],
    bulletPointsRaw: [''] // newline separated
  });

  eduForm: FormGroup = this.fb.group({
    institution: ['', [Validators.required]],
    degree: ['', [Validators.required]],
    fieldOfStudy: ['', [Validators.required]],
    startDate: ['', [Validators.required]],
    endDate: [''],
    isCurrent: [false],
    gradeOrGpa: [''],
    description: ['']
  });

  skillForm: FormGroup = this.fb.group({
    name: ['', [Validators.required]],
    category: [SkillCategory.Technical, [Validators.required]],
    proficiency: [SkillProficiency.Intermediate, [Validators.required]],
    yearsOfExperience: [null]
  });

  projectForm: FormGroup = this.fb.group({
    title: ['', [Validators.required]],
    role: [''],
    description: ['', [Validators.required]],
    technologiesUsed: ['', [Validators.required]],
    repoUrl: [''],
    liveUrl: [''],
    bulletPointsRaw: ['']
  });

  readonly SkillCategory = SkillCategory;
  readonly SkillProficiency = SkillProficiency;

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading.set(true);
    this.profileService.getProfile().subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.success && res.data) {
          this.profile.set(res.data);
          this.patchPersonalForm(res.data);
        }
      },
      error: () => {
        this.isLoading.set(false);
        this.showToast('danger', 'Failed to load profile. Please refresh.');
      }
    });
  }

  patchPersonalForm(p: UserProfile): void {
    this.personalForm.patchValue({
      fullName: p.fullName,
      headline: p.headline || '',
      summary: p.summary || '',
      phoneNumber: p.phoneNumber || '',
      locationCity: p.locationCity || '',
      locationCountry: p.locationCountry || 'Bangladesh',
      postalCode: p.postalCode || '',
      linkedInUrl: p.linkedInUrl || '',
      githubUrl: p.githubUrl || '',
      portfolioUrl: p.portfolioUrl || ''
    });
  }

  onSavePersonal(): void {
    if (this.personalForm.invalid) {
      this.personalForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.profileService.updateProfile(this.personalForm.value).subscribe({
      next: (res) => {
        this.isSaving.set(false);
        if (res.success && res.data) {
          this.profile.set(res.data);
          this.showToast('success', 'Personal details saved successfully.');
        }
      },
      error: () => {
        this.isSaving.set(false);
        this.showToast('danger', 'Failed to save personal details.');
      }
    });
  }

  // --- Work Experience Methods ---
  openAddExpModal(): void {
    this.editingExpId.set(null);
    this.expForm.reset({ isCurrent: false });
    this.showExpModal.set(true);
  }

  openEditExpModal(exp: WorkExperience): void {
    this.editingExpId.set(exp.id);
    this.expForm.patchValue({
      jobTitle: exp.jobTitle,
      companyName: exp.companyName,
      location: exp.location || '',
      startDate: exp.startDate ? exp.startDate.substring(0, 10) : '',
      endDate: exp.endDate ? exp.endDate.substring(0, 10) : '',
      isCurrent: exp.isCurrent,
      bulletPointsRaw: (exp.bulletPoints || []).join('\n')
    });
    this.showExpModal.set(true);
  }

  closeExpModal(): void {
    this.showExpModal.set(false);
    this.editingExpId.set(null);
  }

  onSaveExp(): void {
    if (this.expForm.invalid) {
      this.expForm.markAllAsTouched();
      return;
    }

    const val = this.expForm.value;
    const bullets = (val.bulletPointsRaw || '')
      .split('\n')
      .map((b: string) => b.trim())
      .filter((b: string) => b.length > 0);

    const payload = {
      jobTitle: val.jobTitle,
      companyName: val.companyName,
      location: val.location,
      startDate: val.startDate,
      endDate: val.isCurrent ? undefined : val.endDate,
      isCurrent: val.isCurrent,
      bulletPoints: bullets,
      displayOrder: 0
    };

    const expId = this.editingExpId();
    this.isSaving.set(true);

    const request$ = expId
      ? this.profileService.updateExperience(expId, payload)
      : this.profileService.addExperience(payload);

    request$.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.closeExpModal();
        this.loadProfile();
        this.showToast('success', expId ? 'Experience updated.' : 'Experience added.');
      },
      error: () => {
        this.isSaving.set(false);
        this.showToast('danger', 'Failed to save experience.');
      }
    });
  }

  onDeleteExp(id: string): void {
    if (!confirm('Are you sure you want to delete this experience?')) return;

    this.profileService.deleteExperience(id).subscribe({
      next: () => {
        this.loadProfile();
        this.showToast('success', 'Experience deleted.');
      },
      error: () => this.showToast('danger', 'Failed to delete experience.')
    });
  }

  // --- Education Methods ---
  openAddEduModal(): void {
    this.editingEduId.set(null);
    this.eduForm.reset({ isCurrent: false });
    this.showEduModal.set(true);
  }

  openEditEduModal(edu: Education): void {
    this.editingEduId.set(edu.id);
    this.eduForm.patchValue({
      institution: edu.institution,
      degree: edu.degree,
      fieldOfStudy: edu.fieldOfStudy,
      startDate: edu.startDate ? edu.startDate.substring(0, 10) : '',
      endDate: edu.endDate ? edu.endDate.substring(0, 10) : '',
      isCurrent: edu.isCurrent,
      gradeOrGpa: edu.gradeOrGpa || '',
      description: edu.description || ''
    });
    this.showEduModal.set(true);
  }

  closeEduModal(): void {
    this.showEduModal.set(false);
    this.editingEduId.set(null);
  }

  onSaveEdu(): void {
    if (this.eduForm.invalid) {
      this.eduForm.markAllAsTouched();
      return;
    }

    const val = this.eduForm.value;
    const payload = {
      institution: val.institution,
      degree: val.degree,
      fieldOfStudy: val.fieldOfStudy,
      startDate: val.startDate,
      endDate: val.isCurrent ? undefined : val.endDate,
      isCurrent: val.isCurrent,
      gradeOrGpa: val.gradeOrGpa,
      description: val.description,
      displayOrder: 0
    };

    const eduId = this.editingEduId();
    this.isSaving.set(true);

    const request$ = eduId
      ? this.profileService.updateEducation(eduId, payload)
      : this.profileService.addEducation(payload);

    request$.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.closeEduModal();
        this.loadProfile();
        this.showToast('success', eduId ? 'Education updated.' : 'Education added.');
      },
      error: () => {
        this.isSaving.set(false);
        this.showToast('danger', 'Failed to save education.');
      }
    });
  }

  onDeleteEdu(id: string): void {
    if (!confirm('Are you sure you want to delete this education entry?')) return;

    this.profileService.deleteEducation(id).subscribe({
      next: () => {
        this.loadProfile();
        this.showToast('success', 'Education entry deleted.');
      },
      error: () => this.showToast('danger', 'Failed to delete education entry.')
    });
  }

  // --- Skills Methods ---
  onAddSkill(): void {
    if (this.skillForm.invalid) {
      this.skillForm.markAllAsTouched();
      return;
    }

    const val = this.skillForm.value;
    this.isSaving.set(true);
    this.profileService.addSkill({
      name: val.name,
      category: Number(val.category),
      proficiency: Number(val.proficiency),
      yearsOfExperience: val.yearsOfExperience ? Number(val.yearsOfExperience) : undefined
    }).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.skillForm.reset({
          category: SkillCategory.Technical,
          proficiency: SkillProficiency.Intermediate
        });
        this.loadProfile();
        this.showToast('success', 'Skill added.');
      },
      error: (err) => {
        this.isSaving.set(false);
        const msg = err.error?.errors?.[0] || 'Failed to add skill.';
        this.showToast('danger', msg);
      }
    });
  }

  onDeleteSkill(id: string): void {
    this.profileService.deleteSkill(id).subscribe({
      next: () => {
        this.loadProfile();
        this.showToast('success', 'Skill removed.');
      },
      error: () => this.showToast('danger', 'Failed to remove skill.')
    });
  }

  // --- Projects Methods ---
  openAddProjectModal(): void {
    this.editingProjectId.set(null);
    this.projectForm.reset();
    this.showProjectModal.set(true);
  }

  openEditProjectModal(proj: Project): void {
    this.editingProjectId.set(proj.id);
    this.projectForm.patchValue({
      title: proj.title,
      role: proj.role || '',
      description: proj.description,
      technologiesUsed: proj.technologiesUsed,
      repoUrl: proj.repoUrl || '',
      liveUrl: proj.liveUrl || '',
      bulletPointsRaw: (proj.bulletPoints || []).join('\n')
    });
    this.showProjectModal.set(true);
  }

  closeProjectModal(): void {
    this.showProjectModal.set(false);
    this.editingProjectId.set(null);
  }

  onSaveProject(): void {
    if (this.projectForm.invalid) {
      this.projectForm.markAllAsTouched();
      return;
    }

    const val = this.projectForm.value;
    const bullets = (val.bulletPointsRaw || '')
      .split('\n')
      .map((b: string) => b.trim())
      .filter((b: string) => b.length > 0);

    const payload = {
      title: val.title,
      role: val.role,
      description: val.description,
      technologiesUsed: val.technologiesUsed,
      repoUrl: val.repoUrl,
      liveUrl: val.liveUrl,
      bulletPoints: bullets,
      displayOrder: 0
    };

    const projId = this.editingProjectId();
    this.isSaving.set(true);

    const request$ = projId
      ? this.profileService.updateProject(projId, payload)
      : this.profileService.addProject(payload);

    request$.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.closeProjectModal();
        this.loadProfile();
        this.showToast('success', projId ? 'Project updated.' : 'Project added.');
      },
      error: () => {
        this.isSaving.set(false);
        this.showToast('danger', 'Failed to save project.');
      }
    });
  }

  onDeleteProject(id: string): void {
    if (!confirm('Are you sure you want to delete this project?')) return;

    this.profileService.deleteProject(id).subscribe({
      next: () => {
        this.loadProfile();
        this.showToast('success', 'Project deleted.');
      },
      error: () => this.showToast('danger', 'Failed to delete project.')
    });
  }

  // --- Resume Import Methods ---
  openImportModal(): void {
    this.selectedFile.set(null);
    this.parsedData.set(null);
    this.importedSkills.set([]);
    this.importStage.set('upload');
    this.showImportModal.set(true);
  }

  closeImportModal(): void {
    this.showImportModal.set(false);
    this.selectedFile.set(null);
    this.parsedData.set(null);
  }

  onFileSelected(event: any): void {
    const file = event.target?.files?.[0];
    if (file) {
      if (file.size > 5 * 1024 * 1024) {
        this.showToast('danger', 'File exceeds the 5MB size limit.');
        return;
      }
      this.selectedFile.set(file);
    }
  }

  onUploadAndParse(): void {
    const file = this.selectedFile();
    if (!file) return;

    this.isParsing.set(true);
    this.profileService.uploadResumeForParsing(file).subscribe({
      next: (res) => {
        this.isParsing.set(false);
        if (res.success && res.data) {
          this.parsedData.set(res.data);
          this.importedSkills.set(res.data.skills || []);

          this.importReviewForm.patchValue({
            fullName: res.data.fullName || this.profile()?.fullName || '',
            headline: res.data.headline || this.profile()?.headline || '',
            summary: res.data.summary || this.profile()?.summary || '',
            phoneNumber: res.data.phoneNumber || this.profile()?.phoneNumber || '',
            locationCity: res.data.locationCity || this.profile()?.locationCity || '',
            locationCountry: res.data.locationCountry || 'Bangladesh',
            linkedInUrl: this.profile()?.linkedInUrl || '',
            githubUrl: this.profile()?.githubUrl || ''
          });

          this.importStage.set('review');
        } else {
          this.showToast('danger', res.message || 'Failed to parse file.');
        }
      },
      error: (err) => {
        this.isParsing.set(false);
        const msg = err.error?.message || err.error?.errors?.[0] || 'Failed to extract text from resume.';
        this.showToast('danger', msg);
      }
    });
  }

  removeImportedSkill(index: number): void {
    const skills = [...this.importedSkills()];
    skills.splice(index, 1);
    this.importedSkills.set(skills);
  }

  onConfirmImport(): void {
    const val = this.importReviewForm.value;
    const parsed = this.parsedData();

    const payload = {
      fullName: val.fullName,
      headline: val.headline,
      summary: val.summary,
      phoneNumber: val.phoneNumber,
      locationCity: val.locationCity,
      locationCountry: val.locationCountry,
      linkedInUrl: val.linkedInUrl,
      githubUrl: val.githubUrl,
      confirmedExperiences: parsed?.experiences || [],
      confirmedEducations: parsed?.educations || [],
      confirmedSkills: this.importedSkills()
    };

    this.isSaving.set(true);
    this.profileService.confirmImportedProfile(payload).subscribe({
      next: (res) => {
        this.isSaving.set(false);
        if (res.success && res.data) {
          this.profile.set(res.data);
          this.patchPersonalForm(res.data);
          this.closeImportModal();
          this.showToast('success', 'Resume information reviewed and imported into your Master Profile!');
        }
      },
      error: () => {
        this.isSaving.set(false);
        this.showToast('danger', 'Failed to confirm imported profile.');
      }
    });
  }

  // --- UI Helpers ---
  getCategoryName(cat: SkillCategory): string {
    switch (cat) {
      case SkillCategory.Technical: return 'Technical';
      case SkillCategory.Soft: return 'Soft Skill';
      case SkillCategory.ToolOrFramework: return 'Tool/Framework';
      case SkillCategory.Language: return 'Language';
      default: return 'Other';
    }
  }

  showToast(type: 'success' | 'danger', message: string): void {
    this.notification.set({ type, message });
    setTimeout(() => {
      this.notification.set(null);
    }, 4000);
  }
}
