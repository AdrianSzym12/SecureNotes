# Odtworzenie laboratorium SecureNotes BEFORE/AFTER

Instrukcja dla prowadzącego zajęcia Bezpieczeństwo Aplikacji Internetowych, etap 07A.2b, 17.10.2026. Polecenia poniżej są przeznaczone do późniejszego, świadomego wykonania przez operatora. Podczas pisania instrukcji Codex nie uruchamiał aplikacji, HTTP, SQL ani migracji i nie odczytywał lokalnych sekretów.

Użytkownik potwierdził na swoim komputerze pierwszy start BEFORE z monitem hasła, kolejny bez monitu, HTTPS 7028, dostęp do bazy i aktualne migracje, start demo skryptem na HTTPS 7443 oraz dostępność obu stron. Jest to potwierdzenie użytkownika, nie test wykonany przez Codexa ani dowód odtworzenia na drugim komputerze. Nie zgłoszono osobnej próby ResetStoredPassword, uszkodzenia DPAPI ani nowego pełnego porównania AFTER. Formalne wyniki pozostają **6 PARTIAL — BEFORE / 9 NOT RUN** w [results.md](results.md).

## A. Cel, architektura i wymagania

AFTER (`develop`) to istniejąca zabezpieczona referencja. BEFORE (`demo/security-before`) jest rekonstrukcją z czterema celowymi osłabieniami: innerHTML treści notatki, wyjątek script-src-attr na dokładnym `/notes.html`, brak Antiforgery dla POST JSON oraz adapter formularza URL-encoded bez tokenu. BEFORE pozostaje podatny bez przełączników. Uruchamiać wyłącznie na własnym komputerze, lokalnie, z fikcyjnymi kontami i danymi; nie wystawiać przez tunele ani hosting.

Warstwy: statyczny HTML/JavaScript → ASP.NET Core API → Application/Domain → EF Core/Pomelo → MariaDB. JWT jest przechowywany w SecureNotes.Auth, z HttpOnly, Secure, SameSite=Strict. Właściciel notatki pochodzi z claims. Więcej: [architektura bazowa](architecture.md) i [rejestr zmian BEFORE](change-register.md).

| Element | Wymaganie / kontrola przed konfiguracją |
|---|---|
| Windows i PowerShell | Windows PowerShell 5.1; DPAPI wymaga konta Windows i tego komputera. Sprawdź `$PSVersionTable.PSVersion` |
| Git | Klient w PATH: `git --version`; skrypt BEFORE wymaga aktywnej gałęzi |
| .NET | Zainstalowany .NET 8 SDK oraz Microsoft.AspNetCore.App 8.x: `dotnet --list-sdks`, `dotnet --list-runtimes`. Sam Runtime nie wystarcza do budowy |
| MariaDB | Lokalny serwer; kod deklaruje MariaDBServerVersion 10.4.32. Wersja 10.4.32 była używana w laboratorium; inna wersja wymaga własnej weryfikacji zgodności |
| Klient bazy | Natywny mysql.exe lub mariadb.exe z interaktywnym monitem; phpMyAdmin może służyć ręcznej administracji |
| Przeglądarka | Aktualny Edge/Chrome/Firefox z DevTools i odrębnymi profilami BEFORE/AFTER |
| HTTPS | Lokalny zaufany certyfikat developerski ASP.NET Core dla localhost |

XAMPP jest jednym ze sposobów uruchomienia MariaDB, nie wymaganiem ścieżki instalacji. Visual Studio 2022 jest opcjonalne: dwa launchery działają z PowerShell. Przy pierwszym dotnet run może być potrzebne przywrócenie istniejących pakietów NuGet: m.in. EF Core 8.0.13, Pomelo 8.0.3, JwtBearer 8.0.22, Swashbuckle 6.6.2. Nie potrzeba globalnego dotnet-ef ani nowych bibliotek. Serwer CSRF używa wyłącznie frameworka ASP.NET Core 8.

| Proces | Adres/port | Dane i zastosowanie |
|---|---|---|
| MariaDB | TCP 127.0.0.1:3306 | BEFORE: securenotes_before_lab, konto sn_before_lab@127.0.0.1 |
| SecureNotes BEFORE albo AFTER | HTTPS localhost:7028 | Nigdy równocześnie na tym porcie; AFTER używa osobnej securenotes_db |
| Profil https SecureNotes | Dodatkowo HTTP localhost:5070 | Musi być wolny; do logowania i demonstracji używać HTTPS, nie tego HTTP |
| CSRF Demo | HTTPS localhost:7443 | Osobny lokalny proces; tylko na czas demonstracji formularza |
| Testy automatyczne | securenotes_test_db | Poza uruchamianiem laboratorium; nie uruchamiać pełnego zestawu integracyjnego |

