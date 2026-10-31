export interface CreateJobDescriptionRequest {
  rawContent: string;
  jobTitle?: string;
  companyName?: string;
}

export interface UpdateJobDescriptionRequest {
  jobTitle: string;
  companyName?: string;
  experienceRequirement?: string;
  requiredSkills: string[];
  preferredSkills: string[];
  responsibilities: string[];
  educationRequirements: string[];
  keywords: string[];
}

export interface JobDescription {
  id: string;
  userId: string;
  jobTitle: string;
  companyName?: string;
  rawContent: string;
  experienceRequirement?: string;
  requiredSkills: string[];
  preferredSkills: string[];
  responsibilities: string[];
  educationRequirements: string[];
  keywords: string[];
  isAnalyzed: boolean;
  analyzedAtUtc?: string;
  createdAtUtc: string;
}

export interface JobDescriptionSummary {
  id: string;
  jobTitle: string;
  companyName?: string;
  experienceRequirement?: string;
  requiredSkillsCount: number;
  createdAtUtc: string;
}
