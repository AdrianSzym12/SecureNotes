#requires -Version 5.1
[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [string]$MySqlPath,
    [ValidateSet('127.0.0.1')][string]$Server = '127.0.0.1',
    [ValidateRange(1, 65535)][int]$Port = 3306,
    [ValidatePattern('^[A-Za-z0-9_]+$')][string]$AdminUser = 'root'
)

$ErrorActionPreference = 'Stop'
$database = 'securenotes_before_lab'
$account = "'sn_before_lab'@'127.0.0.1'"
# Underscores are wildcard characters in database grants, even in backticks.
$grantScope = '`securenotes\_before\_lab`.*'
$requiredPrivileges = @('ALTER', 'CREATE', 'DELETE', 'INDEX', 'INSERT', 'SELECT', 'UPDATE')

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'Brak dotnet w PATH.' }
$sdks = @(& dotnet --list-sdks)
if ($LASTEXITCODE -ne 0 -or -not ($sdks -match '^8\.')) { throw 'Wymagany .NET 8 SDK.' }
$runtimes = @(& dotnet --list-runtimes)
if ($LASTEXITCODE -ne 0 -or -not ($runtimes -match '^Microsoft.AspNetCore.App 8\.')) {
    throw 'Wymagany ASP.NET Core Runtime 8.'
}
if (-not $MySqlPath) {
    $client = Get-Command mysql.exe, mariadb.exe -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $client) { throw 'Podaj -MySqlPath ze sciezka do mysql.exe lub mariadb.exe.' }
    $MySqlPath = $client.Source
}
$MySqlPath = (Resolve-Path -LiteralPath $MySqlPath).Path
if ([IO.Path]::GetExtension($MySqlPath) -ne '.exe') { throw 'Wskaz natywny plik klienta .exe.' }

function Invoke-AdminSql([string]$Sql) {
    # The client prompts for the ADMIN password at the console. It is never an argument.
    # SQL travels on stdin; native stderr is suppressed to avoid echoing credentials on errors.
    Write-Host 'Klient MariaDB moze poprosic o haslo ADMINISTRATORA (Enter, jezeli puste).'
    $output = @($Sql | & $MySqlPath --no-defaults --protocol=TCP "--host=$Server" "--port=$Port" `
        "--user=$AdminUser" --password --connect-timeout=5 --default-character-set=utf8mb4 `
        --batch --raw --skip-column-names --binary-mode 2>$null)
    if ($LASTEXITCODE -ne 0) {
        throw 'Operacja MariaDB nie powiodla sie. Nie wyswietlono SQL ani diagnostyki mogacej zawierac dane uwierzytelnienia. Sprawdz dostep lokalnie; patrz README.'
    }
    return $output
}

function Assert-ExpectedGrants([string[]]$Grants) {
    $seenDatabaseGrant = $false
    foreach ($raw in $Grants) {
        if ($raw -match 'WITH GRANT OPTION') { throw 'Konto ma GRANT OPTION; przerwano bez zmiany praw.' }
        # SHOW GRANTS can include a password hash or plugin data in its USAGE row.
        $line = $raw -replace '\s+IDENTIFIED\s+.*$', ''
        if ($line -match '^GRANT USAGE ON \*\.\* TO ' -and $line -notmatch 'WITH GRANT OPTION') {
            # USAGE is the account's existence, not a global privilege.
            continue
        }
        if ($line -notmatch '^GRANT (.+) ON (`[^`]+`\.\*) TO ') {
            throw 'Nieoczekiwany grant lub rola. Sprawdz konto w phpMyAdmin; niczego nie zmieniono automatycznie.'
        }
        $privileges = @($Matches[1].Split(',') | ForEach-Object { $_.Trim().ToUpperInvariant() } | Sort-Object)
        $scope = $Matches[2]
        if ($scope -cne $grantScope -or $line -match 'WITH GRANT OPTION' -or
            (Compare-Object $requiredPrivileges $privileges) -or $seenDatabaseGrant) {
            throw 'Zakres konta nie jest dokladnie zgodny z planem (takze escapowanie _). Przerwano bez korekty praw.'
        }
        $seenDatabaseGrant = $true
    }
    if (-not $seenDatabaseGrant) { throw 'Brak pelnego oczekiwanego grantu do jednej bazy. Bez automatycznej naprawy.' }
    Write-Host "PASS: SHOW GRANTS: USAGE oraz SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, INDEX ON $grantScope; bez DROP i GRANT OPTION."
}

