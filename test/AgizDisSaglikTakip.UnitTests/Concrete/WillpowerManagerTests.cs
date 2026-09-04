using AgizDisSaglikTakip.Business.Concrete;
using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.Entities;
using AgizDisSaglikTakip.Entities.Enums;
using Moq;

namespace AgizDisSaglikTakip.UnitTests.Concrete;

// WillpowerManager, DateTime.Today'e göre çalışıyor (enjekte edilebilir bir saat soyutlaması
// yok) — bu yüzden testler tarihleri sabit değil, "bugün"e göre bağıl kuruyor.
public class WillpowerManagerTests
{
    private const int UserId = 1;
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    private static WillpowerManager CreateManager(
        List<Goal> goals, List<GoalStatus> statuses, List<GoalPause> pauses)
    {
        var goalRepo = new Mock<IGoalRepository>();
        goalRepo.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync(goals);

        var statusRepo = new Mock<IGoalStatusRepository>();
        statusRepo.Setup(r => r.GetAllByUserIdAsync(UserId)).ReturnsAsync(statuses);

        var pauseRepo = new Mock<IGoalPauseRepository>();
        pauseRepo.Setup(r => r.GetAllByUserIdAsync(UserId)).ReturnsAsync(pauses);

        return new WillpowerManager(goalRepo.Object, statusRepo.Object, pauseRepo.Object);
    }

    private static Goal CreateGoal(int id, Importance importance, PeriodUnit periodUnit, int periodFrequency) => new()
    {
        Id = id,
        UserId = UserId,
        Title = $"Hedef {id}",
        Description = "Test",
        Importance = importance,
        PeriodUnit = periodUnit,
        PeriodFrequency = periodFrequency,
        TrackingType = TrackingType.Yapildi,
        CreatedAt = DateTime.Today.AddYears(-1)
    };

    private static GoalStatus CreateStatus(int goalId, DateOnly date) => new()
    {
        GoalId = goalId,
        ActivityDate = date,
        ActivityTime = new TimeOnly(8, 0),
        CreatedAt = date.ToDateTime(TimeOnly.MinValue)
    };

    [Fact]
    public async Task GetScoreAsync_HicHedefYokken_SkorSifirVeCokKolayDoner()
    {
        var manager = CreateManager(new List<Goal>(), new List<GoalStatus>(), new List<GoalPause>());

        var result = await manager.GetScoreAsync(UserId);

        Assert.True(result.Success);
        Assert.Equal(0, result.Data!.Score);
        Assert.Equal("Çok Kolay", result.Data.Label);
    }

    [Fact]
    public async Task GetScoreAsync_BugunYapilmamisGunlukHedef_CezaUygulanirVeSifiraKirpilir()
    {
        var goal = CreateGoal(1, Importance.Dusuk, PeriodUnit.Gun, periodFrequency: 1);
        var manager = CreateManager(new List<Goal> { goal }, new List<GoalStatus>(), new List<GoalPause>());

        var result = await manager.GetScoreAsync(UserId);

        // Ham puan negatife düşer (-1) ama skor asla 0'ın altına inmiyor.
        Assert.Equal(0, result.Data!.Score);
        Assert.Equal(0, result.Data.RawPoints);
    }

    [Fact]
    public async Task GetScoreAsync_UcGunlukArdisikSeri_HamPuanAgirlikCarpiSeriyeEsitOlur()
    {
        // Aylık hedef seçildi ki günlük ceza mantığı devreye girmesin, sadece seri katkısını izole edelim.
        var goal = CreateGoal(1, Importance.Yuksek, PeriodUnit.Ay, periodFrequency: 1);
        var statuses = new List<GoalStatus>
        {
            CreateStatus(1, Today),
            CreateStatus(1, Today.AddDays(-1)),
            CreateStatus(1, Today.AddDays(-2))
        };
        var manager = CreateManager(new List<Goal> { goal }, statuses, new List<GoalPause>());

        var result = await manager.GetScoreAsync(UserId);

        // weight = Yuksek(2)+1 = 3, streak = 3 gün -> ham puan = 9.
        Assert.Equal(9, result.Data!.RawPoints);
    }