MariaDB ma nasłuchiwać na lokalnym 3306. Dla 7028/5070/7443 przed startem nie może być obcego procesu. Odczytowa kontrola Windows:

```powershell
Get-NetTCPConnection -State Listen -ErrorAction SilentlyContinue |
    Where-Object { $_.LocalPort -in 3306,7028,5070,7443 } |
    Select-Object LocalAddress,LocalPort,OwningProcess
```

Nie zatrzymywać automatycznie cudzych procesów. W Git znajdują się kod, migracje, profile i instrukcje; baz, kont MariaDB, poświadczeń DPAPI, User Secrets, certyfikatów, fikcyjnych kont SecureNotes i wcześniejszych notatek nie pobiera się przez clone. Archiwizacja screenshotów z rozmowy pozostaje niepotwierdzona.

## B. Pobranie kodu z GitHub

Wybierz dowolny własny katalog roboczy i uruchom z niego:

```powershell
git clone --branch demo/security-before --single-branch https://github.com/AdrianSzym12/SecureNotes.git
Set-Location .\SecureNotes
git branch --show-current
git rev-parse HEAD
git status --short
Test-Path .\scripts\lab\start-before.ps1
Test-Path .\scripts\lab\start-csrf-demo.ps1
Test-Path .\docs\before-after\LAB-SETUP.md
```

Dalsze polecenia BEFORE wykonuj z tego katalogu, chyba że wyraźnie wskazano inaczej. Skrypty same ustalają ścieżki względem własnego pliku, ale podana względna ścieżka wywołania zakłada katalog główny klonu. Preferuj Git względem ZIP: start-before sprawdza `demo/security-before` i odrzuca brak gałęzi/detached HEAD.

**Warunek publikacji:** w chwili przygotowania 07A.2b ostatni zatwierdzony BEFORE to `e801e8352b5bcdbffdab1ec1299bdfb5dfa21659`, a launchery i niniejsza instrukcja są dopiero lokalnymi zmianami. Nie potwierdzono ich publikacji na GitHub. Autor musi wcześniej opublikować właściwe commity, a odbiorca sprawdzić ich SHA i obecność plików. Nie zastępować brakujących skryptów uruchomieniem aplikacji z domyślną bazą. Ten etap nie wykonuje synchronizacji ani push.

AFTER pobierz do innego, nieistniejącego katalogu. Z katalogu nadrzędnego obu klonów:

```powershell
git clone --branch develop --single-branch https://github.com/AdrianSzym12/SecureNotes.git SecureNotes-AFTER
git -C .\SecureNotes-AFTER branch --show-current
git -C .\SecureNotes-AFTER rev-parse HEAD
git -C .\SecureNotes-AFTER status --short
```

Referencja projektu AFTER ma SHA `21be7bb6aadd69c729250eb5d5248cabb114de04`. Jeśli zdalny develop wskazuje inny kod, zatrzymaj porównanie i uzgodnij wersję; nie resetuj istniejących katalogów ani nie zmieniaj kodu dla dopasowania demonstracji. Nazwy katalogów klonów nie określają gałęzi. Na nowym komputerze nie jest potrzebna rekonstrukcja istniejących worktree autora.

## C. MariaDB: baza i konto BEFORE

1. Uruchom lokalną MariaDB przez panel XAMPP albo właściwą usługę Windows. Ustal port 3306 i istniejące konto administratora MariaDB, uprawnione do kontroli mysql.user, tworzenia bazy/konta i nadania praw do tej jednej bazy. To konto bazy, nie wymóg uruchamiania PowerShell jako administrator Windows. Nie zakładaj pustego hasła root.
2. Jeśli klient jest w PATH, sprawdź `Get-Command mysql.exe,mariadb.exe -ErrorAction SilentlyContinue`. W przeciwnym razie przygotuj jego rzeczywistą ścieżkę (to jedyna wartość do zastąpienia w przykładzie):

```powershell
$mysqlClient = 'C:\MariaDB\bin\mysql.exe'
Test-Path -LiteralPath $mysqlClient
& $mysqlClient --no-defaults --protocol=TCP --host=127.0.0.1 --port=3306 --user=root --password
```

Wpisz hasło administratora dopiero w monicie klienta. Jeśli używasz innego administratora, zastąp niepoufną nazwę root. Pomyślne wejście do klienta potwierdza TCP i uwierzytelnienie; wyjdź poleceniem `exit`, bez wykonywania SQL. Nie używaj `--password=...`, transkrypcji ani debug trace.

3. Z katalogu BEFORE przejrzyj odczytowy plan setupu:

```powershell
.\scripts\lab\setup-before.ps1 -MySqlPath $mysqlClient -WhatIf
```

