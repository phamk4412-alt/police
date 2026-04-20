$env:DOTNET_CLI_HOME = "D:\police\.dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

dotnet run --project "D:\police\webserver\PoliceWebServer.csproj" --urls "http://localhost:5055"
