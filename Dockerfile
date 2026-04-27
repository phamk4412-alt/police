FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY webserver/PoliceWebServer.csproj webserver/
RUN dotnet restore webserver/PoliceWebServer.csproj

COPY . .
RUN dotnet publish webserver/PoliceWebServer.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./
COPY index.html ../index.html
COPY app-config.js ../app-config.js
COPY app-api.js ../app-api.js
COPY hcm-boundary.geojson ../hcm-boundary.geojson
COPY admin ../admin
COPY user ../user
COPY police ../police
COPY support ../support

ENV ASPNETCORE_ENVIRONMENT=Production
ENV POLICE_CROSS_SITE_COOKIES=true

ENTRYPOINT ["sh", "-c", "dotnet PoliceWebServer.dll --urls http://0.0.0.0:${PORT:-10000}"]
