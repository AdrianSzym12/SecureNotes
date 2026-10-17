# Lokalna demonstracja CSRF

Stan 06F, 17.10.2026: użytkownik potwierdził działającą demonstrację cross-origin / same-site w BEFORE, najpierw z nawigacją głównej karty, następnie z ukrytym iframe. T-CSRF-B01 ma PARTIAL — BEFORE; AFTER nie badano. Wyniki serii: 6 PARTIAL — BEFORE / 9 NOT RUN. Codex nie uruchamiał serwerów, HTTP ani SQL.

Wymagania: .NET 8 SDK, istniejący ważny i zaufany certyfikat developerski dla localhost oraz wolny port 7443. Nie potrzeba administratora, dodatkowych pakietów NuGet ani zmian XAMPP/MariaDB. Projekt nie należy do rozwiązania SecureNotes i nie odwołuje się do jego projektów.

## Późniejsze uruchomienie

Pełne odtworzenie nowego komputera: [LAB-SETUP.md](../../../docs/before-after/LAB-SETUP.md). Użytkownik w 07A.2b potwierdził uruchomienie tego serwera skryptem i dostępność HTTPS 7443; Codex go nie uruchamiał. Wyłącznie po osobnym zatwierdzeniu testów, w drugim terminalu PowerShell z katalogu BEFORE:

```powershell
dotnet dev-certs https --check --trust
.\scripts\lab\start-csrf-demo.ps1
```

Pierwsze polecenie tylko sprawdza certyfikat; przygotowanie na nowym komputerze opisuje LAB-SETUP. Launcher ustala projekt względem siebie, sprawdza .NET 8 i wywołuje istniejące `dotnet run --project <projekt> --no-launch-profile`, bez pytań i bez startu BEFORE. Jeśli certyfikatu brak, jest niezaufany lub wygasł, zatrzymać przygotowanie; launcher nie instaluje ani nie generuje certyfikatów. Kestrel UseHttps korzysta z istniejącego certyfikatu developerskiego. Nie eksportować klucza, nie ignorować ostrzeżeń przeglądarki i nie osłabiać Secure cookies. Kod czyści odziedziczoną konfigurację demonstracji, więc nie używa konfiguracyjnych haseł, User Secrets ani alternatywnych endpointów.

Otworzyć ręcznie **https://localhost:7443/**. Logowanie serwera jest wyłączone, więc brak komunikatu „Now listening” jest oczekiwany. Zatrzymanie: Ctrl+C. Zwykły dotnet run może utworzyć lokalne bin/obj; kompilacja kontrolna etapu 06D kieruje artefakty poza repozytorium. Nie uruchamiać w sesji zawierającej niepotrzebne sekrety procesu.

## Późniejsza próba T-CSRF-B01

1. Uruchomić BEFORE zgodnie z [instrukcją laboratorium](../README.md), potwierdzić bazę securenotes_before_lab i zalogować własne fikcyjne konto na https://localhost:7028. Nie używać AFTER ani bazy testów automatycznych w tej próbie.
2. Zanotować SHA wraz z niezapisanym diffem, datę i przeglądarkę oraz własne notatki przed próbą. Otworzyć stronę 7443 w tym samym profilu jako samodzielną kartę. W DevTools włączyć Preserve log; nie utrwalać wartości cookies/tokenów.
3. Nadać notatce unikalny fikcyjny tytuł. Nacisnąć „Wyślij formularz CSRF”. To natywna nawigacja POST do ukrytego iframe `csrf-result-frame`, application/x-www-form-urlencoded, pola title/content, bez tokenu Antiforgery. Strona demonstracyjna pozostaje otwarta; wynik należy sprawdzić w SecureNotes. Nie ma JavaScript, fetch, automatycznego wysyłania ani komunikatu o sukcesie: strona z innego originu nie odczytuje odpowiedzi API.
4. Zarejestrować rzeczywisty Origin, Content-Type, obecność cookie bez jego wartości, status odpowiedzi, ID i Location. Wrócić do UI BEFORE, odświeżyć notatki i potwierdzić nowy zapis; użytkownik może osobno potwierdzić rekord w laboratoryjnej bazie. Samo 201 nie wystarcza bez zapisu. 401 dotyczy uwierzytelnienia; dla 400/403/415 ustalić przyczynę, bez automatycznego przypisywania jej Antiforgery.
5. Zanonimizować materiały przed publikacją (także identyfikatory użytkowników i niepotrzebne dane phpMyAdmin). Nazwy plików i ID dowodów nadać dopiero po archiwizacji. Usunąć przez UI wyłącznie własną notatkę testową z prawidłowym tokenem i potwierdzić brak w odczycie.

