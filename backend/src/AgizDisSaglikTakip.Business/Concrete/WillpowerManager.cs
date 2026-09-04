using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.Willpower;
using AgizDisSaglikTakip.Business.Utilities;
using AgizDisSaglikTakip.Core.Utilities.Results;
using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.Entities;
using AgizDisSaglikTakip.Entities.Enums;

namespace AgizDisSaglikTakip.Business.Concrete;

// Matematiğin özeti:

// 1) Her hedefin "ham puana" katkısı = önem ağırlığı (Düşük=1, Orta=2, Yüksek=3) x güncel seri uzunluğu.

// 2) Günlük hedeflerde o gün eksik kalan her tekrar, önem derecesiyle orantılı puan KIRAR
//    (hedef o gün duraklatılmışsa bu ceza hiç uygulanmaz).

// 3) Toplam ham puan, aşağıdaki Tiers tablosundaki kademelere göre 0-100'e eşleniyor — (kademe ilerledikçe zorlaşıyor),


public class WillpowerManager : IWillpowerService
{
    private sealed record WillpowerTier(int ScoreFrom, int ScoreTo, double RawFrom, double RawTo, string Label);

    // Her kademenin ham puan aralığı önceki kademeden daha geniş — yani aynı 20 puanlık skor artışı
    // için gereken ham puan miktarı kademe ilerledikçe katlanarak büyüyor (10 -> 20 -> 80 -> 200 -> 400 -> 1000).
    private static readonly WillpowerTier[] Tiers =
    {
        new(0,  10,  0,    10,   "Çok Kolay"),         // 10 ham puan
        new(10, 20,  10,   30,   "Kolay"),             // 20
        new(20, 40,  30,   110,  "Orta"),              // 80
        new(40, 60,  110,  310,  "Zor"),               // 200
        new(60, 80,  310,  710,  "Çok Zor"),           // 400
        new(80, 100, 710,  1710, "Neredeyse İmkansız") // 1000
    };

    private readonly IGoalRepository _goalRepository;
    private readonly IGoalStatusRepository _goalStatusRepository;
    private readonly IGoalPauseRepository _goalPauseRepository;

    public WillpowerManager(IGoalRepository goalRepository, IGoalStatusRepository goalStatusRepository, IGoalPauseRepository goalPauseRepository)
    {
        _goalRepository = goalRepository;
        _goalStatusRepository = goalStatusRepository;
        _goalPauseRepository = goalPauseRepository;
    }