WhatIf rzeczywiście łączy się z bazą i pyta o hasło **administratora**. Odczytuje stan serwera/konta/bazy; nie tworzy bazy/konta i nie pyta o nowe hasło aplikacji. Jeśli klient jest w PATH, można pominąć cały parametr MySqlPath. Inny administrator: rzeczywisty parametr `-AdminUser`, np. `-AdminUser lab_admin` zamiast root (konto musi już istnieć). Nie zmieniaj portu przygotowania względem 3306 wymuszanego przez launcher.

4. Po sprawdzeniu celu uruchom setup bez WhatIf:

```powershell
.\scripts\lab\setup-before.ps1 -MySqlPath $mysqlClient
```

ShouldProcess i wysoki ConfirmImpact mogą wymagać potwierdzeń PowerShell. Przy nowej bazie wpisuje się dodatkowo `UTWORZ BAZE`, przy nowym koncie `UTWORZ KONTO`. Skrypt tworzy wyłącznie securenotes_before_lab z utf8mb4/utf8mb4_general_ci i sn_before_lab@127.0.0.1; nowego hasła tego konta wymaga dwukrotnie, przez maskowane monity. Zachowaj je w swoim menedżerze haseł do pierwszego startu. Nie jest to hasło administratora ani przyszłego konta użytkownika SecureNotes. Kolejne wywołania klienta SQL mogą ponownie pytać o hasło administratora.

Zakres konta: SELECT, INSERT, UPDATE, DELETE do pracy aplikacji oraz CREATE, ALTER, INDEX do obecnej migracji. Dokładny grant dotyczy `` `securenotes\_before\_lab`.* `` z escapowaniem podkreśleń; bez DROP, GRANT OPTION, ról i praw do innych baz. Globalny USAGE oznacza istnienie konta, nie uprawnienia administracyjne. SQL setupu przekazywany jest przez stdin; hasło nowego konta jest w pamięci przekształcane do mysql_native_password hash. Hash także jest poufny i może podlegać audytowi serwera — nie publikuj surowego SHOW GRANTS ani logów.

Istniejącej bazy setup nie czyści. Niezgodna kolacja/granty, inne hosty tego samego konta lub częściowa awaria wymagają ręcznej diagnozy. Setup nie resetuje hasła ani nie naprawia uprawnień; CREATE USER i GRANT nie są jedną transakcją. Jeśli klient nie obsługuje interaktywnego promptu przy SQL na stdin, użyj zatwierdzonej ręcznej administracji w phpMyAdmin: identyczna baza, host konta 127.0.0.1, siedem praw do dokładnego escapowanego zakresu, bez globalnych praw. Hasło wpisz wyłącznie lokalnie. Nie obchodź uwierzytelniania.

5. Odczytowa kontrola konta aplikacji:

```powershell
.\scripts\lab\verify-before.ps1 -MySqlPath $mysqlClient
```

Tutaj wpisuje się hasło **sn_before_lab**, potencjalnie kilka razy w natywnych monitach. Verify nie korzysta z zapisu DPAPI launchera. Sprawdza rzeczywiste CURRENT_USER/DATABASE, granty, kodowanie i tabele/historię. Przed pierwszym startem brak tabel daje oczekiwane NOT READY, nie błąd połączenia. FAIL konta/grantów lub częściowy istniejący schemat wymaga wyjaśnienia przed startem. Nie uruchamiaj `database/setup.sql` w ciemno ani nie twórz tabel ręcznie.

## D. JWT, User Secrets i HTTPS

### D1. Wymagane ustawienia aplikacji

`PersistenceService.AddPersistence` odczytuje sekcję Api: Server, Port, Database, User, Password. Server/Database/User/Password muszą być niepuste, Port w zakresie 1–65535. Nie jest to ConnectionStrings:DefaultConnection. Start-before ustawia procesowo dokładnie 127.0.0.1, 3306, securenotes_before_lab, sn_before_lab i odszyfrowane hasło; zmienne procesu mają pierwszeństwo nad User Secrets w standardowym WebApplication.CreateBuilder. Nie zapisuj tych haseł w appsettings ani launchSettings.

`AddJwtAuthentication` wymaga sekcji Jwt:

| Klucz | Wymaganie |
|---|---|
| Jwt:Issuer | Niepusty identyfikator wystawcy, spójny dla wystawiania i walidacji |
| Jwt:Audience | Niepusty identyfikator odbiorcy, spójny dla wystawiania i walidacji |
| Jwt:ExpirationMinutes | Liczba większa od zera, np. 30 |
| Jwt:SecretKey | Niepusty klucz, minimum 32 bajty po kodowaniu UTF-8; użyj losowego klucza, nie krótkiego hasła |

