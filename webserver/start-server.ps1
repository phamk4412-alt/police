$env:DOTNET_CLI_HOME = "D:\police-main\.dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:NUGET_PACKAGES = "D:\police-main\.nuget\packages"

Write-Host "Server Police Smart Hub dang khoi dong..."
Write-Host "API local: http://localhost:5055"
Write-Host "API cho thiet bi khac: http://<IP-hoac-domain-cua-server>:5055"
Write-Host "SignalR hub: /hubs/incidents"
Write-Host "Cau hinh DB bang webserver/appsettings.json hoac bien moi truong POLICE_DATABASE_PROVIDER, POLICE_SQLSERVER_CONNECTION, POLICE_POSTGRES_CONNECTION"

dotnet run --project "D:\police-main\webserver\PoliceWebServer.csproj" --urls "http://0.0.0.0:5055"
