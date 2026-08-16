using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.Willpower;
using AgizDisSaglikTakip.Core.Utilities.Results;
using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.Entities.Enums;

namespace AgizDisSaglikTakip.Business.Concrete;

// Matematiğin özeti (kullanıcıya da anlatıldı):
// 1) Her hedefin "ham puana" katkısı = önem ağırlığı (Düşük=1, Orta=2, Yüksek=3) x güncel seri uzunluğu.
//    Seri kırılınca katkı sıfırlanır — kayıp zaten weight*streak olduğu için uzun seriyi bozmak otomatik
//    olarak daha çok puan kaybettirir, ayrı bir ceza formülüne gerek yok.
// 2) Günlük hedeflerde bugün eksik kalan her tekrar, önem derecesiyle orantılı puan KIRAR
//    (weight * eksikSayısı) — tek bir hedefte uzun seri tutup diğerlerini ihmal etmeyi caydırır.
// 3) Toplam ham puan, aşağıdaki Tiers tablosundaki kademelere göre 0-100'e eşleniyor — her kademede
//    aynı miktar ham puanın skora kattığı miktar bir öncekinden daha az (kademe ilerledikçe zorlaşıyor),
//    ama önceki (doygunluk/asla-100-olmayan) modelden farklı olarak skor gerçekten 100'e ULAŞABİLİYOR.
public class WillpowerManager : IWillpowerService
{
    private sealed record WillpowerTier(int ScoreFrom, int ScoreTo, double RawFrom, double RawTo, string Label);

    // Her kademenin ham puan aralığı önceki kademeden daha geniş — yani aynı 20 puanlık skor artışı
    // için gereken ham puan miktarı kademe ilerledikçe katlanarak büyüyor (10 -> 20 -> 80 -> 200 -> 400 -> 1000).
    private static readonly WillpowerTier[] Tiers =
    {
        new(0,  10,  0,    10,   "Çok Kolay"),
        new(10, 20,  10,   30,   "Kolay"),
        new(20, 40,  30,   110,  "Orta"),
        new(40, 60,  110,  310,  "Zor"),
        new(60, 80,  310,  710,  "Çok Zor"),
        new(80, 100, 710,  1710, "Neredeyse İmkansız")
    };

    private readonly IGoalRepository _goalRepository;
    private readonly IGoalStatusRepository _goalStatusRepository;

    public WillpowerManager(IGoalRepository goalRepository, IGoalStatusRepository goalStatusRepository)
    {
        _goalRepository = goalRepository;
        _goalStatusRepository = goalStatusRepository;
    }

    public async Task<ServiceResult<WillpowerScoreDto>> GetScoreAsync(int userId)
    {
        var goals = await _goalRepository.GetByUserIdAsync(userId);
        var allStatus = await _goalStatusRepository.GetAllByUserIdAsync(userId);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var yesterday = today.AddDays(-1);

        double rawPoints = 0;

        foreach (var goal in goals)
        {
            var goalStatuses = allStatus.Where(gs => gs.GoalId == goal.Id).ToList();
            var weight = (int)goal.Importance + 1; // Dusuk=0->1, Orta=1->2, Yuksek=2->3

            if (goalStatuses.Count > 0)
            {
                var dateSet = new HashSet<DateOnly>(goalStatuses.Select(gs => gs.ActivityDate));

                // Gün henüz bitmediği için bugün kayıt yoksa dünün serisiyle devam ediyoruz (1 günlük
                // tolerans); dün de yoksa seri gerçekten kopmuş sayılır ve katkı sıfırlanır.
                DateOnly? reference = dateSet.Contains(today) ? today
                    : dateSet.Contains(yesterday) ? yesterday
                    : null;

                if (reference != null)
                {
                    var streak = ComputeStreakAt(dateSet, reference.Value);
                    rawPoints += weight * streak;
                }
            }

            // Sadece günlük hedeflerde "bugün" anlamlı — haftalık/aylık hedefler her gün cezalandırılmaz.
            if (goal.PeriodUnit == PeriodUnit.Gun)
            {
                var doneToday = goalStatuses.Count(gs => gs.ActivityDate == today);
                var missing = Math.Max(0, goal.PeriodFrequency - doneToday);
                rawPoints -= weight * missing;
            }
        }

        rawPoints = Math.Max(0, rawPoints);

        var (score, label) = MapToTieredScore(rawPoints);

        return ServiceResult<WillpowerScoreDto>.Ok(new WillpowerScoreDto
        {
            Score = score,
            Label = label,
            RawPoints = Math.Round(rawPoints, 1),
            Tiers = Tiers.Select(t => new WillpowerTierDto
            {
                ScoreFrom = t.ScoreFrom,
                ScoreTo = t.ScoreTo,
                Label = t.Label
            }).ToList()
        });
    }

    private static (int Score, string Label) MapToTieredScore(double rawPoints)
    {
        foreach (var tier in Tiers)
        {
            if (rawPoints <= tier.RawTo)
            {
                var progress = (rawPoints - tier.RawFrom) / (tier.RawTo - tier.RawFrom);
                var score = tier.ScoreFrom + progress * (tier.ScoreTo - tier.ScoreFrom);
                return ((int)Math.Round(score), tier.Label);
            }
        }

        // Ham puan en üst kademeyi de aştıysa skor 100'de kilitlenir.
        var last = Tiers[^1];
        return (100, last.Label);
    }

    private static int ComputeStreakAt(HashSet<DateOnly> dateSet, DateOnly target)
    {
        var current = target;
        var streak = 0;

        while (dateSet.Contains(current))
        {
            streak++;
            current = current.AddDays(-1);
        }

        return streak;
    }
}
