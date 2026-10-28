export enum SkillCategory {
  Technical = 1,
  Soft = 2,
  ToolOrFramework = 3,
  Language = 4,
  Other = 5
}

export enum SkillProficiency {
  Beginner = 1,
  Intermediate = 2,
  Advanced = 3,
  Expert = 4
}

export interface WorkExperience {
  id: string;
  jobTitle: string;
  companyName: string;
  location?: string;
  startDate: string;
  endDate?: string;
  isCurrent: boolean;
  bulletPoints: string[];
  displayOrder: number;
}

export interface Education {
  id: string;
  institution: string;
  degree: string;
  fieldOfStudy: string;
  startDate: string;
  endDate?: string;
  isCurrent: boolean;
  gradeOrGpa?: string;
  description?: string;
  displayOrder: number;
}

export interface UserSkill {
  id: string;
  name: string;
  category: SkillCategory;
  proficiency: SkillProficiency;
  yearsOfExperience?: number;
}

export interface Project {
  id: string;
  title: string;
  role?: string;
  description: string;
  technologiesUsed: string;
  repoUrl?: string;
  liveUrl?: string;
  bulletPoints: string[];
  displayOrder: number;
}

export interface Certification {
  id: string;
  name: string;
  issuingOrganization: string;
  issueDate: string;
  expirationDate?: string;
  credentialId?: string;
  credentialUrl?: string;
}

export interface Language {
  id: string;
  name: string;
  proficiencyLevel: string;
}

export interface ParsedResume {
  fullName?: string;
  email?: string;
  phoneNumber?: string;
  locationCity?: string;
  locationCountry?: string;
  headline?: string;
  summary?: string;
  experiences: Omit<WorkExperience, 'id'>[];
  educations: Omit<Education, 'id'>[];
  skills: Omit<UserSkill, 'id'>[];
  rawExtractedText: string;
}

export interface ConfirmImportProfile {
  fullName?: string;
  headline?: string;
  summary?: string;
  phoneNumber?: string;
  locationCity?: string;
  locationCountry?: string;
  linkedInUrl?: string;
  githubUrl?: string;
  confirmedExperiences: Omit<WorkExperience, 'id'>[];
  confirmedEducations: Omit<Education, 'id'>[];
  confirmedSkills: Omit<UserSkill, 'id'>[];
}

export interface UserProfile {
  id: string;
  userId: string;
  fullName: string;
  email: string;
  headline?: string;
  summary?: string;
  phoneNumber?: string;
  locationCity?: string;
  locationCountry?: string;
  postalCode?: string;
  linkedInUrl?: string;
  githubUrl?: string;
  portfolioUrl?: string;
  experiences: WorkExperience[];
  educations: Education[];
  skills: UserSkill[];
  projects: Project[];
  certifications: Certification[];
  languages: Language[];
}

