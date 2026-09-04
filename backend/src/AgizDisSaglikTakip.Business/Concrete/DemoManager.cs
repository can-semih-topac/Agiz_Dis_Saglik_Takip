using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.Constants;
using AgizDisSaglikTakip.Business.DTOs.Auth;
using AgizDisSaglikTakip.Core.Utilities.Results;
using AgizDisSaglikTakip.Core.Utilities.Security.Hashing;
using AgizDisSaglikTakip.Core.Utilities.Security.Jwt;
using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.Entities;
using AgizDisSaglikTakip.Entities.Enums;

namespace AgizDisSaglikTakip.Business.Concrete;

public class DemoManager : IDemoService
{
    private readonly IUserRepository _userRepository;
    private readonly IGoalRepository _goalRepository;
    private readonly IGoalStatusRepository _goalStatusRepository;
    private readonly IStatusNoteRepository _statusNoteRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly JwtSettings _jwtSettings;

    public DemoManager(
        IUserRepository userRepository,
        IGoalRepository goalRepository,
        IGoalStatusRepository goalStatusRepository,
        IStatusNoteRepository statusNoteRepository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IRefreshTokenRepository refreshTokenRepository,
        JwtSettings jwtSettings)
    {
        _userRepository = userRepository;
        _goalRepository = goalRepository;
        _goalStatusRepository = goalStatusRepository;
        _statusNoteRepository = statusNoteRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtSettings = jwtSettings;
    }

