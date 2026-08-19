using AgizDisSaglikTakip.Entities;

namespace AgizDisSaglikTakip.Business.Utilities;

// GoalStatusManager (seri gösterimi) ve WillpowerManager (irade puanı) aynı seri mantığını
// kullanıyor — duraklatma eklenince ikisinin de aynı şekilde davranması gerektiği için buraya taşındı.
public static class StreakCalculator
{
    // Bir hedefin duraklatma aralıklarını, o hedefin GoalId'sine göre gruplayıp her birini
    // gün gün açarak bir HashSet'e çeviriyor — aralık kontrolü yerine tek tek gün kontrolü
    // yapmak seri hesaplamasında çok daha basit.
    public static Dictionary<int, HashSet<DateOnly>> BuildPausedDatesByGoal(List<GoalPause> pauses, DateOnly today)
    {
        var result = new Dictionary<int, HashSet<DateOnly>>();

        foreach (var pause in pauses)
        {
            if (!result.TryGetValue(pause.GoalId, out var dates))
            {
                dates = new HashSet<DateOnly>();
                result[pause.GoalId] = dates;
            }

            var end = pause.EndDate ?? today;
            for (var d = pause.StartDate; d <= end; d = d.AddDays(1))
            {
                dates.Add(d);
            }
        }

        return result;
    }

    // Verilen tarihten geriye doğru, boşluksuz kaç gün kayıt var (o tarih dahil).
    // Duraklatılmış günler seriyi ne bozar ne de sayar — sanki hiç yaşanmamış gibi atlanır.
    public static int ComputeStreakAt(HashSet<DateOnly> recordedDates, DateOnly target, HashSet<DateOnly>? pausedDates)
    {
        var current = target;
        var streak = 0;

        while (true)
        {
            if (recordedDates.Contains(current))
            {
                streak++;
            }
            else if (pausedDates == null || !pausedDates.Contains(current))
            {
                break;
            }

            current = current.AddDays(-1);
        }

        return streak;
    }

    // Sıralı, tekrarsız tarih listesindeki en uzun ardışık diziyi bulur — iki kayıt arasındaki
    // boşluk tamamen duraklatma ile kapanıyorsa seri kopmamış sayılır.
    public static int ComputeLongestStreak(List<DateOnly> distinctSortedDates, HashSet<DateOnly>? pausedDates)
    {
        if (distinctSortedDates.Count == 0)
            return 0;

        var longest = 1;
        var current = 1;

        for (var i = 1; i < distinctSortedDates.Count; i++)
        {
            if (IsConsecutiveOrBridgedByPause(distinctSortedDates[i - 1], distinctSortedDates[i], pausedDates))
            {
                current++;
            }
            else
            {
                current = 1;
            }

            longest = Math.Max(longest, current);
        }

        return longest;
    }

    private static bool IsConsecutiveOrBridgedByPause(DateOnly from, DateOnly to, HashSet<DateOnly>? pausedDates)
    {
        if (to == from.AddDays(1))
            return true;

        if (pausedDates == null)
            return false;

        for (var d = from.AddDays(1); d < to; d = d.AddDays(1))
        {
            if (!pausedDates.Contains(d))
                return false;
        }

        return true;
    }
}
