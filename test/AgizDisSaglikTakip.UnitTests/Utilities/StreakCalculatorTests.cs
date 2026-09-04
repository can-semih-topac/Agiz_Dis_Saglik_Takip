using AgizDisSaglikTakip.Business.Utilities;
using AgizDisSaglikTakip.Entities;

namespace AgizDisSaglikTakip.UnitTests.Utilities;

public class StreakCalculatorTests
{
    private static DateOnly D(int day) => new(2026, 1, day);

    [Fact]
    public void BuildPausedDatesByGoal_KapaliAralik_BaslangicVeBitisDahilTumGunleriIcerir()
    {
        var pauses = new List<GoalPause>
        {
            new() { GoalId = 1, StartDate = D(1), EndDate = D(3) }
        };

        var result = StreakCalculator.BuildPausedDatesByGoal(pauses, today: D(10));

        var dates = result[1];
        Assert.Equal(3, dates.Count);
        Assert.Contains(D(1), dates);
        Assert.Contains(D(2), dates);
        Assert.Contains(D(3), dates);
    }

    [Fact]
    public void BuildPausedDatesByGoal_AcikAralik_BugüneKadarUzanir()
    {
        var pauses = new List<GoalPause>
        {
            new() { GoalId = 1, StartDate = D(3), EndDate = null }
        };

        var result = StreakCalculator.BuildPausedDatesByGoal(pauses, today: D(5));

        var dates = result[1];
        Assert.Equal(3, dates.Count);
        Assert.Contains(D(3), dates);
        Assert.Contains(D(5), dates);
    }

    [Fact]
    public void BuildPausedDatesByGoal_FarkliHedefler_AyriGruplanir()
    {
        var pauses = new List<GoalPause>
        {
            new() { GoalId = 1, StartDate = D(1), EndDate = D(1) },
            new() { GoalId = 2, StartDate = D(1), EndDate = D(1) }
        };

        var result = StreakCalculator.BuildPausedDatesByGoal(pauses, today: D(10));

        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey(1));
        Assert.True(result.ContainsKey(2));
    }

    [Fact]
    public void ComputeStreakAt_ArdaSizKayitlar_HepsiniSayar()
    {
        var recorded = new HashSet<DateOnly> { D(3), D(4), D(5) };

        var streak = StreakCalculator.ComputeStreakAt(recorded, target: D(5), pausedDates: null);

        Assert.Equal(3, streak);
    }

    [Fact]
    public void ComputeStreakAt_HedefGunKayitliDegil_SifirDoner()
    {
        var recorded = new HashSet<DateOnly> { D(1), D(2) };

        var streak = StreakCalculator.ComputeStreakAt(recorded, target: D(5), pausedDates: null);

        Assert.Equal(0, streak);
    }

    [Fact]
    public void ComputeStreakAt_DuraklatilmisGunSeriyiBozmazAmaSaymazDa()
    {
        // 5 ve 3 kayıtlı, 4 duraklatılmış (kaydı yok) -> seri kırılmadan 3'e kadar devam etmeli.
        var recorded = new HashSet<DateOnly> { D(3), D(5) };
        var paused = new HashSet<DateOnly> { D(4) };

        var streak = StreakCalculator.ComputeStreakAt(recorded, target: D(5), pausedDates: paused);

        Assert.Equal(2, streak);
    }

    [Fact]
    public void ComputeStreakAt_DuraklatilmamisBosGunSeriyiKirar()
    {
        var recorded = new HashSet<DateOnly> { D(3), D(5) };

        var streak = StreakCalculator.ComputeStreakAt(recorded, target: D(5), pausedDates: null);

        // 4. gün ne kayıtlı ne duraklatılmış -> seri orada durur, sadece 5. gün sayılır.
        Assert.Equal(1, streak);
    }

    [Fact]
    public void ComputeLongestStreak_BosListe_SifirDoner()
    {
        var result = StreakCalculator.ComputeLongestStreak(new List<DateOnly>(), pausedDates: null);

        Assert.Equal(0, result);
    }

    [Fact]
    public void ComputeLongestStreak_ArdisikTarihler_TumUzunluguDoner()
    {
        var dates = new List<DateOnly> { D(1), D(2), D(3) };

        var result = StreakCalculator.ComputeLongestStreak(dates, pausedDates: null);

        Assert.Equal(3, result);
    }

    [Fact]
    public void ComputeLongestStreak_KopukTarihlerDuraklatmaOlmadan_BirDoner()
    {
        var dates = new List<DateOnly> { D(1), D(10) };

        var result = StreakCalculator.ComputeLongestStreak(dates, pausedDates: null);

        Assert.Equal(1, result);
    }

    [Fact]
    public void ComputeLongestStreak_AradakiBoslukTamamenDuraklatmaylaKapaniyorsa_SeriKopmaz()
    {
        var dates = new List<DateOnly> { D(1), D(5) };
        var paused = new HashSet<DateOnly> { D(2), D(3), D(4) };

        var result = StreakCalculator.ComputeLongestStreak(dates, pausedDates: paused);

        Assert.Equal(2, result);
    }

    [Fact]
    public void ComputeLongestStreak_AradakiBoslukKismenDuraklatilmissa_SeriKopar()
    {
        var dates = new List<DateOnly> { D(1), D(5) };
        // Sadece 2 ve 3 duraklatılmış, 4 duraklatılmamış -> köprü tam kapanmıyor.
        var paused = new HashSet<DateOnly> { D(2), D(3) };

        var result = StreakCalculator.ComputeLongestStreak(dates, pausedDates: paused);

        Assert.Equal(1, result);
    }
}
