#requires -Version 5.1
[CmdletBinding()]
param(
    [string]$MySqlPath,
    [switch]$VerifyDatabase,
    [switch]$ResetStoredPassword
)
$ErrorActionPreference = 'Stop'
if ($env:OS -ne 'Windows_NT') { throw 'Magazyn DPAPI wymaga Windows.' }
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$project = Join-Path $repo 'SecureNotes.API/SecureNotes.API.csproj'
if (-not (Test-Path -LiteralPath $project -PathType Leaf)) { throw 'Nie znaleziono projektu wzgledem skryptu.' }
if (-not (Get-Command git -ErrorAction SilentlyContinue)) { throw 'Brak git w PATH.' }
$branch = & git -C $repo branch --show-current
if ($LASTEXITCODE -ne 0 -or $branch -cne 'demo/security-before') { throw 'Dozwolony tylko branch demo/security-before.' }
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'Brak dotnet w PATH.' }
$sdks = @(& dotnet --list-sdks)
if ($LASTEXITCODE -ne 0 -or -not ($sdks -match '^8\.')) { throw 'Wymagany .NET 8 SDK.' }
$runtimes = @(& dotnet --list-runtimes)
if ($LASTEXITCODE -ne 0 -or -not ($runtimes -match '^Microsoft.AspNetCore.App 8\.')) {
    throw 'Wymagany ASP.NET Core Runtime 8.'
}
$launch = Get-Content -LiteralPath (Join-Path $repo 'SecureNotes.API/Properties/launchSettings.json') -Raw | ConvertFrom-Json
$profile = $launch.profiles.https
if (-not $profile -or $profile.commandName -ne 'Project' -or
    $profile.environmentVariables.ASPNETCORE_ENVIRONMENT -ne 'Development' -or
    $profile.applicationUrl -ne 'https://localhost:7028;http://localhost:5070') {
    throw 'Profil HTTPS nie odpowiada uzgodnionej konfiguracji lokalnej; sprawdz go bez automatycznej zmiany.'
}
foreach ($property in $profile.environmentVariables.PSObject.Properties) {
    if ($property.Name -match '^(Api(__|:)|DOTNET_ENVIRONMENT$)') { throw 'Profil nadpisuje ustawienia laboratorium; wymagana reczna kontrola.' }
}
if ($env:DOTNET_ENVIRONMENT -and $env:DOTNET_ENVIRONMENT -ne 'Development') {
    throw 'DOTNET_ENVIRONMENT koliduje z Development. Uzyj osobnej, poprawnie przygotowanej sesji.'
}
Write-Host 'Cel: 127.0.0.1:3306 | baza securenotes_before_lab | uzytkownik sn_before_lab'
if ($VerifyDatabase) {
    Write-Host 'Opcjonalna kontrola konta, bazy, uprawnien i schematu; dodatkowe monity hasla MariaDB.'
    & (Join-Path $PSScriptRoot 'verify-before.ps1') -MySqlPath $MySqlPath
}
Write-Warning 'Kazdy start w Development wywoluje Database.MigrateAsync(). Pierwszy start tworzy tabele. Nie uruchamiamy dotnet ef ani testow.'
$localAppData = [Environment]::GetFolderPath('LocalApplicationData')
if ([string]::IsNullOrWhiteSpace($localAppData)) { throw 'Nie znaleziono katalogu LocalAppData uzytkownika Windows.' }
$credentialDirectory = [IO.Path]::GetFullPath((Join-Path $localAppData 'SecureNotesLab'))
if ($credentialDirectory.StartsWith($repo.TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -or
    $credentialDirectory.Equals($repo, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Magazyn hasla musi znajdowac sie poza repozytorium.'
}
$storedPasswordPath = Join-Path $credentialDirectory 'before-db-password.dpapi'
$password = $null
try {
    if (-not $ResetStoredPassword -and (Test-Path -LiteralPath $storedPasswordPath)) {
        try {
            $password = ConvertTo-SecureString -String ([IO.File]::ReadAllText($storedPasswordPath).Trim()) -ErrorAction Stop
            if ($password.Length -eq 0) { throw 'Empty credential.' }
        } catch {
            throw 'Nie mozna odczytac lub odszyfrowac lokalnego hasla DPAPI. Uzyj tego samego konta Windows/komputera lub -ResetStoredPassword. Nie zmieniono zapisu.'
        }
    } else {
        Write-Host 'Jednorazowy zapis DPAPI poza repozytorium, dla biezacego konta Windows i komputera.'
        Write-Host 'Podaj istniejace haslo sn_before_lab. Nie zmieniamy hasla w MariaDB.'
        $password = Read-Host 'Haslo sn_before_lab' -AsSecureString
        if ($password.Length -eq 0) { throw 'Haslo nie moze byc puste; dotychczasowy zapis pozostaje bez zmian.' }
        $temporaryPath = $null
        $encryptedPassword = $null
        try {
            # Without -Key, Windows PowerShell uses per-user Windows DPAPI.
            $encryptedPassword = ConvertFrom-SecureString -SecureString $password -ErrorAction Stop
            $null = [IO.Directory]::CreateDirectory($credentialDirectory)
            $temporaryPath = Join-Path $credentialDirectory ([guid]::NewGuid().ToString('N') + '.tmp')
            [IO.File]::WriteAllText($temporaryPath, $encryptedPassword, [Text.Encoding]::ASCII)
            if (Test-Path -LiteralPath $storedPasswordPath) {
                [IO.File]::Replace($temporaryPath, $storedPasswordPath, $null)
            } else {
                [IO.File]::Move($temporaryPath, $storedPasswordPath)
            }
            Write-Host 'Zapisano zaszyfrowane haslo DPAPI w lokalnym magazynie uzytkownika.'
        } catch {
            throw 'Nie zapisano hasla DPAPI. Sprawdz dostep do LocalAppData; aplikacja nie zostala uruchomiona. Nie ujawniono szczegolow sekretu.'
        } finally {
            $encryptedPassword = $null
            if ($temporaryPath -and (Test-Path -LiteralPath $temporaryPath -PathType Leaf)) {
                Remove-Item -LiteralPath $temporaryPath -ErrorAction SilentlyContinue
            }
        }
    }
} catch {
    if ($null -ne $password) { $password.Dispose() }
    throw
}
$names = @('Api__Server', 'Api__Port', 'Api__Database', 'Api__User', 'Api__Password')
$previous = @{}
foreach ($name in $names) { $previous[$name] = [Environment]::GetEnvironmentVariable($name, 'Process') }
$locationChanged = $false
try {
    $env:Api__Server = '127.0.0.1'
    $env:Api__Port = '3306'
    $env:Api__Database = 'securenotes_before_lab'
    $env:Api__User = 'sn_before_lab'
    $env:Api__Password = [Net.NetworkCredential]::new('', $password).Password
    # Existing profile sets Development. No extra application arguments or EF command.
    Push-Location -LiteralPath $repo
    $locationChanged = $true
    & dotnet run --project $project --launch-profile https
    if ($LASTEXITCODE -ne 0) { throw 'Aplikacja zakonczyla sie bledem. Sprawdz lokalna diagnostyke bez publikowania sekretow.' }
} finally {
    if ($locationChanged) { Pop-Location }
    foreach ($name in $names) { [Environment]::SetEnvironmentVariable($name, $previous[$name], 'Process') }
    $previous.Clear()
    $password.Dispose()
}
