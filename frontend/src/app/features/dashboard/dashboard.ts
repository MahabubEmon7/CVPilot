import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ProfileService } from '../../core/services/profile.service';
import { UserProfile } from '../../core/models/profile.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.scss']
})
export class DashboardComponent implements OnInit {
  authService = inject(AuthService);
  private profileService = inject(ProfileService);

  profile = signal<UserProfile | null>(null);
  isLoading = signal(true);

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
        }
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  getProfileCompletionPercentage(): number {
    const p = this.profile();
    if (!p) return 20;

    let score = 20; // user account created
    if (p.headline && p.summary) score += 20;
    if (p.experiences && p.experiences.length > 0) score += 20;
    if (p.educations && p.educations.length > 0) score += 15;
    if (p.skills && p.skills.length > 0) score += 15;
    if (p.projects && p.projects.length > 0) score += 10;
    return Math.min(score, 100);
  }
}