Write-Host "Kontrola lokalnej MariaDB: $Server`:$Port; cel: $database; konto: $account"
Write-Host 'WhatIf wykonuje tylko kontrole odczytowe i nadal wymaga uwierzytelnienia administratora.'
$state = @(Invoke-AdminSql @"
SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = '$database';
SELECT COUNT(*) FROM mysql.user WHERE User = 'sn_before_lab' AND Host = '127.0.0.1';
SELECT COUNT(*) FROM mysql.user WHERE User = 'sn_before_lab' AND Host <> '127.0.0.1';
SELECT VERSION();
"@)
if ($state.Count -ne 4) { throw 'Nieoczekiwana odpowiedz kontroli serwera; przerwano.' }
if ($state[3] -notmatch 'MariaDB') { throw 'Ten setup konta jest przeznaczony dla MariaDB; nie zmieniono serwera innego typu.' }
$databaseExists = $state[0] -eq '1'
$accountExists = $state[1] -eq '1'
if ($state[2] -ne '0') { throw 'Istnieje sn_before_lab dla innego hosta. Wymagana reczna weryfikacja; brak zmian.' }

# Validate existing accounts BEFORE any mutation, including CREATE DATABASE.
if ($accountExists) {
    $existingGrants = @(Invoke-AdminSql "SHOW GRANTS FOR $account;")
    Assert-ExpectedGrants $existingGrants
    $existingGrants = $null
}
if ($databaseExists) {
    $schema = @(Invoke-AdminSql "SELECT DEFAULT_CHARACTER_SET_NAME, DEFAULT_COLLATION_NAME FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = '$database';")
    if ($schema.Count -ne 1 -or $schema[0] -cne "utf8mb4`tutf8mb4_general_ci") {
        throw 'Istniejaca baza ma inne parametry; nie zmieniono jej. Sprawdz recznie.'
    }
    Write-Host 'Baza istnieje: zachowujemy wszystkie tabele i dane.'
} elseif ($PSCmdlet.ShouldProcess($database, 'CREATE DATABASE utf8mb4 / utf8mb4_general_ci')) {
    if ((Read-Host "Wpisz UTWORZ BAZE aby utworzyc tylko $database") -cne 'UTWORZ BAZE') { throw 'Anulowano.' }
    $null = Invoke-AdminSql 'CREATE DATABASE `securenotes_before_lab` CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;'
} elseif (-not $WhatIfPreference) {
    throw 'Nie zatwierdzono utworzenia bazy; przerwano przed tworzeniem konta.'
}

if (-not $accountExists -and $PSCmdlet.ShouldProcess($account, "CREATE USER oraz siedem praw tylko ON $grantScope")) {
    Write-Host 'Haslo konta zostanie przeksztalcone w mysql_native_password hash w pamieci i wyslane przez stdin.'
    Write-Host 'Hash jest poufny. Pamiec i logi/audyt serwera nie daja gwarancji niewidocznosci. Nie uzywaj transkrypcji/debuggera.'
    if ((Read-Host 'Wpisz UTWORZ KONTO po sprawdzeniu polityki logowania serwera') -cne 'UTWORZ KONTO') { throw 'Anulowano.' }
    $password = Read-Host 'Nowe haslo sn_before_lab' -AsSecureString
    $confirmation = Read-Host 'Powtorz nowe haslo sn_before_lab' -AsSecureString
    $plain = $null; $repeated = $null; $bytes = $null; $firstHash = $null; $secondHash = $null; $hash = $null
    $sha1 = [Security.Cryptography.SHA1]::Create()
    try {
        $plain = [Net.NetworkCredential]::new('', $password).Password
        $repeated = [Net.NetworkCredential]::new('', $confirmation).Password
        if ([string]::IsNullOrEmpty($plain) -or $plain -cne $repeated) { throw 'Hasla sa puste lub rozne.' }
        $bytes = [Text.Encoding]::UTF8.GetBytes($plain)
        $firstHash = $sha1.ComputeHash($bytes)
        $secondHash = $sha1.ComputeHash($firstHash)
        $hash = '*' + [BitConverter]::ToString($secondHash).Replace('-', '')
        # Hash contains only '*' and hex digits, so no password-controlled SQL is interpolated.
        $null = Invoke-AdminSql "CREATE USER $account IDENTIFIED BY PASSWORD '$hash';"
        # Never reset/alter an account on rerun. A partial failure requires manual review.
        $null = Invoke-AdminSql "GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, INDEX ON $grantScope TO $account;"
    } finally {
        foreach ($buffer in @($bytes, $firstHash, $secondHash)) {
            if ($null -ne $buffer) { [Array]::Clear($buffer, 0, $buffer.Length) }
        }
        $sha1.Dispose(); $password.Dispose(); $confirmation.Dispose()
        $plain = $null; $repeated = $null; $hash = $null
    }
}

if (-not $WhatIfPreference) {
    $grants = @(Invoke-AdminSql "SHOW GRANTS FOR $account;")
    Assert-ExpectedGrants $grants
    $grants = $null
    $result = @(Invoke-AdminSql "SELECT SCHEMA_NAME, DEFAULT_CHARACTER_SET_NAME, DEFAULT_COLLATION_NAME FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = '$database'; SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA = '$database';")
    if ($result.Count -ne 2 -or $result[0] -cne "securenotes_before_lab`tutf8mb4`tutf8mb4_general_ci" -or $result[1] -notmatch '^\d+$') {
        throw 'Koncowa kontrola metadanych nie potwierdzila oczekiwanej bazy. Bez dalszych zmian.'
    }
    Write-Host 'Wynik: nazwa, kodowanie, kolacja oraz liczba tabel (tabele tworzy pozniej EF Core):'
    $result | ForEach-Object { Write-Host $_ }
} else {
    Write-Host 'WhatIf: brak operacji zmieniajacych; konto nie jest tworzone i haslo nowego konta nie jest pobierane.'
}
