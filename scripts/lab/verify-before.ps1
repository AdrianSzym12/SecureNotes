#requires -Version 5.1
[CmdletBinding()]
param(
    [string]$MySqlPath,
    [ValidateSet('127.0.0.1')][string]$Server = '127.0.0.1',
    [ValidateRange(1, 65535)][int]$Port = 3306
)
$ErrorActionPreference = 'Stop'
$rows = [Collections.Generic.List[object]]::new()
function Add-Result([string]$Item, [string]$Status, [string]$Detail) {
    $rows.Add([pscustomobject]@{ Element = $Item; Status = $Status; Opis = $Detail })
}
try {
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'Brak dotnet w PATH.' }
    $sdks = @(& dotnet --list-sdks)
    if ($LASTEXITCODE -ne 0 -or -not ($sdks -match '^8\.')) { throw 'Brak .NET 8 SDK.' }
    $runtimes = @(& dotnet --list-runtimes)
    if ($LASTEXITCODE -ne 0 -or -not ($runtimes -match '^Microsoft.AspNetCore.App 8\.')) { throw 'Brak ASP.NET Core Runtime 8.' }
    Add-Result '.NET 8' 'PASS' 'SDK i ASP.NET Core Runtime 8 sa dostepne.'
    if (-not $MySqlPath) {
        $client = Get-Command mysql.exe, mariadb.exe -ErrorAction SilentlyContinue | Select-Object -First 1
        if (-not $client) { throw 'Podaj -MySqlPath do mysql.exe lub mariadb.exe.' }
        $MySqlPath = $client.Source
    }
    $MySqlPath = (Resolve-Path -LiteralPath $MySqlPath).Path
    if ([IO.Path]::GetExtension($MySqlPath) -ne '.exe') { throw 'Wskaz natywny klient .exe.' }
    function Read-LabSql([string]$Sql) {
        Write-Host 'Klient MariaDB poprosi o haslo sn_before_lab; podaj je interaktywnie.'
        $output = @($Sql | & $MySqlPath --no-defaults --protocol=TCP "--host=$Server" "--port=$Port" `
            --user=sn_before_lab --password --database=securenotes_before_lab --connect-timeout=5 `
            --batch --raw --skip-column-names --binary-mode 2>$null)
        if ($LASTEXITCODE -ne 0) { throw 'Odczyt MariaDB nieudany. Sprawdz haslo, serwer, baze i prawa; nie wypisano poufnej diagnostyki.' }
        return $output
    }
    $info = @(Read-LabSql @'
SELECT CURRENT_USER(), DATABASE(), VERSION();
SELECT DEFAULT_CHARACTER_SET_NAME, DEFAULT_COLLATION_NAME FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = 'securenotes_before_lab';
'@)
    if ($info.Count -ne 2) { throw 'Nieoczekiwany format metadanych.' }
    $identity = $info[0].Split("`t")
    if ($identity.Count -ne 3 -or $identity[0] -cne 'sn_before_lab@127.0.0.1' -or $identity[1] -cne 'securenotes_before_lab') {
        throw 'CURRENT_USER lub DATABASE nie jest zgodne z celem BEFORE.'
    }
    Add-Result 'TCP / konto / baza' 'PASS' "$($identity[0]); $($identity[1]); serwer $($identity[2])"
    if ($info[1] -cne "utf8mb4`tutf8mb4_general_ci") { throw 'Nieoczekiwane kodowanie lub kolacja bazy.' }
    Add-Result 'Kodowanie / kolacja' 'PASS' 'utf8mb4 / utf8mb4_general_ci'

    $grants = @(Read-LabSql 'SHOW GRANTS;')
    $expected = @('ALTER', 'CREATE', 'DELETE', 'INDEX', 'INSERT', 'SELECT', 'UPDATE')
    $seen = $false
    foreach ($raw in $grants) {
        if ($raw -match 'WITH GRANT OPTION') { throw 'Nieoczekiwane GRANT OPTION.' }
        $line = $raw -replace '\s+IDENTIFIED\s+.*$', ''
        if ($line -match '^GRANT USAGE ON \*\.\* TO ' -and $line -notmatch 'WITH GRANT OPTION') { continue }
        if ($line -notmatch '^GRANT (.+) ON (`[^`]+`\.\*) TO ') { throw 'Nieoczekiwany grant/rola. Nie wyswietlono surowych danych uwierzytelnienia.' }
        $privileges = @($Matches[1].Split(',') | ForEach-Object { $_.Trim().ToUpperInvariant() } | Sort-Object)
        if ($Matches[2] -cne '`securenotes\_before\_lab`.*' -or $line -match 'WITH GRANT OPTION' -or
            (Compare-Object $expected $privileges) -or $seen) { throw 'Nieoczekiwane uprawnienia lub nieescapowany zakres bazy.' }
        $seen = $true
    }
    $grants = $null
    if (-not $seen) { throw 'Brak wymaganych siedmiu uprawnien do jednej bazy.' }
    Add-Result 'SHOW GRANTS' 'PASS' 'Siedem praw tylko do dokladnej bazy; bez DROP, GRANT OPTION i praw globalnych (USAGE nie nadaje praw).'

    $tables = @(Read-LabSql "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA = 'securenotes_before_lab' ORDER BY TABLE_NAME;")
    if ($tables.Count -eq 0) {
        Add-Result 'Tabele / migracje' 'NOT READY' 'Pusta baza jest poprawnym stanem przed pierwszym startem; migracje wykona aplikacja.'
    } else {
        Add-Result 'Tabele' 'PASS' ($tables -join ', ')
        $lowerTables = @($tables | ForEach-Object { $_.ToLowerInvariant() })
        if ($lowerTables -contains '__efmigrationshistory') {
            # Use the actual metadata name to handle server casing; never accept an arbitrary identifier.
            $historyTable = @($tables | Where-Object { $_ -ieq '__efmigrationshistory' })[0]
            if ($historyTable -cnotmatch '^[A-Za-z_]+$') { throw 'Niepoprawny identyfikator historii.' }
            $history = @(Read-LabSql ('SELECT MigrationId, ProductVersion FROM `' + $historyTable + '` ORDER BY MigrationId;'))
            $migrationIds = @($history | ForEach-Object { ($_ -split "`t")[0] })
            if ($migrationIds -contains '20261008210341_InitialCreate' -and $lowerTables -contains 'users' -and $lowerTables -contains 'notes') {
                Add-Result 'Migracje' 'PASS' ($history -join '; ')
            } else {
                Add-Result 'Migracje' 'NOT READY' 'Historia lub tabele nie potwierdzaja kompletnej migracji InitialCreate.'
                throw 'Czesciowy/nieoczekiwany schemat wymaga recznej diagnozy, bez resetu.'
            }
        } else {
            Add-Result 'Migracje' 'NOT READY' 'Tabele istnieja bez historii EF Core; nie uruchamiaj migracji w ciemno.'
            throw 'Istniejace tabele bez historii wymagaja recznej diagnozy.'
        }
    }
    $rows | Format-Table -Wrap -AutoSize
} catch {
    Add-Result 'Weryfikacja' 'FAIL' $_.Exception.Message
    $rows | Format-Table -Wrap -AutoSize
    throw 'Weryfikacja BEFORE zakonczona bledem; nie uruchamiaj aplikacji przed wyjasnieniem.'
}