Użytkownik 17.10.2026 potwierdził utworzenie `SN-T-CSRF-B01`, odpowiedź JSON i zapis widoczny w UI we wcześniejszym wariancie z nawigacją głównej karty. Następnie potwierdził wariant iframe: pozostanie na stronie demonstracyjnej, utworzenie `SN-T-CSRF-B02`, widoczność w UI i nowy rekord MariaDB. Oba przebiegi należą do T-CSRF-B01; nazwa drugiej notatki nie oznacza wykonania T-CSRF-B02 (próba bez uwierzytelnienia), który pozostaje NOT RUN. Dokładnych kodów HTTP i nagłówków nie udokumentowano. Nie przypisano screenshotom nazw/ścieżek/ID — archiwizacja pozostaje otwarta.

W 06D.1 wyłącznie CSP serwera demonstracyjnego otrzymała `frame-src https://localhost:7028`; zachowano istniejące `form-action https://localhost:7028`, `default-src 'none'`, `base-uri 'none'` i `frame-ancestors 'none'`. Nie zmieniono CSP ani `X-Frame-Options` SecureNotes: jego `frame-ancestors 'none'` i `X-Frame-Options: DENY` mogą zablokować wyświetlenie odpowiedzi, co samo nie oznacza błędu POST ani braku zapisu. Nie zgłoszono konkretnego komunikatu blokady w potwierdzonych próbach. Przy odtworzeniu obserwować Network; powodzenie potwierdza rzeczywisty zapis w SecureNotes. Strona demonstracyjna nie odczytuje odpowiedzi i nie wyświetla komunikatu o sukcesie.

## Ograniczenia i cookies

ListenLocalhost wiąże port 7443 wyłącznie z loopback IPv4/IPv6, bez adresów sieciowych. Brak endpointu HTTP. Dodatkowo odrzucane są żądania z inną nazwą hosta/portem. Udostępniono tylko GET /; nie włączono katalogu plików statycznych ani przyjmowania danych formularza na serwerze demonstracyjnym. Docelowy POST idzie bezpośrednio z przeglądarki do SecureNotes.

Cookies nie są izolowane portem: przeglądarka może wysłać SecureNotes.Auth również przy otwarciu strony 7443. Kod ich nie odczytuje, nie wypisuje, nie zapisuje i nie przekazuje. Wyłączono dostawców logowania; brak HTTP logging, uwierzytelniania, bazy, zapisu na dysk i zewnętrznych usług. Nie uruchamiać za proxy ani z narzędziem przechwytującym sesje. CSP strony zachowuje domyślną blokadę zasobów i skryptów; formularz i nawigację iframe dopuszcza wyłącznie do https://localhost:7028.

Główna próba jest **cross-origin / same-site**, bez konieczności pomyślnego CORS preflight dla nawigacji formularza. Nie dowodzi obejścia SameSite=Strict ani ataku cross-site. Nie otwierać tej demonstracji przez IP, file:// ani iframe. Kontrola cross-site z 127.0.0.1 wymaga osobnego planu i certyfikatu; ten serwer celowo odrzuca taki Host. JWT, cookies, CORS i CSP SecureNotes pozostają niezmienione.