API ma UserSecretsId `833ceb9d-d91d-4691-a91c-d5c1ac2a86d3`. Profil https ustawia Development, a standardowy builder ładuje User Secrets dla tego ID. BEFORE i AFTER mają **ten sam ID**, więc na tym samym koncie Windows współdzielą magazyn, także w osobnych klonach. Nie zmieniaj ID w projekcie; nie zapisuj tam bazowego hasła BEFORE. JWT można skonfigurować raz dla świeżego lokalnego laboratorium, a sesje obu aplikacji nadal trzeba rozdzielać. Na używanym już stanowisku nie nadpisuj istniejących sekretów bez świadomej decyzji — zmiana klucza unieważnia stare podpisy/sesje.

### D2. Jednorazowe przygotowanie JWT na nowym komputerze

Poniższy fragment do ręcznego wykonania w PowerShell 5.1 z katalogu BEFORE tworzy nowy losowy klucz w pamięci i podaje JSON na stdin narzędzia User Secrets. Nie drukuje klucza ani nie umieszcza jego wartości w argumentach lub historii. Nie uruchamiaj z transkrypcją, debugowaniem ani przekierowaniem do pliku. Dotyczy nowego magazynu; na istniejącym najpierw ustal konfigurację z operatorem.

```powershell
$rng = [Security.Cryptography.RandomNumberGenerator]::Create()
$keyBytes = New-Object byte[] 32
$jwtSettings = $null
$jwtJson = $null
try {
    $rng.GetBytes($keyBytes)
    $jwtSettings = @{ Jwt = @{
        Issuer = 'SecureNotesLab'
        Audience = 'SecureNotesLabBrowser'
        ExpirationMinutes = 30
        SecretKey = [Convert]::ToBase64String($keyBytes)
    } }
    $jwtJson = $jwtSettings | ConvertTo-Json -Depth 3 -Compress
    $jwtJson | dotnet user-secrets set --project .\SecureNotes.API\SecureNotes.API.csproj
    if ($LASTEXITCODE -ne 0) { throw 'Nie zapisano lokalnej konfiguracji JWT.' }
} finally {
    [Array]::Clear($keyBytes, 0, $keyBytes.Length)
    $rng.Dispose()
    $jwtSettings = $null
    $jwtJson = $null
}
```

Klucz bazuje na 32 losowych bajtach, a jego reprezentacja Base64 spełnia kontrolę długości UTF-8 w kodzie. Narzędzie obsługuje JSON na stdin; csproj już ma UserSecretsId, więc nie wymaga user-secrets init. Nie używaj `dotnet user-secrets list` do udostępniania diagnostyki — wypisuje wartości. Sukces narzędzia i kod wyjścia potwierdzają zapis, a późniejszy start waliduje obecność, długość i parametry bez potrzeby wypisywania klucza. Błąd JWT diagnozuj po kategorii komunikatu aplikacji. Nie włączaj ujawniania danych tokenów.

User Secrets to plik lokalny poza repozytorium, na Windows pod `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`. **Nie jest szyfrowany jak DPAPI**; dostęp chroni konto/system plików. Nie kopiuj go do Git ani screenshotów. Klucz chwilowo istnieje w pamięci; czyszczenie referencji nie gwarantuje wymazania wszystkich kopii stringów zarządzanych. Konfiguracja autora nie jest pobierana z GitHuba.

### D3. Certyfikat localhost

```powershell
dotnet dev-certs https --check --trust
```

To kontrola istniejącego ważnego i zaufanego certyfikatu. Jeśli go brak, na nowym własnym komputerze operator świadomie wykonuje:

```powershell
dotnet dev-certs https --trust
dotnet dev-certs https --check --trust
```

Pierwsze polecenie może utworzyć certyfikat i wymagać zaakceptowania systemowego monitu zaufania. Nie robią tego launchery. W środowisku zarządzanym uzgodnij zaufanie z administratorem; nie ignoruj błędów przeglądarki ani nie wyłączaj Secure. Nie eksportuj certyfikatu z kluczem do repozytorium, nie wykonuj rutynowo dev-certs --clean. Sprawdź zaufanie konkretnej przeglądarki. Certyfikat localhost nie wystarcza do osobnej próby HTTPS po IP.

## E. Pierwszy i kolejne starty BEFORE

Z katalogu BEFORE, w zwykłej interaktywnej konsoli Windows PowerShell:

```powershell
.\scripts\lab\start-before.ps1
```

Skrypt sprawdza projekt, dokładną gałąź, obecność SDK/ASP.NET Core 8 i profil https z Development oraz brakiem sprzecznych nadpisań. Wyświetla cel bazy i ostrzeżenie MigrateAsync. **Nie wymaga START BEFORE.**

