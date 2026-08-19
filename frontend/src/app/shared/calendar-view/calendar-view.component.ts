import { Component, EventEmitter, Input, OnChanges, OnInit, Output } from '@angular/core';

interface CalendarDay {
  date: string;
  day: number;
  hasRecords: boolean;
  isToday: boolean;
}

@Component({
  selector: 'app-calendar-view',
  imports: [],
  templateUrl: './calendar-view.component.html',
  styleUrl: './calendar-view.component.css'
})
export class CalendarViewComponent implements OnInit, OnChanges {
  // "YYYY-MM-DD" formatında, en az bir kaydı olan günler — noktalı işaret için.
  @Input() recordDates: Set<string> = new Set();
  @Input() selectedDate: string | null = null;
  @Output() dateSelected = new EventEmitter<string>();

  private readonly today = new Date();
  viewYear = this.today.getFullYear();
  viewMonth = this.today.getMonth();

  readonly dayHeaders = ['Pzt', 'Sal', 'Çar', 'Per', 'Cum', 'Cmt', 'Paz'];
  readonly monthNames = [
    'Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran',
    'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'
  ];

  weeks: (CalendarDay | null)[][] = [];

  ngOnInit(): void {
    this.buildCalendar();
  }

  ngOnChanges(): void {
    this.buildCalendar();
  }

  get monthLabel(): string {
    return `${this.monthNames[this.viewMonth]} ${this.viewYear}`;
  }

  prevMonth(): void {
    this.viewMonth--;
    if (this.viewMonth < 0) {
      this.viewMonth = 11;
      this.viewYear--;
    }
    this.buildCalendar();
  }

  nextMonth(): void {
    this.viewMonth++;
    if (this.viewMonth > 11) {
      this.viewMonth = 0;
      this.viewYear++;
    }
    this.buildCalendar();
  }

  selectDay(day: CalendarDay | null): void {
    if (!day) return;
    this.dateSelected.emit(day.date);
  }

  isSelected(day: CalendarDay): boolean {
    return day.date === this.selectedDate;
  }

  private buildCalendar(): void {
    const firstOfMonth = new Date(this.viewYear, this.viewMonth, 1);
    const daysInMonth = new Date(this.viewYear, this.viewMonth + 1, 0).getDate();
    // JS'te Pazar=0 — Pazartesi=0 olacak şekilde kaydırıyoruz (Türkiye haftası).
    const leadingBlanks = (firstOfMonth.getDay() + 6) % 7;
    const todayStr = this.formatDate(new Date());

    const cells: (CalendarDay | null)[] = [];
    for (let i = 0; i < leadingBlanks; i++) {
      cells.push(null);
    }
    for (let d = 1; d <= daysInMonth; d++) {
      const dateStr = this.formatDate(new Date(this.viewYear, this.viewMonth, d));
      cells.push({
        date: dateStr,
        day: d,
        hasRecords: this.recordDates.has(dateStr),
        isToday: dateStr === todayStr
      });
    }
    while (cells.length % 7 !== 0) {
      cells.push(null);
    }

    const weeks: (CalendarDay | null)[][] = [];
    for (let i = 0; i < cells.length; i += 7) {
      weeks.push(cells.slice(i, i + 7));
    }
    this.weeks = weeks;
  }

  private formatDate(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }
}