    [Fact]
    public async Task GetScoreAsync_DuraklatilmisGunlukHedef_OGunCezaAlmaz()
    {
        // Aynı özellikte iki hedef: biri bugün duraklatılmış, diğeri değil. İkisi de kayıtsız.
        // Duraklatılan hedefin katkısı 0 olmalı, duraklatılmayanınki negatif olmalı.
        var pausedGoal = CreateGoal(1, Importance.Dusuk, PeriodUnit.Gun, periodFrequency: 1);
        var activeGoal = CreateGoal(2, Importance.Dusuk, PeriodUnit.Gun, periodFrequency: 1);
        // Üçüncü, pozitif katkılı bir hedef ekliyoruz ki toplam 0'a kırpılıp fark gizlenmesin.
        var boosterGoal = CreateGoal(3, Importance.Yuksek, PeriodUnit.Ay, periodFrequency: 1);

        var statuses = new List<GoalStatus> { CreateStatus(3, Today) };
        var pauses = new List<GoalPause> { new() { GoalId = 1, StartDate = Today, EndDate = Today } };

        var manager = CreateManager(
            new List<Goal> { pausedGoal, activeGoal, boosterGoal }, statuses, pauses);

        var result = await manager.GetScoreAsync(UserId);

        // boosterGoal: weight=3, streak=1 gün (sadece bugün) -> +3
        // pausedGoal: duraklatıldığı için ceza yok -> 0
        // activeGoal: duraklatılmadı, kayıtsız, weight=1, missing=1 -> -1
        // toplam = 3 + 0 - 1 = 2
        Assert.Equal(2, result.Data!.RawPoints);
    }

    [Fact]
    public async Task GetScoreAsync_CokYuksekHamPuan_Skor100deKilitlenir()
    {
        // Son kademenin üst sınırını (1710) asacak kadar uzun bir seri üretiyoruz.
        var goal = CreateGoal(1, Importance.Yuksek, PeriodUnit.Ay, periodFrequency: 1);
        var statuses = new List<GoalStatus>();
        for (var i = 0; i < 600; i++)
            statuses.Add(CreateStatus(1, Today.AddDays(-i)));

        var manager = CreateManager(new List<Goal> { goal }, statuses, new List<GoalPause>());

        var result = await manager.GetScoreAsync(UserId);

        Assert.Equal(100, result.Data!.Score);
        Assert.Equal("Neredeyse İmkansız", result.Data.Label);
    }

    [Theory]
    [InlineData("day", 30)]
    [InlineData("month", 12)]
    [InlineData("year", 4)]
    [InlineData("week", 12)]
    [InlineData("taninmayan-deger", 12)]
    public async Task GetHistoryAsync_GranulariteyeGoreDogruNoktaSayisiDoner(string granularity, int expectedCount)
    {
        var manager = CreateManager(new List<Goal>(), new List<GoalStatus>(), new List<GoalPause>());

        var result = await manager.GetHistoryAsync(UserId, granularity);

        Assert.Equal(expectedCount, result.Data!.Count);
    }

    [Fact]
    public async Task GetHistoryAsync_HenuzOlusturulmamisHedef_GecmisNoktalaraDahilEdilmez()
    {
        // Hedef sadece 2 gün önce oluşturulmuş -> 3 gün önceki geçmiş noktasında hiç var olmamalı,
        // dolayısıyla o noktada hiçbir katkısı olmamalı (skor 0 kalmalı).
        var goal = CreateGoal(1, Importance.Yuksek, PeriodUnit.Ay, periodFrequency: 1);
        goal.CreatedAt = DateTime.Today.AddDays(-2);
        var statuses = new List<GoalStatus> { CreateStatus(1, Today) };

        var manager = CreateManager(new List<Goal> { goal }, statuses, new List<GoalPause>());

        var result = await manager.GetHistoryAsync(UserId, "day");

        var threeDaysAgo = result.Data!.Single(p => p.Date == Today.AddDays(-3));
        Assert.Equal(0, threeDaysAgo.Score);
    }
}
