using System.Text;
using AgizDisSaglikTakip.Business;
using AgizDisSaglikTakip.Core;
using AgizDisSaglikTakip.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

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
