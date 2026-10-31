import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse } from '../models/api-response.model';
import { 
  JobDescription, 
  JobDescriptionSummary, 
  CreateJobDescriptionRequest, 
  UpdateJobDescriptionRequest 
} from '../models/job-description.model';

@Injectable({
  providedIn: 'root'
})
export class JobDescriptionService {
  private api = inject(ApiService);

  createAndAnalyze(data: CreateJobDescriptionRequest): Observable<ApiResponse<JobDescription>> {
    return this.api.post<JobDescription>('jobdescriptions', data);
  }

  getAll(): Observable<ApiResponse<JobDescriptionSummary[]>> {
    return this.api.get<JobDescriptionSummary[]>('jobdescriptions');
  }

  getById(id: string): Observable<ApiResponse<JobDescription>> {
    return this.api.get<JobDescription>(`jobdescriptions/${id}`);
  }

  update(id: string, data: UpdateJobDescriptionRequest): Observable<ApiResponse<JobDescription>> {
    return this.api.put<JobDescription>(`jobdescriptions/${id}`, data);
  }

  delete(id: string): Observable<ApiResponse<void>> {
    return this.api.delete<void>(`jobdescriptions/${id}`);
  }

  reanalyze(id: string): Observable<ApiResponse<JobDescription>> {
    return this.api.post<JobDescription>(`jobdescriptions/${id}/reanalyze`, {});
  }
}
