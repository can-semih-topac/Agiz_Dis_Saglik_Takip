const TURKISH_MONTHS = [
  'Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran',
  'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'
];

// "2026-07-12" + "14:25:00" -> "12 Temmuz 14.25"
export function formatTurkishDateTime(activityDate: string, activityTime: string): string {
  const [, month, day] = activityDate.split('-').map(Number);
  const time = activityTime.slice(0, 5).replace(':', '.');
  return `${Number(day)} ${TURKISH_MONTHS[month - 1]} ${time}`;
}
