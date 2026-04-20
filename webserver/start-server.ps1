$env:DOTNET_CLI_HOME = "D:\police\.dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

Write-Host "Server dang chay cho moi thiet bi trong cung mang noi bo..."
Write-Host "Mo tren may chu: http://localhost:5055"
Write-Host "Mo tren may khac: http://<IP-cua-may-chu>:5055"

dotnet run --project "D:\police\webserver\PoliceWebServer.csproj" --urls "http://0.0.0.0:5055"
