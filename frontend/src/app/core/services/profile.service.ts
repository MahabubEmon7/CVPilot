import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse } from '../models/api-response.model';
import { 
  UserProfile, 
  WorkExperience, 
  Education, 
  UserSkill, 
  Project, 
  SkillCategory, 
  SkillProficiency 
} from '../models/profile.model';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private api = inject(ApiService);

  getProfile(): Observable<ApiResponse<UserProfile>> {
    return this.api.get<UserProfile>('profile');
  }

  updateProfile(data: Partial<UserProfile>): Observable<ApiResponse<UserProfile>> {
    return this.api.put<UserProfile>('profile', data);
  }

  addExperience(data: Omit<WorkExperience, 'id'>): Observable<ApiResponse<WorkExperience>> {
    return this.api.post<WorkExperience>('profile/experience', data);
  }

  updateExperience(id: string, data: Omit<WorkExperience, 'id'>): Observable<ApiResponse<WorkExperience>> {
    return this.api.put<WorkExperience>(`profile/experience/${id}`, data);
  }

  deleteExperience(id: string): Observable<ApiResponse<void>> {
    return this.api.delete<void>(`profile/experience/${id}`);
  }

  addEducation(data: Omit<Education, 'id'>): Observable<ApiResponse<Education>> {
    return this.api.post<Education>('profile/education', data);
  }

  updateEducation(id: string, data: Omit<Education, 'id'>): Observable<ApiResponse<Education>> {
    return this.api.put<Education>(`profile/education/${id}`, data);
  }

  deleteEducation(id: string): Observable<ApiResponse<void>> {
    return this.api.delete<void>(`profile/education/${id}`);
  }

  addSkill(data: { name: string; category: SkillCategory; proficiency: SkillProficiency; yearsOfExperience?: number }): Observable<ApiResponse<UserSkill>> {
    return this.api.post<UserSkill>('profile/skills', data);
  }

  deleteSkill(id: string): Observable<ApiResponse<void>> {
    return this.api.delete<void>(`profile/skills/${id}`);
  }

  addProject(data: Omit<Project, 'id'>): Observable<ApiResponse<Project>> {
    return this.api.post<Project>('profile/projects', data);
  }

  updateProject(id: string, data: Omit<Project, 'id'>): Observable<ApiResponse<Project>> {
    return this.api.put<Project>(`profile/projects/${id}`, data);
  }

  deleteProject(id: string): Observable<ApiResponse<void>> {
    return this.api.delete<void>(`profile/projects/${id}`);
  }

  uploadResumeForParsing(file: File): Observable<ApiResponse<any>> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.api.post<any>('profile/import/upload', formData);
  }

  confirmImportedProfile(data: any): Observable<ApiResponse<UserProfile>> {
    return this.api.post<UserProfile>('profile/import/confirm', data);
  }
}