Przy pierwszym starcie pyta raz o istniejące hasło sn_before_lab i zapisuje DPAPI w `%LOCALAPPDATA%\SecureNotesLab\before-db-password.dpapi`. ConvertFrom-SecureString bez Key chroni zapis dla tego konta Windows/komputera; kolejne starty automatycznie go odszyfrowują. Nie kopiuj zapisu na inny komputer; tam pierwsze podanie jest potrzebne ponownie. Przy błędnym/uszkodzonym/niedostępnym zapisie skrypt przerywa bez automatycznego nadpisania. Lokalną aktualizację wykonuje się tak:

```powershell
.\scripts\lab\start-before.ps1 -ResetStoredPassword
```

Podaj poprawne istniejące hasło. Nie jest to reset hasła w MariaDB ani ponowienie setupu. Puste hasło nie zastępuje poprzedniego zapisu; zapis jest zastępowany przez plik tymczasowy i operację Replace. Bez sprawdzenia SQL skrypt nie rozpoznaje literówki hasła: nieudany start wymaga poprawienia lokalnego zapisu tą opcją. DPAPI chroni dane w spoczynku, nie eliminuje jawnego hasła w pamięci i procesowym Api__Password podczas działania dotnet. Nie wypisuj środowiska, nie uruchamiaj AFTER z tego kontekstu. Finally przywraca pięć poprzednich Api__* i katalog roboczy; przy wymuszonym zabiciu PowerShell nie ma gwarancji finally, więc używaj osobnej konsoli i zamykaj ją po pracy.

Opcjonalna diagnostyka, nie standardowy restart:

```powershell
.\scripts\lab\start-before.ps1 -VerifyDatabase -MySqlPath $mysqlClient
```

Verify uruchamia się przed startem i ma własne, dodatkowe monity hasła MariaDB; DPAPI ich nie zastępuje. Można też uruchomić verify samodzielnie. Nie ma automatycznych napraw bazy. Jeśli polityka Windows blokuje skrypt, sprawdź Get-ExecutionPolicy -List i podpis/pochodzenie pliku zgodnie z zasadami komputera; nie wyłączaj zabezpieczeń globalnie ani nie używaj Unrestricted jako domyślnej instrukcji.

W Development istniejący Program.cs wywołuje Database.MigrateAsync. Pierwszy start prawidłowo pustej bazy wykona `20261008210341_InitialCreate`, utworzy Users, Notes i historię EF; na Windows nazwy mogą być małymi literami. Kolejne starty przy aktualnej historii nie powinny dodawać migracji. Nie uruchamiaj osobno dotnet ef, rollbacku ani ręcznych CREATE TABLE. Obecne prawa nie obejmują DROP. Przy częściowej migracji zatrzymaj się i zachowaj diagnostykę bez sekretów, bez usuwania bazy. Przywrócenie NuGet/budowa może tworzyć bin/obj w BEFORE; nie są materiałem do publikacji.

Otwórz **https://localhost:7028/**, zarejestruj nowe fikcyjne konto aplikacji, np. sn-a@example.test z własnym hasłem spełniającym walidację formularza, zaloguj się i utwórz zwykłą notatkę. To trzecie odrębne hasło, niezwiązane z administratorem MariaDB i sn_before_lab. Zamknij i uruchom aplikację ponownie, aby sprawdzić brak monitu DPAPI oraz trwałość własnej notatki. Po pierwszej migracji verify powinien potwierdzić Users/Notes i InitialCreate; jego kontrola nie jest pełnym audytem wszystkich kolumn. Pełne testy integracyjne nie są krokiem przygotowania.

## F. Uruchomienie lokalnej demonstracji CSRF

Pozostaw BEFORE uruchomiony i konto A zalogowane na 7028. W **drugim, niezależnym terminalu** przejdź do tego samego klonu BEFORE:

```powershell
.\scripts\lab\start-csrf-demo.ps1
```

Skrypt ustala ścieżkę względem siebie, sprawdza projekt i .NET 8, wykonuje dotnet run z no-launch-profile; nie uruchamia BEFORE. Otwórz **https://localhost:7443/** jako samodzielną kartę w tym samym profilu przeglądarki. Kestrel słucha wyłącznie loopback i używa istniejącego certyfikatu. Logowanie jest wyłączone, więc brak „Now listening” jest spodziewany; Ctrl+C zatrzymuje ten proces. Nie ma potrzeby zmiany Apache/XAMPP.

Podaj unikalny fikcyjny tytuł i zwykłą treść, kliknij „Wyślij formularz CSRF”. Natywny POST wysyła title/content jako application/x-www-form-urlencoded bez tokenu na https://localhost:7028/api/notes, do ukrytego iframe. Nie ma JavaScript, fetch ani automatycznej wysyłki. Główna karta pozostaje na stronie demonstracyjnej; ona nie odczytuje obcej odpowiedzi i nie deklaruje sukcesu. **Potwierdź nową notatkę w UI BEFORE po odświeżeniu**, najlepiej także po ID przez odczyt API. Ewentualna blokada wyświetlenia odpowiedzi przez frame-ancestors none/X-Frame-Options DENY nie oznacza automatycznie odrzucenia POST. Nie usuwaj tych zabezpieczeń z SecureNotes.

