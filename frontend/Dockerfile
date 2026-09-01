# Derleme aşaması: Angular'ı build edip statik dosyalara çeviriyoruz.
FROM node:20-alpine AS build
WORKDIR /app

# package*.json önce kopyalanıp npm ci yapılıyor — kaynak kod değişse bile bağımlılıklar
# aynıysa Docker bu katmanı cache'ten kullanır.
COPY package*.json ./
RUN npm ci

COPY . .
# "ng build" varsayılan olarak production konfigürasyonuyla derler (environment.ts kullanılır) —
# yani çıktı, gerçek API adresine (api.cansemihtopac.com) konuşacak şekilde derlenir.
RUN npx ng build

# Çalışma aşaması: statik dosyaları nginx ile sunuyoruz (önceki "npx serve" ile aynı işi görür).
FROM nginx:alpine AS runtime
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
