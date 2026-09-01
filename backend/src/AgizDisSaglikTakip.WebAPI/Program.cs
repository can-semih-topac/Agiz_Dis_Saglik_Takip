using System.Text;
using AgizDisSaglikTakip.Business;
using AgizDisSaglikTakip.Business.Seed;
using AgizDisSaglikTakip.Core;
using AgizDisSaglikTakip.Core.Utilities.Security.Encryption;
using AgizDisSaglikTakip.DataAccess;
using AgizDisSaglikTakip.DataAccess.Contexts;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Sentry: işlenmemiş exception'ları ve ASP.NET Core isteklerini otomatik yakalayıp
// sentry.io'ya gönderiyor. DSN, JWT/AES anahtarları gibi user-secrets'ta duruyor —
// koda veya appsettings.json'a gömülmüyor.
builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"];
    // Release Health: uygulamanın "sağlıklı" oturum/istek oranını izlemek için.
    options.AutoSessionTracking = true;
});

// Redis: şifre sıfırlama kodu gibi kısa ömürlü verileri SQL Server yerine burada,
// kendiliğinden süresi dolacak şekilde tutacağız (bkz. AuthManager).
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Configuration"] ?? "localhost:6379";
});

// ElasticSearch: admin panelindeki loglarda tam metin arama yapabilmek için (bkz. LogManager).
// ElasticsearchClient thread-safe ve oluşturulması maliyetli — tek bir örnek (singleton) yeterli.
builder.Services.AddSingleton(new ElasticsearchClient(
    new Uri(builder.Configuration["Elasticsearch:Uri"] ?? "http://127.0.0.1:9200")));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Web API şablonunda MVC'nin aksine varsayılan olarak wwwroot klasörü oluşturulmuyor;
// WebRootPath boş gelirse elle hesaplayıp klasörü oluşturuyoruz.
if (string.IsNullOrEmpty(builder.Environment.WebRootPath))
{
    builder.Environment.WebRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
}
Directory.CreateDirectory(builder.Environment.WebRootPath);

builder.Services.AddDataAccessServices(builder.Configuration);
builder.Services.AddCoreServices(builder.Configuration, builder.Environment.WebRootPath);
builder.Services.AddBusinessServices();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Varsayılanda ASP.NET Core "sub" gibi kısa claim adlarını uzun URI'lere eşliyor; bunu kapatıp token'a ne yazdıysak Controller'da onu okuyoruz.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
            // [Authorize(Roles = "...")] varsayılan olarak uzun bir claim URI'sine bakıyor; token'a
            // kısa "role" adıyla yazdığımız için burada da aynı adı söylememiz lazım, yoksa admin dahil
            // kimse Roles kontrolünden geçemez.
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();

// Angular dev server (localhost:4200) ve canlıdaki frontend (Cloudflare Tunnel üzerinden
// ads.cansemihtopac.com) farklı origin'ler olduğu için tarayıcı bunlara açıkça izin
// vermedikçe backend'e istek atmayı engelliyor (CORS).
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://ads.cansemihtopac.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Migration'lar EF Core'un kendi __EFMigrationsHistory tablosuyla takip ediliyor — zaten
// uygulanmışsa bu çağrı hiçbir şey yapmaz (milisaniyeler sürer), boş bir veritabanında
// (taze "docker compose up" gibi) ise şemayı sıfırdan kurar. Demo şablon verisi de aynı
// mantıkla idempotent: sadece daha önce hiç oluşturulmamışsa ekleniyor (bkz. DemoDataSeeder).
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();

    var encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();
    await DemoDataSeeder.SeedAsync(dbContext, encryptionService);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Environment.WebRootFileProvider = new PhysicalFileProvider(app.Environment.WebRootPath);
app.UseStaticFiles();

app.UseCors("AllowAngularDev");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Content("<h1>Backend çalışıyor</h1>", "text/html; charset=utf-8"));
app.MapControllers();

app.Run();