To **cross-origin / same-site**: HTTPS localhost z różnymi portami. Nie jest to obejście SameSite=Strict ani dowód ataku z zewnętrznej domeny. Cookies nie izolują portów, więc mogą dotrzeć też do demo 7443; kod ich nie czyta ani nie loguje. Serwer musi pozostać całkowicie lokalny i kontrolowany. Szczegóły: [README demo](../../scripts/lab/csrf-demo/README.md). Osobne B03 po IP i T-CORS-01 nie działają bezpośrednio na tym hoście (odmowa Host IP i blokujące connect-src); warunki opisuje [plan](test-plan.md). Nie traktuj takiej odmowy jako wyniku SameSite/CORS.

## G. Odtworzenie Stored XSS na własnej notatce

Użyj oddzielnej fikcyjnej notatki, tytułu z numerem próby i własnego konta. Najpierw można zapisać `<strong>DEMO-XSS-HTML</strong>`: pogrubienie potwierdza interpretację HTML, nie wykonanie JavaScript. Do powtarzalnej próby według T-XSS-02/03 użyj treści z istniejącego planu:

```html
<img src="/__sn_xss_probe_missing__.png" onerror="document.documentElement.dataset.snXss='executed';alert('SN-XSS-01')">
```

W BEFORE treść jest przypisana przez innerHTML, a CSP dopuszcza atrybuty zdarzeń tylko dla dokładnego /notes.html. Po odczycie notatki img powinno wywołać lokalny błąd obrazu (adres musi rzeczywiście nie istnieć), handler powinien ustawić data-sn-xss i pokazać alert. Potwierdź zapisane ID/content w nowym GET; zamknij alert i przeładuj cały dokument, bez ponownego wpisywania payloadu. Zarejestruj nowy GET → DOM → efekt. Nie ustawiaj znacznika ręcznie i nie używaj ręcznego JS w konsoli jako dowodu XSS.

Jest to procedura do odtworzenia, nie dopisany wynik tej instrukcji. Historyczny humorystyczny alert użytkownika zapisany w results.md pozostaje bez zmian i nie otrzymuje automatycznie znacznika tego payloadu. Używaj jednego własnego payloadu naraz, zamykaj alerty i po zachowaniu dowodów usuwaj przez UI tylko swoje ID próby. Nie czytaj tokenów ani nie używaj zewnętrznych adresów. W AFTER oczekiwany jest tekst payloadu zamiast img/handlera, ale porównanie nadal wymaga rzeczywistego wykonania. Pośrednią rolę samej CSP pokazuje zachowany checkpoint T-XSS-04, nie cofanie docelowego BEFORE.

## H. AFTER: osobna konfiguracja na nowym komputerze

**Ograniczenie:** działające AFTER i 85/85 testów automatycznych potwierdził użytkownik na istniejącym stanowisku. Nie dostarczono zweryfikowanego uruchomienia od zera na drugim komputerze. Poniższy wariant konfiguracji wynika z faktycznego kodu develop, lecz pozostaje **procedurą do weryfikacji**, nie wynikiem testu przenośności. Nie uruchamiać testów automatycznych w ramach tej instrukcji.

Sprawdzono: develop również wymaga net8.0 i tych samych zależności EF/Pomelo/JWT, ma ten sam UserSecretsId, korzysta z ApiConfiguration i MigrateAsync w Development, a profil https zajmuje 7028 oraz 5070. Nie ma laboratoryjnych launcherów BEFORE na bazowym develop; nie kopiuj do niego start-before i nie uruchamiaj z jego konfiguracją. Frontend pozostawia textContent i oryginalną CSP oraz Antiforgery POST.

