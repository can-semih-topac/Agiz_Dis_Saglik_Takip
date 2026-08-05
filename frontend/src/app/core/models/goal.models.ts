// Backend'deki enum'larla birebir aynı sayısal değerler (0,1,2) — JSON'da int olarak gidip geliyor.
export enum PeriodUnit {
  Gun = 0,
  Hafta = 1,
  Ay = 2
}

export enum Importance {
  Dusuk = 0,
  Orta = 1,
  Yuksek = 2
}

export interface GoalDto {
  id: number;
  title: string;
  description: string;
  periodUnit: PeriodUnit;
  periodFrequency: number;
  importance: Importance;
  createdAt: string;
}

export interface CreateGoalDto {
  title: string;
  description: string;
  periodUnit: PeriodUnit;
  periodFrequency: number;
  importance: Importance;
}