    public async Task<ServiceResult<WillpowerScoreDto>> GetScoreAsync(int userId)
    {
        var goals = await _goalRepository.GetByUserIdAsync(userId);
        var allStatus = await _goalStatusRepository.GetAllByUserIdAsync(userId);
        var pausedDatesByGoal = await BuildPausedDatesByGoalAsync(userId);
        var today = DateOnly.FromDateTime(DateTime.Today);

        var (score, label, rawPoints) = ComputeScoreAsOf(goals, allStatus, pausedDatesByGoal, today);

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

    public async Task<ServiceResult<List<WillpowerHistoryPointDto>>> GetHistoryAsync(int userId, string granularity)
    {
        var goals = await _goalRepository.GetByUserIdAsync(userId);
        var allStatus = await _goalStatusRepository.GetAllByUserIdAsync(userId);
        var pausedDatesByGoal = await BuildPausedDatesByGoalAsync(userId);
        var today = DateOnly.FromDateTime(DateTime.Today);

        var (count, stepBack) = granularity switch
        {
            "day" => (30, (Func<int, DateOnly>)(i => today.AddDays(-i))),
            "month" => (12, (Func<int, DateOnly>)(i => today.AddMonths(-i))),
            "year" => (4, (Func<int, DateOnly>)(i => today.AddYears(-i))),
            _ => (12, (Func<int, DateOnly>)(i => today.AddDays(-i * 7))) // "week" ve tanımsız değer varsayılanı
        };

        var points = new List<WillpowerHistoryPointDto>();

        for (var i = count - 1; i >= 0; i--)
        {
            var pointDate = stepBack(i);
            // O tarihte henüz oluşturulmamış hedefler o tarihin skoruna dahil edilmemeli.
            var goalsAtDate = goals.Where(g => DateOnly.FromDateTime(g.CreatedAt) <= pointDate).ToList();

            var (score, _, _) = ComputeScoreAsOf(goalsAtDate, allStatus, pausedDatesByGoal, pointDate);
            points.Add(new WillpowerHistoryPointDto { Date = pointDate, Score = score });
        }

        return ServiceResult<List<WillpowerHistoryPointDto>>.Ok(points);
    }

    private async Task<Dictionary<int, HashSet<DateOnly>>> BuildPausedDatesByGoalAsync(int userId)
    {
        var pauses = await _goalPauseRepository.GetAllByUserIdAsync(userId);
        return StreakCalculator.BuildPausedDatesByGoal(pauses, DateOnly.FromDateTime(DateTime.Today));
    }

    // Verilen tarihe kadarki (o tarih dahil) kayıtlara bakarak o tarih itibarıyla skoru hesaplar.
    private static (int Score, string Label, double RawPoints) ComputeScoreAsOf(
        List<Goal> goals, List<GoalStatus> allStatus, Dictionary<int, HashSet<DateOnly>> pausedDatesByGoal, DateOnly referenceDate)
    {
        var previousDate = referenceDate.AddDays(-1);
        double rawPoints = 0;

        foreach (var goal in goals)
        {
            var pausedDates = pausedDatesByGoal.GetValueOrDefault(goal.Id);
            rawPoints += ComputeGoalContribution(goal, allStatus, pausedDates, referenceDate, previousDate);
        }

        rawPoints = Math.Max(0, rawPoints);
        var (score, label) = MapToTieredScore(rawPoints);

        return (score, label, rawPoints);
    }

    // Tek bir hedefin toplam ham puana katkısını hesaplar (seriden gelen artı, günlük eksikten gelen eksi).
    private static double ComputeGoalContribution(
        Goal goal, List<GoalStatus> allStatus, HashSet<DateOnly>? pausedDates, DateOnly referenceDate, DateOnly previousDate)
    {
        var isPausedOnReferenceDate = pausedDates != null && pausedDates.Contains(referenceDate);
        var goalStatuses = allStatus
            .Where(gs => gs.GoalId == goal.Id && gs.ActivityDate <= referenceDate)
            .ToList();
        var weight = (int)goal.Importance + 1; // Dusuk=0->1, Orta=1->2, Yuksek=2->3
        double points = 0;

        if (goalStatuses.Count > 0 || isPausedOnReferenceDate)
        {
            var dateSet = new HashSet<DateOnly>(goalStatuses.Select(gs => gs.ActivityDate));
            var reference = ResolveStreakReferenceDate(dateSet, referenceDate, previousDate, isPausedOnReferenceDate);

            if (reference != null)
            {
                var streak = StreakCalculator.ComputeStreakAt(dateSet, reference.Value, pausedDates);
                points += weight * streak;
            }
        }

        // Sadece günlük hedeflerde "o gün" anlamlı — haftalık/aylık hedefler her gün cezalandırılmaz.
        // Hedef o gün duraklatılmışsa da ceza uygulanmaz — beklenti zaten yok.
        if (goal.PeriodUnit == PeriodUnit.Gun && !isPausedOnReferenceDate)
        {
            var doneOnDate = goalStatuses.Count(gs => gs.ActivityDate == referenceDate);
            var missing = Math.Max(0, goal.PeriodFrequency - doneOnDate);
            points -= weight * missing;
        }

        return points;
    }

    // O gün henüz bitmediği için o günde kayıt yoksa bir önceki günün serisiyle devam ediyoruz
    // (1 günlük tolerans); duraklatılmışsa "donmuş" seriyi geriye doğru bulmak için referans günü
    // yine bugün kabul ediliyor (StreakCalculator duraklatılan günleri atlayarak son gerçek kayda
    // kadar geri yürüyor).
    private static DateOnly? ResolveStreakReferenceDate(
        HashSet<DateOnly> dateSet, DateOnly referenceDate, DateOnly previousDate, bool isPausedOnReferenceDate)
    {
        if (dateSet.Contains(referenceDate))
            return referenceDate;
        if (dateSet.Contains(previousDate))
            return previousDate;
        if (isPausedOnReferenceDate)
            return referenceDate;
        return null;
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
}
