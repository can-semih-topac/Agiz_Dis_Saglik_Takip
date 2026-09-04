# Derleme aşaması: SDK image'ı sadece build sırasında lazım, çalışma zamanında gereksiz büyüklük.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Önce sadece .csproj dosyalarını kopyalayıp restore yapıyoruz — kaynak kod değişse bile
# paketler değişmediyse Docker bu katmanı cache'ten kullanır, her seferinde yeniden indirmez.
COPY src/AgizDisSaglikTakip.Entities/AgizDisSaglikTakip.Entities.csproj src/AgizDisSaglikTakip.Entities/
COPY src/AgizDisSaglikTakip.Core/AgizDisSaglikTakip.Core.csproj src/AgizDisSaglikTakip.Core/
COPY src/AgizDisSaglikTakip.DataAccess/AgizDisSaglikTakip.DataAccess.csproj src/AgizDisSaglikTakip.DataAccess/
COPY src/AgizDisSaglikTakip.Business/AgizDisSaglikTakip.Business.csproj src/AgizDisSaglikTakip.Business/
COPY src/AgizDisSaglikTakip.WebAPI/AgizDisSaglikTakip.WebAPI.csproj src/AgizDisSaglikTakip.WebAPI/
RUN dotnet restore src/AgizDisSaglikTakip.WebAPI/AgizDisSaglikTakip.WebAPI.csproj

COPY src/ src/
RUN dotnet publish src/AgizDisSaglikTakip.WebAPI/AgizDisSaglikTakip.WebAPI.csproj -c Release -o /app --no-restore

# Çalışma aşaması: sadece ASP.NET Core runtime'ı içeren küçük image.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# .NET 8'in varsayılan container portu 8080 — docker-compose'da bunu 5299'a map ediyoruz,
# böylece Cloudflare Tünelinin config.yml'i (localhost:5299) hiç değişmiyor.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Container'ı root yerine aspnet:8.0 imajına gömülü "app" kullanıcısıyla (UID 1654) çalıştırıyoruz
# (SonarQube S6471) — container bir şekilde ele geçirilirse root yetkisiyle host'a sızma riskini
# azaltıyor. Kalıcı upload klasörü (backend-uploads volume'ü) bu kullanıcıya devredildi.
USER app

ENTRYPOINT ["dotnet", "AgizDisSaglikTakip.WebAPI.dll"]
