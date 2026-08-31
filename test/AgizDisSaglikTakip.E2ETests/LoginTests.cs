using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace AgizDisSaglikTakip.E2ETests;

// Uçtan uca (end-to-end) test: kod içindeki bir metodu değil, gerçek bir tarayıcıyı
// kod ile yönetip kullanıcının yaşadığı akışı baştan sona doğruluyor.
// Test edilen site: Angular geliştirme sunucusu (ng serve, localhost:4200) — bilerek
// localhost:4300'deki (üretim build'i) DEĞİL, çünkü o build genel internet üzerinden
// (api.cansemihtopac.com -> Cloudflare tüneli) backend'e ulaşıyor; test bu ekstra ağ
// bağımlılığına muhtaç kalmasın diye "ng serve" ile doğrudan localhost:5299'a konuşuyoruz.
// Backend'in de (localhost:5299) aynı anda ayakta olması gerekiyor.
public class LoginTests
{
    private const string BaseUrl = "http://localhost:4200";

    [Fact]
    public void DemoGirisi_AnaSayfayaYonlendirmeli()
    {
        var options = new ChromeOptions();
        // Headless: gerçek bir pencere açmadan, arka planda çalışır — CI ortamları için de gerekli.
        options.AddArgument("--headless=new");
        options.AddArgument("--window-size=1280,900");

        using var driver = new ChromeDriver(options);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        driver.Navigate().GoToUrl($"{BaseUrl}/login");

        var demoButton = wait.Until(d => d.FindElement(By.CssSelector(".demo-btn")));
        demoButton.Click();

        // Demo girişi backend'e istek atıp token aldıktan sonra /home'a yönlendiriyor —
        // bu ağ isteği anlık olmadığı için URL'in değişmesini bekliyoruz.
        wait.Until(d => d.Url.Contains("/home"));

        Assert.Contains("/home", driver.Url);
        Assert.Contains("Hoş geldin", driver.PageSource);
    }
}
