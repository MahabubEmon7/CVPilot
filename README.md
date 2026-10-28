# CVPilot AI — AI-Powered Job-Specific Resume Builder

CVPilot AI is a full-stack, enterprise-grade web application designed to help job seekers create tailored, professional resumes and cover letters based on specific job circulars or job descriptions.

## 🚀 Key Features

- **Master Profile (Single Source of Truth):** Manage verified personal info, employment history, degrees, confirmed skills, and portfolio projects.
- **Resume Import & Extraction:** Upload existing resumes (PDF, DOCX, TXT) and review extracted information before confirming it into the master profile.
- **Job Circular Analyzer:** Paste any job circular; extracts required qualifications, skills, and experience criteria into structured fields.
- **Fact-Constrained AI Guarantee:** Zero hallucinations. The platform never fabricates employment history, degrees, or unconfirmed skills.
- **Match Scoring & Skill Gap Breakdown:** Transparent job-to-profile match percentage and actionable recommendations.
- **Multiple ATS Resume Templates:** Minimal ATS-friendly, Modern Professional, and Software Engineer layouts.
- **Direct Document Downloads:** Native backend PDF and DOCX file generation.

---

## 🛠️ Technology Stack

### Backend

- **Runtime:** .NET 10 (C#)
- **Architecture:** Clean Architecture (Domain, Application, Infrastructure, API)
- **ORM:** Entity Framework Core 10 (SQL Server)
- **Security:** ASP.NET Core Identity & JWT Bearer Authentication, BCrypt Password Hashing
- **Document Processing:** UglyToad.PdfPig (PDF text extraction), DocumentFormat.OpenXml (Word processing)
- **API Documentation:** Swagger / OpenAPI

### Frontend

- **Framework:** Angular 22 (Standalone Components)
- **Language:** TypeScript (Strict Mode)
- **State & Reactivity:** Angular Signals & RxJS
- **Styling:** SCSS, Bootstrap 5, Bootstrap Icons
- **Routing:** Feature-based lazy-loaded routes with `authGuard` and `authInterceptor`

---

## 🏃 Running the Application

### 1. Backend API

```powershell
cd backend/src/CVPilot.API
dotnet run
```

- API runs at `http://localhost:5000`
- Swagger UI documentation available at `http://localhost:5000/`

### 2. Frontend Web App

```powershell
cd frontend
npm install
npm start
```

- Web application runs at `http://localhost:4200`
