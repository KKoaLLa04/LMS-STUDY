# Build context của Dockerfile này là ROOT của repo (không phải thư mục Backend/),
# vì cần COPY được cả Frontend/ lẫn Backend/. Trên Render: đặt
# "Docker Build Context Directory" = . và "Dockerfile Path" = Backend/Dockerfile.

FROM node:20-alpine AS frontend-build
WORKDIR /frontend
COPY Frontend/package.json Frontend/package-lock.json ./
RUN npm ci
COPY Frontend/ .
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Backend/Backend.csproj .
RUN dotnet restore
COPY Backend/. .
# Angular build ra dist/frontend/browser (SPA thuần, không SSR) — copy đè vào wwwroot
# để ASP.NET Core serve như static file (xem app.MapFallbackToFile("index.html") trong Program.cs).
COPY --from=frontend-build /frontend/dist/frontend/browser/. ./wwwroot/
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Render cấp cổng động qua biến môi trường PORT lúc container khởi động (không phải build time),
# nên phải resolve bằng shell form thay vì ENV tĩnh.
ENTRYPOINT ["/bin/sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet Backend.dll"]
