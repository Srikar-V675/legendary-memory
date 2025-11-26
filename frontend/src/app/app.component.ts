import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService, DashboardMetrics } from './dashboard.service';
import { interval, Subscription } from 'rxjs';
import { switchMap, startWith } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit, OnDestroy {
  metrics: DashboardMetrics | null = null;
  loading = true;
  error: string | null = null;
  private subscription?: Subscription;

  constructor(private dashboardService: DashboardService) { }

  ngOnInit() {
    // Auto-refresh every 10 seconds
    this.subscription = interval(10000)
      .pipe(
        startWith(0),
        switchMap(() => this.dashboardService.getDashboardMetrics())
      )
      .subscribe({
        next: (data) => {
          this.metrics = data;
          this.loading = false;
          this.error = null;
        },
        error: (err) => {
          this.error = 'Failed to fetch dashboard data. Make sure the API is running on http://localhost:8080';
          this.loading = false;
          console.error('Error fetching dashboard:', err);
        }
      });
  }

  ngOnDestroy() {
    this.subscription?.unsubscribe();
  }

  getStatusClass(status: string): string {
    return `status-${status.toLowerCase()}`;
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleString();
  }

  formatTime(dateString: string): string {
    return new Date(dateString).toLocaleTimeString();
  }
}
