namespace AgizDisSaglikTakip.Business.DTOs.Willpower;

public class WillpowerScoreDto
{
    // 0-100 arası, ekranda gösterilecek olan.
    public int Score { get; set; }
    // Skorun düştüğü kademenin adı (ör. "Zor") — ekranda skorun yanında gösterilir.
    public string Label { get; set; } = string.Empty;
    // Ham puan — şeffaflık/hata ayıklama için, ekranda göstermek zorunlu değil.
    public double RawPoints { get; set; }
    // Kullanıcıya tüm kademe skalasını göstermek için — backend tek doğru kaynak.
    public List<WillpowerTierDto> Tiers { get; set; } = new();
}

public class WillpowerTierDto
{
    public int ScoreFrom { get; set; }
    public int ScoreTo { get; set; }
    public string Label { get; set; } = string.Empty;
}
