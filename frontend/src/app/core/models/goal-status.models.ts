import { TrackingType } from './goal.models';

export interface GoalStatusDto {
  id: number;
  goalId: number;
  goalTitle: string;
  trackingType: TrackingType;
  activityDate: string;
  activityTime: string;
  // Hedefin türü Yapildi ise null.
  durationMinutes: number | null;
  // Bu kaydın tarihine kadar, bu hedef için kesintisiz kaç gündür kayıt girildiği.
  streakCount: number;
}

export interface CreateGoalStatusDto {
  goalId: number;
  activityDate: string;
  activityTime: string;
  durationMinutes: number | null;
}

export interface UpdateGoalStatusDto {
  activityDate: string;
  activityTime: string;
  durationMinutes: number | null;
}

export interface LongestStreakDto {
  goalId: number;
  goalTitle: string;
  longestStreak: number;
}
