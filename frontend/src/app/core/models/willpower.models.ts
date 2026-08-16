export interface WillpowerTierDto {
  scoreFrom: number;
  scoreTo: number;
  label: string;
}

export interface WillpowerScoreDto {
  score: number;
  label: string;
  rawPoints: number;
  tiers: WillpowerTierDto[];
}

export interface WillpowerHistoryPointDto {
  date: string;
  score: number;
}

export type WillpowerGranularity = 'day' | 'week' | 'month' | 'year';
