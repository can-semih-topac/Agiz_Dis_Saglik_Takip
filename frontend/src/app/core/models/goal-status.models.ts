export interface GoalStatusDto {
  id: number;
  goalId: number;
  goalTitle: string;
  activityDate: string;
  activityTime: string;
  durationMinutes: number;
  isApplied: boolean;
}

export interface CreateGoalStatusDto {
  goalId: number;
  activityDate: string;
  activityTime: string;
  durationMinutes: number;
  isApplied: boolean;
}
