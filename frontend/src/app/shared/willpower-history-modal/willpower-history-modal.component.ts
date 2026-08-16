import { Component, EventEmitter, inject, OnInit, Output } from '@angular/core';
import { WillpowerService } from '../../core/services/willpower.service';
import { WillpowerGranularity, WillpowerHistoryPointDto } from '../../core/models/willpower.models';
import { TURKISH_MONTHS } from '../turkish-date';

@Component({
  selector: 'app-willpower-history-modal',
  imports: [],
  templateUrl: './willpower-history-modal.component.html',
  styleUrl: './willpower-history-modal.component.css'
})
export class WillpowerHistoryModalComponent implements OnInit {
  private willpowerService = inject(WillpowerService);

  @Output() closed = new EventEmitter<void>();

  granularity: WillpowerGranularity = 'week';
  points: WillpowerHistoryPointDto[] = [];
  loading = false;

  readonly granularityOptions: { value: WillpowerGranularity; label: string }[] = [
    { value: 'day', label: 'Gün' },
    { value: 'week', label: 'Hafta' },
    { value: 'month', label: 'Ay' },
    { value: 'year', label: 'Yıl' }
  ];

  // SVG viewBox koordinatları — çizim tamamen bu sabitlere göre hesaplanıyor.
  readonly chartWidth = 600;
  readonly chartHeight = 220;
  readonly padding = { top: 16, right: 16, bottom: 28, left: 32 };
  readonly gridLines = [0, 25, 50, 75, 100];

  ngOnInit(): void {
    this.load();
  }

  selectGranularity(g: WillpowerGranularity): void {
    if (this.granularity === g) return;
    this.granularity = g;
    this.load();
  }

  get plotWidth(): number {
    return this.chartWidth - this.padding.left - this.padding.right;
  }

  get plotHeight(): number {
    return this.chartHeight - this.padding.top - this.padding.bottom;
  }

  xFor(index: number): number {
    if (this.points.length <= 1) return this.padding.left;
    return this.padding.left + (index / (this.points.length - 1)) * this.plotWidth;
  }

  yFor(score: number): number {
    return this.padding.top + (1 - score / 100) * this.plotHeight;
  }

  get polylinePoints(): string {
    return this.points.map((p, i) => `${this.xFor(i)},${this.yFor(p.score)}`).join(' ');
  }

  // Tüm noktaları etiketlemek karışır — yaklaşık 6 etiket kalacak şekilde seyreltiyoruz.
  get labeledIndices(): number[] {
    const n = this.points.length;
    if (n === 0) return [];
    const maxLabels = 6;
    if (n <= maxLabels) return this.points.map((_, i) => i);

    const step = Math.ceil((n - 1) / (maxLabels - 1));
    const indices: number[] = [];
    for (let i = 0; i < n; i += step) indices.push(i);
    if (indices[indices.length - 1] !== n - 1) indices.push(n - 1);
    return indices;
  }

  formatPointLabel(p: WillpowerHistoryPointDto): string {
    const [year, month, day] = p.date.split('-').map(Number);
    if (this.granularity === 'year') return `${year}`;
    if (this.granularity === 'month') return `${TURKISH_MONTHS[month - 1].slice(0, 3)} ${String(year).slice(2)}`;
    return `${day} ${TURKISH_MONTHS[month - 1].slice(0, 3)}`;
  }

  private load(): void {
    this.loading = true;
    this.willpowerService.getHistory(this.granularity).subscribe({
      next: (result) => {
        this.loading = false;
        if (result.success) this.points = result.data;
      },
      error: () => { this.loading = false; }
    });
  }

  close(): void {
    this.closed.emit();
  }
}