    public async Task<ServiceResult<LoginResultDto>> EnterDemoAsync()
    {
        var template = await _userRepository.GetByEmailAsync(DemoAccountConstants.TemplateEmail);
        var demoUser = await _userRepository.GetByEmailAsync(DemoAccountConstants.DemoEmail);
        if (template == null || demoUser == null)
            return ServiceResult<LoginResultDto>.Fail("Demo hesabı henüz hazırlanmamış.");

        // Önceki ziyaretçi profil bilgilerini (isim, şifre vb.) değiştirmiş olabilir — hesabı
        // tamamen şablondaki haline döndürüyoruz, kalıcı bir iz kalmasın diye.
        demoUser.FullName = template.FullName;
        demoUser.PhoneNumber = template.PhoneNumber;
        demoUser.BirthDate = template.BirthDate;
        // Şifre boş bırakılırsa "güvenliğiniz için şifre belirleyin" bildirimi çıkıyor — demoda
        // anlamsız olduğu için hiç kimsenin bilmediği rastgele bir şifre atayıp bildirimi engelliyoruz.
        demoUser.PasswordHash = _passwordHasher.Hash(Guid.NewGuid().ToString("N"));
        demoUser.MustChangePassword = false;
        await _userRepository.UpdateAsync(demoUser);

        // Önceki ziyaretin verilerini tamamen temizliyoruz: notlar goal statüslere Restrict ile
        // bağlı olduğu için önce onlar, sonra hedefler (GoalStatus'lar hedef üzerinden Cascade siliniyor).
        // Demo verisi zaten geçici/tekrar üretilen olduğu için burası kalıcı (hard) silme — bu yüzden
        // yumuşak silinmiş (bir önceki ziyaretçinin sildiği) kayıtları da "IncludingDeleted" ile
        // dahil ediyoruz, aksi halde onlar görünmez ama kalıcı olarak birikirdi.
        var oldNotes = await _statusNoteRepository.GetAllByUserIdIncludingDeletedAsync(demoUser.Id);
        if (oldNotes.Count > 0)
            await _statusNoteRepository.DeleteRangeAsync(oldNotes);

        var oldGoals = await _goalRepository.GetByUserIdIncludingDeletedAsync(demoUser.Id);
        if (oldGoals.Count > 0)
            await _goalRepository.DeleteRangeAsync(oldGoals);

        var templateGoals = await _goalRepository.GetByUserIdAsync(template.Id);
        var templateStatuses = await _goalStatusRepository.GetAllByUserIdAsync(template.Id);
        var templateNotes = await _statusNoteRepository.GetAllByUserIdAsync(template.Id);

        var shiftDays = ComputeShiftDays(templateStatuses);

        // Önce hedefleri (GoalStatus'ları da navigation üzerinden aynı grafın parçası olarak) kopyalıyoruz.
        var goalIdMap = new Dictionary<int, Goal>();
        var clonedGoals = new List<Goal>();
        foreach (var g in templateGoals)
        {
            var clone = new Goal
            {
                UserId = demoUser.Id,
                Title = g.Title,
                Description = g.Description,
                PeriodUnit = g.PeriodUnit,
                PeriodFrequency = g.PeriodFrequency,
                Importance = g.Importance,
                TrackingType = g.TrackingType,
                CreatedAt = g.CreatedAt.AddDays(shiftDays)
            };
            clonedGoals.Add(clone);
            goalIdMap[g.Id] = clone;
        }

        var statusIdMap = new Dictionary<int, GoalStatus>();
        foreach (var gs in templateStatuses)
        {
            if (!goalIdMap.TryGetValue(gs.GoalId, out var clonedGoal))
                continue;

            var clonedStatus = new GoalStatus
            {
                Goal = clonedGoal,
                ActivityDate = gs.ActivityDate.AddDays(shiftDays),
                ActivityTime = gs.ActivityTime,
                DurationMinutes = gs.DurationMinutes,
                CreatedAt = gs.CreatedAt.AddDays(shiftDays)
            };
            clonedGoal.GoalStatuses.Add(clonedStatus);
            statusIdMap[gs.Id] = clonedStatus;
        }

        // Goal ve GoalStatus'lar aynı grafın parçası olduğu için tek SaveChanges ile ekleniyor;
        // az sonra notları eklerken StatusNote.GoalStatus buradaki (artık gerçek Id'si olan) nesneleri referans alacak.
        await _goalRepository.AddRangeAsync(clonedGoals);

        // Yukarıdaki kaydırma "bugün"ü bilerek boş bırakıyor (ziyaretçi ilk kaydı kendi eklesin diye).
        // Ama hem "Bugün Yapılanlar" hem "Yapılması Gerekenler" ilk açılışta örnekli görünsün istendiği
        // için "Gargara Kullanma"ya bugünden tek bir kayıt ekliyoruz: frekansı (3) doldurmuyor, yani
        // hem bugün yapılan bir örnek olarak görünüyor hem de hâlâ "yapılması gereken" listesinde kalıp
        // ziyaretçinin hızlı ekleme özelliğini denemesine imkan tanıyor. Diğer günlük hedefler bugün
        // hiç işlenmediği için zaten "Yapılması Gerekenler"de örnek olarak duruyor.
        var gargaraGoal = clonedGoals.FirstOrDefault(g => g.Title == "Gargara Kulllanma");
        if (gargaraGoal != null)
        {
            await _goalStatusRepository.AddAsync(new GoalStatus
            {
                GoalId = gargaraGoal.Id,
                ActivityDate = DateOnly.FromDateTime(DateTime.Today),
                ActivityTime = new TimeOnly(8, 0),
                CreatedAt = DateTime.Now
            });
        }

        var clonedNotes = new List<StatusNote>();
        foreach (var note in templateNotes)
        {
            GoalStatus? clonedStatus = null;
            if (note.GoalStatusId.HasValue)
                statusIdMap.TryGetValue(note.GoalStatusId.Value, out clonedStatus);

            clonedNotes.Add(new StatusNote
            {
                UserId = demoUser.Id,
                Description = note.Description,
                ImagePath = note.ImagePath,
                GoalStatus = clonedStatus,
                CreatedAt = note.CreatedAt.AddDays(shiftDays)
            });
        }

        if (clonedNotes.Count > 0)
            await _statusNoteRepository.AddRangeAsync(clonedNotes);

        var token = _tokenService.CreateToken(demoUser.Id, demoUser.Email, demoUser.Role.ToString());
        var refreshToken = await IssueRefreshTokenAsync(demoUser.Id);
        var result = new LoginResultDto
        {
            Token = token,
            RefreshToken = refreshToken,
            Email = demoUser.Email,
            FullName = demoUser.FullName,
            IsAdmin = demoUser.Role == Role.Admin
        };

        return ServiceResult<LoginResultDto>.Ok(result, "Demo hesabına giriş yapıldı.");
    }

    // AuthManager.IssueRefreshTokenAsync ile aynı mantık — demo hesabı da (kısa ömürlü access
    // token'la tutarlı olsun diye) bir refresh token alır, aksi halde 15 dakikada bir sayfayı
    // yenileyen ziyaretçi oturumdan atılırdı.
    private async Task<string> IssueRefreshTokenAsync(int userId)
    {
        await _refreshTokenRepository.DeleteExpiredForUserAsync(userId);

        var refreshToken = _tokenService.GenerateRefreshToken();

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            UserId = userId,
            TokenHash = _tokenService.HashRefreshToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        });

        return refreshToken;
    }

    // Şablondaki en güncel kaydı "dün"e taşıyacak kayma miktarını hesaplar — böylece demo, ne zaman
    // ziyaret edilirse edilsin güncel görünür ve "bugün" boş kalır (ziyaretçi ilk kaydı kendisi ekleyebilsin diye).
    private static int ComputeShiftDays(List<GoalStatus> templateStatuses)
    {
        if (templateStatuses.Count == 0)
            return 0;

        var maxDate = templateStatuses.Max(gs => gs.ActivityDate);
        var yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        return yesterday.DayNumber - maxDate.DayNumber;
    }
}