1. Zatrzymaj BEFORE i demo przez Ctrl+C, wyloguj profile, zamknij ich karty. Upewnij się, że proces start-before zakończył finally, a 7028/5070 są wolne. Zamknij konsolę z Api__* BEFORE. Przejdź do klonu SecureNotes-AFTER w nowej konsoli; sprawdź develop, SHA i czysty status.
2. Na **nowym** lokalnym serwerze operator przygotowuje osobną bazę securenotes_db, utf8mb4/utf8mb4_general_ci, oraz osobne konto tylko do niej. Przykładowa proponowana nazwa nowego konta to sn_after_lab@127.0.0.1 — nie jest to konto tworzone przez repozytorium. Utwórz je ręcznie w phpMyAdmin, hasło wprowadzając wyłącznie w UI. Dla obecnego InitialCreate przyznaj SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, INDEX tylko do dokładnego zakresu `` `securenotes\_db`.* ``, bez globalnych praw, DROP i GRANT OPTION. Sprawdź efektywny zakres konta oraz brak dostępu do bazy BEFORE. Jeżeli taka baza lub konto już istnieją, nie nadpisuj ich: najpierw uzgodnij z administratorem istniejący stan/dane i konto.
3. Setup-before i verify-before mają sztywny cel BEFORE i **nie są narzędziami konfiguracji AFTER**. Nie używaj ich do tej bazy, nie twórz securenotes_after_lab ani nowej bazy testów automatycznych. Wspólną konfigurację Jwt przygotowaną w sekcji D można wykorzystać w Development; nie zakładaj natomiast gotowej konfiguracji Api lub hasła AFTER.
4. W klonie AFTER, dopiero po przygotowaniu bazy i uprawnień, poniższy fragment ustawia procesowo rzeczywiste klucze Api i uruchamia istniejący profil. Wpisz hasło **wybranego konta AFTER**, nie sn_before_lab; dostosuj wyłącznie niepoufną nazwę konta, jeśli operator wybrał inną. Nie zapisywać fragmentu z hasłem w pliku ani nie drukować środowiska.

```powershell
$names = @('Api__Server','Api__Port','Api__Database','Api__User','Api__Password')
$previous = @{}
foreach ($name in $names) {
    $previous[$name] = [Environment]::GetEnvironmentVariable($name, 'Process')
}
$afterPassword = $null
try {
    if ((git branch --show-current) -cne 'develop') { throw 'Wymagany klon develop.' }
    if ($env:DOTNET_ENVIRONMENT -and $env:DOTNET_ENVIRONMENT -ne 'Development') {
        throw 'Sprzeczne DOTNET_ENVIRONMENT; przygotuj czysta sesje.'
    }
    $afterPassword = Read-Host 'Haslo konta bazy AFTER' -AsSecureString
    if ($afterPassword.Length -eq 0) { throw 'Haslo nie moze byc puste.' }
    $env:Api__Server = '127.0.0.1'
    $env:Api__Port = '3306'
    $env:Api__Database = 'securenotes_db'
    $env:Api__User = 'sn_after_lab'
    $env:Api__Password = [Net.NetworkCredential]::new('', $afterPassword).Password
    Write-Host 'AFTER: develop | 127.0.0.1:3306 | securenotes_db | Development'
    Write-Warning 'Profil Development wywoluje Database.MigrateAsync tylko zatwierdzonej bazy AFTER.'
    dotnet run --project .\SecureNotes.API\SecureNotes.API.csproj --launch-profile https
    if ($LASTEXITCODE -ne 0) { throw 'Start AFTER zakonczony bledem; diagnozuj bez ujawniania sekretow.' }
} finally {
    foreach ($name in $names) {
        [Environment]::SetEnvironmentVariable($name, $previous[$name], 'Process')
    }
    $previous.Clear()
    if ($null -ne $afterPassword) { $afterPassword.Dispose() }
}
```

Fragment nie zmienia plików develop, wspólnych User Secrets ani lokalnego DPAPI BEFORE. Przy wymuszonym zabiciu hosta zamknij całą dedykowaną sesję. Nie zakłada on, że hasło AFTER jest zapisane przez launcher BEFORE.

5. W osobnym profilu AFTER otwórz HTTPS localhost:7028, zarejestruj własne fikcyjne konto i sprawdź zwykłą notatkę, odczyt/edycję/usunięcie własnych danych. Nie współdziel cookies/tokenów z BEFORE; sama inna karta/port nie zapewnia izolacji. BEFORE i AFTER uruchamiaj kolejno. Migracje i uprawnienia nowej bazy AFTER wymagają rzeczywistej kontroli operatora, a pełne porównania wykonuje się później według test-plan.md. Odmowa formularza w AFTER może wynikać z formatu/routingu, nie automatycznie z Antiforgery.

## I. Rozwiązywanie problemów

