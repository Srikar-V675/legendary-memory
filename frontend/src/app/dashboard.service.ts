import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DashboardMetrics {
  activeCount: number;
  expiredCount: number;
  completedCount: number;
  failedCount: number;
  totalAuctions: number;
  pendingPayment: number;
  successfulPayments: number;
  failedPayments: number;
  paymentSuccessRate: number;
  totalRevenue: number;
  topBidders: TopBidder[];
  recentAuctions: RecentAuction[];
  lastUpdated: string;
}

export interface TopBidder {
  bidderId: number;
  bidderEmail: string;
  totalBids: number;
  totalAmount: number;
  averageBid: number;
}

export interface RecentAuction {
  auctionId: number;
  productName: string;
  status: string;
  startTime: string;
  expiryTime: string;
  highestBid: number;
  bidCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiUrl = 'http://localhost:8080/api/dashboard';

  constructor(private http: HttpClient) { }

  getDashboardMetrics(): Observable<DashboardMetrics> {
    return this.http.get<DashboardMetrics>(this.apiUrl);
  }
}
