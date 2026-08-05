import { Component, inject, OnInit } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { GoalStatusService } from '../../core/services/goal-status.service';
import { SuggestionService } from '../../core/services/suggestion.service';
import { GoalStatusDto } from '../../core/models/goal-status.models';

@Component({
  selector: 'app-home',
  imports: [RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  private authService = inject(AuthService);
  private goalStatusService = inject(GoalStatusService);
  private suggestionService = inject(SuggestionService);
  private router = inject(Router);

  fullName = this.authService.getSession()?.fullName ?? '';
  last7Days: GoalStatusDto[] = [];
  suggestionText = '';

  ngOnInit(): void {
    this.goalStatusService.getLast7Days().subscribe({
      next: (result) => {
        if (result.success) {
          this.last7Days = result.data;
        }
      }
    });

    this.loadSuggestion();
  }

  loadSuggestion(): void {
    this.suggestionService.getRandom().subscribe({
      next: (result) => {
        if (result.success) {
          this.suggestionText = result.data.text;
        }
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