| Objaw | Kontrola i działanie |
|---|---|
| Brak .NET 8 | Sprawdź SDK i Microsoft.AspNetCore.App 8.x; zainstaluj brakujący składnik przed startem. Przy wielu SDK CLI może wybrać nowszy; wymagana jest dostępność frameworka 8 |
| Nie można uruchomić ps1 | Sprawdź politykę wykonywania i pochodzenie pliku; uzgodnij zasady stanowiska, bez globalnego wyłączania zabezpieczeń |
| MariaDB niedostępna | Sprawdź lokalną usługę i 127.0.0.1:3306 oraz rzeczywistą ścieżkę klienta; nie zmieniaj hosta na zdalny |
| Access denied / błędne hasło | Rozróżnij administratora setupu, sn_before_lab verify/startu, konto bazy AFTER i konto WWW; nie resetuj kont automatycznie |
| Brak/niewłaściwy branch | git branch --show-current; potrzebny klon Git z demo/security-before dla start-before. ZIP i detached HEAD nie spełniają kontroli |
| DPAPI nie daje się odczytać | Ten sam użytkownik Windows i komputer, dostęp do LocalAppData; -ResetStoredPassword podaje ponownie istniejące hasło. Nie wyświetlaj zawartości pliku, nie kopiuj go między komputerami |
| Poprawne odszyfrowanie, lecz baza odmawia | Zapis mógł zawierać literówkę/stare hasło; po potwierdzeniu konta popraw tylko lokalny zapis. Odszyfrowanie nie jest testem logowania SQL |
| JWT configuration not found / Invalid JWT configuration | Przygotuj cztery klucze Jwt w Development dla właściwego projektu/ID; sprawdź bezpiecznie kod wyjścia konfiguracji i kategorię błędu. Nie publikuj listy sekretów ani wartości |
| Niezaufane HTTPS | dev-certs check/trust oraz ustawienia konkretnej przeglądarki; nie ignoruj błędu TLS i nie osłabiaj Secure |
| Zajęty 7028/5070/7443 | Sprawdź proces nasłuchujący; zatrzymaj świadomie własną wcześniejszą instancję. Nie uruchamiaj obu wersji naraz |
| Brak tabel / nieudana migracja | Pusta baza przed startem jest oczekiwana; po starcie verify sprawdza InitialCreate. Częściowy schemat lub brak praw wymaga ręcznej diagnozy, bez resetu/rollbacku |
| Formularz nie zapisuje / 401 | Sprawdź zalogowany profil na właściwej wersji, rzeczywisty Host/Origin, Content-Type i żądanie. Port nie izoluje cookies. 401 nie dowodzi ochrony CSRF |
| iframe nie pokazuje JSON | To może być DENY/frame-ancestors. Rozstrzyga nowy zapis w UI/GET, nie widoczność ramki. Nie zmieniaj CSP aplikacji |
| Alert XSS nie działa | Sprawdź dokładną stronę /notes.html, nowy GET, zapisany payload, brakujący obraz, CSP i serwowany renderer; nie wyłączaj barier globalnie |

Na innym komputerze natywny prompt klienta, plugin uwierzytelniania MariaDB, zgodność wersji serwera, certyfikat/przeglądarka i przygotowanie AFTER wymagają własnej weryfikacji. Hasła sn_before_lab nie odzyskuje się z repozytorium ani z zapisu DPAPI innego użytkownika. Nie ma automatycznego odtworzenia wcześniejszych kont WWW/notatek. Skrypty nie naprawiają grantów, danych, migracji ani certyfikatów.

## J. Lista kontrolna operatora

Poniższe pola są puste celowo; nie są wynikami Codexa ani deklaracją zakończonego odtworzenia.

- [ ] Potwierdzono publikację właściwych commitów, branch i rzeczywiste SHA obu klonów; wymagane skrypty i instrukcja istnieją.
- [ ] Windows/PowerShell 5.1, Git, .NET 8 SDK i ASP.NET Core 8 są dostępne.
- [ ] MariaDB działa lokalnie na 3306; klient i bezpieczne monity haseł działają.
- [ ] BEFORE ma własną bazę/konto i dokładnie siedem praw; dane AFTER i testów nie zostały zmienione przez setup-before.
- [ ] JWT skonfigurowano lokalnie dla właściwego UserSecretsId, bez ujawniania wartości i bez plików sekretów w Git.
- [ ] Certyfikat localhost jest ważny i zaufany w używanej przeglądarce; porty nie kolidują.
- [ ] Pierwszy start BEFORE zapisał DPAPI poza repozytorium; kolejny start nie pyta o hasło; nie pomylono kont MariaDB/WWW.
- [ ] Migracja InitialCreate i tabele są potwierdzone; fikcyjne konto WWW działa i zwykła notatka pozostaje po restarcie.
- [ ] Demo uruchomiono osobno przez start-csrf-demo.ps1; świadomy formularz/iframe utworzył własną notatkę potwierdzoną w BEFORE.
- [ ] XSS odtworzono na fikcyjnym payloadzie, z ponownym GET i efektem przypisanym do konkretnego ID, bez zewnętrznych usług.
- [ ] AFTER przygotowano oddzielnie i faktycznie zweryfikowano procedurę nowego komputera; nigdy nie używa bazy BEFORE ani równoczesnego 7028.
- [ ] Porównania i dowody mają rzeczywiste SHA/datę/profil; dotychczasowych 15 statusów nie zmieniono bez wykonania właściwych scenariuszy.
- [ ] Sprzątnięto wyłącznie własne dane tej próby po ID, wylogowano profile i zatrzymano własne procesy; żadnych resetów istniejących baz.
