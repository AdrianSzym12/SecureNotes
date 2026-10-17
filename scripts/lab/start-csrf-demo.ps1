#requires -Version 5.1
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$project = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot 'csrf-demo/CsrfDemo.csproj'))
if (-not (Test-Path -LiteralPath $project -PathType Leaf)) { throw 'Nie znaleziono projektu csrf-demo wzgledem skryptu.' }
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'Brak dotnet w PATH. Wymagany .NET 8 SDK.' }
$sdks = @(& dotnet --list-sdks)
if ($LASTEXITCODE -ne 0 -or -not ($sdks -match '^8\.')) { throw 'Wymagany .NET 8 SDK.' }
$runtimes = @(& dotnet --list-runtimes)
if ($LASTEXITCODE -ne 0 -or -not ($runtimes -match '^Microsoft.AspNetCore.App 8\.')) {
    throw 'Wymagany ASP.NET Core Runtime 8.'
}

Write-Host 'Serwer demonstracyjny CSRF: https://localhost:7443/ (tylko lokalnie). Zatrzymanie: Ctrl+C.'
Write-Host 'Wymagany istniejacy zaufany certyfikat developerski localhost. Logowanie serwera jest wylaczone.'
$locationChanged = $false
try {
    Push-Location -LiteralPath ([IO.Path]::GetDirectoryName($project))
    $locationChanged = $true
    & dotnet run --project $project --no-launch-profile
    if ($LASTEXITCODE -ne 0) {
        throw 'Serwer CSRF zakonczyl sie bledem. Sprawdz SDK/runtime 8, certyfikat HTTPS i port 7443; nie publikuj danych sesji.'
    }
} finally {
    if ($locationChanged) { Pop-Location }
}
