# SecureNotes — Wyniki napraw i retestów bezpieczeństwa

## 1. Cel dokumentu

Dokument przedstawia działania naprawcze podjęte po zakończeniu audytu bezpieczeństwa SecureNotes oraz wyniki ponownej weryfikacji zabezpieczeń.

Pierwotne wyniki audytu znajdują się w `results.md`. Nie są nadpisywane, aby zachować możliwość porównania stanu aplikacji przed naprawą i po niej.

**Branch naprawczy:** `fix/security-hardening`

**Data rozpoczęcia retestów:** 11.10.2026

---

## 2. REM-01 — Cache-Control dla odpowiedzi API

### Problem wykryty podczas audytu

W ramach analizy pasywnej OWASP ZAP (ZAP-01) dodatkowo sprawdzono odpowiedź `GET /api/notes`.

Serwer zwrócił HTTP 200 OK z prywatnymi notatkami zalogowanego użytkownika, ale w nagłówkach odpowiedzi nie stwierdzono obecności `Cache-Control`.

Brak tego nagłówka nie stanowił samodzielnie dowodu wycieku danych, lecz wskazywał na potrzebę jawnego określenia zasad przechowywania wrażliwych odpowiedzi przez pamięci podręczne.

### Zastosowana poprawka

Zmodyfikowano:

`SecureNotes.API/Middleware/SecurityHeadersMiddleware.cs`

W istniejącym mechanizmie `Response.OnStarting()` dodano warunek:

```csharp
if (context.Request.Path.StartsWithSegments("/api"))
{
    headers["Cache-Control"] = "no-store";
}
```

Poprawka obejmuje odpowiedzi kierowane do ścieżek `/api` i ich podścieżek, bez wprowadzania tej samej polityki dla publicznych plików statycznych.

Pozostałe nagłówki bezpieczeństwa, w tym restrykcyjna polityka CSP, zostały zachowane.

### Wyniki retestów ręcznych

| ID | Sprawdzony zasób | Wynik oczekiwany | Wynik rzeczywisty | Status |
|---|---|---|---|---|
| CACHE-01 | GET `/api/notes` | HTTP 200, `Cache-Control: no-store` | HTTP 200, `no-store` | PASS |
| CACHE-02 | GET `/api/auth/me` | HTTP 200, `Cache-Control: no-store` | HTTP 200, `no-store` | PASS |
| CACHE-03 | GET `/api/auth/csrf-token` | HTTP 200, `Cache-Control: no-store` | HTTP 200, `no-store` | PASS |
| CACHE-04 | GET `/css/style.css` | HTTP 200, bez nowego `Cache-Control: no-store` | HTTP 200, brak `no-store` | PASS |

### Materiały dowodowe

**CACHE-01 — prywatne notatki:**

![GET api/notes — Cache-Control no-store](screenshots/retest/cache-01-notes-no-store.png)

**Dodatkowa kontrola działania aplikacji po wyłączeniu Hot Reload:**

![GET api/notes — ponowna weryfikacja](screenshots/retest/cache-02-notes-after-hot-reload-disabled.png)

**CACHE-02 — informacje o zalogowanym użytkowniku:**

![GET api/auth/me — Cache-Control no-store](screenshots/retest/cache-03-auth-me-no-store.png)

**CACHE-03 — endpoint tokenu CSRF:**

![GET api/auth/csrf-token — Cache-Control no-store](screenshots/retest/cache-04-csrf-token-no-store.png)

**CACHE-04 — kontrola pliku statycznego:**

![GET css/style.css — bez dodatkowego no-store](screenshots/retest/cache-05-static-css-unaffected.png)

Materiały dowodowe ograniczono do informacji niezbędnych do potwierdzenia wyników. Nie publikowano wartości JWT, tokenów CSRF ani prywatnej treści odpowiedzi API.

### Testy regresyjne

Po wprowadzeniu poprawki wykonano:

- Rebuild Solution — zakończony sukcesem.
- Test Explorer — 82/82 testy automatyczne zakończone sukcesem.
- Ręczną kontrolę działania strony notatek — aplikacja wyświetlała zapisane notatki poprawnie.

### Wynik końcowy REM-01

**PASS — poprawka potwierdzona dla wszystkich czterech sprawdzonych przypadków.**

Badane odpowiedzi API otrzymują `Cache-Control: no-store`, natomiast sprawdzony publiczny plik CSS nie został objęty nową polityką.

---

## 3. REM-02 — Konfiguracja HSTS

### Problem wykryty podczas audytu

Podczas analizy pasywnej OWASP ZAP (ZAP-01) wykryto brak nagłówka `Strict-Transport-Security` w odpowiedziach lokalnej aplikacji uruchomionej w środowisku Development.

### Zastosowana poprawka

W pliku `SecureNotes.API/Program.cs` zarejestrowano politykę HSTS:

```csharp
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(30);
    options.IncludeSubDomains = false;
    options.Preload = false;
});
```

Middleware HSTS włączono warunkowo poza środowiskiem Development, przed `UseHttpsRedirection()`:

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
```

Nie zmieniano istniejących profili lokalnego uruchamiania ani polityki CSP.

### Weryfikacja

|   ID    |             Sprawdzany warunek          |          Wynik                            | Status |
|---------|-----------------------------------------|-------------------------------------------|--------|
| HSTS-01 | Izolowany potok Production + HTTPS      | `max-age=2592000`                         | PASS   |
| HSTS-02 | Izolowany potok Development + HTTPS     | Brak nagłówka HSTS                        | PASS   |
| HSTS-03 | Izolowana polityka Production           | Brak `includeSubDomains` oraz `preload`   | PASS   |

Zweryfikowano również zawartość rzeczywistego `Program.cs` oraz konfigurację `launchSettings.json`.

Testy HSTS wykorzystują izolowany potok HTTP odtwarzający konfigurację aplikacji. Nie uruchamiają rzeczywistego `Program.Main()` ani nie korzystają z MariaDB.

### Ograniczenia

Nie przeprowadzono testu rzeczywistego wdrożenia Production.

Przed publikacją aplikacji należy sprawdzić działanie HSTS na docelowej domenie HTTPS, poprawność certyfikatu oraz przekierowania HTTP do HTTPS.

Brak HSTS podczas lokalnego uruchomienia w środowisku Development jest zgodny z przyjętą konfiguracją.

### Testy regresyjne

Po zastosowaniu poprawki wykonano:

-Rebuild Solution — zakończony sukcesem.

-Run All Tests — 85/85 testów zakończonych sukcesem.

-Nie stwierdzono niepowodzeń w istniejących testach automatycznych.

**Wynik:** 85/85 PASS.

**Dowód:** `screenshots/retest/hsts-01-automated-tests.png`

![REM-02 — testy automatyczne po konfiguracji HSTS](screenshots/retest/hsts-01-automated-tests.png)

### Wynik końcowy REM-02

PASS — implementacja i izolowane testy konfiguracji HSTS zakończone sukcesem.

Weryfikacja rzeczywistego wdrożenia produkcyjnego pozostaje zadaniem przedwdrożeniowym.

Data retestu: 11.10.2026.

---

## 4. REM-03 — Ocena ograniczeń środowiska Development

### ASP.NET Core Browser Refresh

W ramach kontroli środowiska Development wyłączono Hot Reload w Visual Studio 2022.

Po ponownym uruchomieniu aplikacji potwierdzono, że pomocniczy mechanizm Browser Refresh nie podejmuje już obserwowanych wcześniej prób połączenia WebSocket z portem 44341.

Aplikacja SecureNotes nadal działała prawidłowo, a odpowiedź GET `/api/notes` zwracała HTTP 200 oraz `Cache-Control: no-store`.

Wyłączenie Hot Reload jest lokalnym ustawieniem środowiska programistycznego, a nie zmianą zabezpieczeń aplikacji.

### Swagger UI i Content Security Policy

W trakcie audytu stwierdzono ograniczenie działania interfejsu Swagger UI wynikające z restrykcyjnej polityki CSP.

Podjęto decyzję o pozostawieniu globalnej polityki CSP bez zmian. Nie dodawano dyrektyw `unsafe-inline` ani wyjątków osłabiających zabezpieczenia właściwej aplikacji.

Swagger UI pozostaje dostępny konfiguracyjnie wyłącznie w środowisku Development. Do przeglądania dokumentacji API można również wykorzystać endpoint `/swagger/v1/swagger.json`.

Ewentualne tymczasowe wyłączenie CSP na potrzeby lokalnego debugowania nie stanowi konfiguracji docelowej i nie jest uwzględniane jako dowód zaliczenia testów bezpieczeństwa.

### Ocena REM-03

**ZAMKNIĘTE — świadomie zaakceptowano ograniczenie narzędzia deweloperskiego.**

Nie stwierdzono potrzeby modyfikowania globalnej polityki CSP. Nie wprowadzono zmian w kodzie aplikacji w ramach REM-03.

---

## 5. ZAP-02 — Kontrolna analiza pasywna po naprawach

### Cel

Ponowna analiza aplikacji SecureNotes przy użyciu OWASP ZAP 2.17.0 w celu zweryfikowania działań naprawczych po pierwotnym audycie ZAP-01.

Test wykonano w trybie Safe Mode, korzystając z Manual Explore i lokalnej aplikacji uruchomionej w środowisku Development pod adresem `https://localhost:7028`.

Nie wykonywano Active Scan, Spider ani fuzzingu.

### Wyniki porównawcze

|               Kategoria alertu                |   ZAP-01      |       ZAP-02              |                       Ocena                                   |
|-----------------------------------------------|---------------|---------------------------|---------------------------------------------------------------|
| Cross-Domain Misconfiguration                 | Występował    | Występuje                 | Zewnętrzna domena Edge — poza zakresem                        |
| Strict-Transport-Security Header Not Set      | Występował    | Występuje                 | Lokalny Development; zgodne z konfiguracją HSTS               |
| X-Content-Type-Options Header Missing         | 3 wystąpienia | 2 wystąpienia             | W ZAP-02 wyłącznie zewnętrzne domeny                          |
| Authentication Request Identified             | Występował    | Występuje                 | Informacyjne rozpoznanie endpointu logowania                  |
| Information Disclosure – Suspicious Comments  | Występował    | Nie występuje na liście   | Nie zaobserwowano ponownie alertu dotyczącego Browser Refresh |
| Re-examine Cache-control Directives           | Występował    | Występuje                 | Zweryfikowano indywidualnie odpowiedzi API                    |
| Session Management Response Identified        | 3 wystąpienia | 3 wystąpienia             | Informacyjne rozpoznanie mechanizmów sesyjnych                |

### Weryfikacja Cache-Control

ZAP-02 przechwycił odpowiedź:

`GET https://localhost:7028/api/notes`

Potwierdzono:
- HTTP 200 OK;
- `Cache-Control: no-store`;
- obecność pozostałych nagłówków bezpieczeństwa, w tym CSP i `X-Content-Type-Options: nosniff`.

Endpoint `/api/notes` nie znajdował się na liście wystąpień alertu `Re-examine Cache-control Directives (Systemic)` w analizowanej sesji.

Dodatkowo szczegóły alertu przypisanego do `/api/auth/me` zawierały dowód `no-store`. Obecność ogólnej reguły informacyjnej nie oznaczała więc braku zastosowanej poprawki.

### Pozostałe ustalenia

Dwa wystąpienia `X-Content-Type-Options Header Missing` w ZAP-02 dotyczyły wyłącznie zewnętrznych domen `edge-consumer-static.azureedge.net` oraz `xpaywalletcdn-prod.azureedge.net`.

Alert CORS również dotyczył zewnętrznego zasobu Edge.

HSTS nie był wysyłany przez lokalne środowisko Development, zgodnie z przyjętą konfiguracją. Test rzeczywistego wdrożenia Production pozostaje wymaganiem przedwdrożeniowym.

Alerty dotyczące rozpoznania żądania logowania i mechanizmów sesyjnych zachowały charakter informacyjny.

### Materiały dowodowe

- `screenshots/zap-retest/zap-02-cache-notes-no-store.png`
- `screenshots/zap-retest/zap-02-cache-auth-me-alert.png`
- `screenshots/zap-retest/zap-02-nosniff-external-only.png`
- `screenshots/zap-retest/zap-02-hsts-development.png`
- `screenshots/zap-retest/zap-02-cors-external-domain.png`
- `screenshots/zap-retest/zap-02-auth-session-info.png`

Wszystkie materiały przeznaczone do repozytorium powinny być pozbawione wartości JWT, ciasteczek, tokenów CSRF oraz prywatnej treści notatek.

### Wynik końcowy

**ZAP-02: PASS z udokumentowanymi ustaleniami.**

Kontrolna analiza pasywna potwierdziła skuteczność poprawki Cache-Control dla badanej odpowiedzi zawierającej prywatne notatki.

Liczba zaobserwowanych kategorii alertów zmieniła się z siedmiu podczas ZAP-01 do sześciu podczas ZAP-02. Wynik ten dotyczy zakresu ruchu przechwyconego w obu sesjach i 
    nie stanowi dowodu wyeliminowania wszystkich możliwych podatności.

**Data retestu:** 11.10.2026.

---

## 6. Rozszerzone testy bezpieczeństwa HTTP — ZAP-03–ZAP-06

### 6.1. Cel i zakres

Po zakończeniu kontrolnej analizy pasywnej ZAP-02 przeprowadzono dodatkowe testy bezpieczeństwa aplikacji SecureNotes przy użyciu OWASP ZAP 2.17.0.

Zakres rozszerzono o:
- kontrolę odpowiedzi na metodę OPTIONS i nagłówków CORS;
- obsługę błędów HTTP oraz zachowanie nagłówków bezpieczeństwa;
- kontrolę dostępności wybranych plików konfiguracyjnych i zasobów diagnostycznych;
- ograniczony Spider służący do automatycznego odkrywania publicznych zasobów aplikacji.

Ręczne żądania wykonywano przez ZAP Requester w trybie Protected Mode. Zakres ograniczono do lokalnej aplikacji `https://localhost:7028`.

Nie uruchamiano Active Scan ani fuzzingu. Nie wykonywano ponownie pełnego zestawu wcześniejszych scenariuszy AUTH, IDOR, CSRF i XSS.

### 6.2. ZAP-03 — Kontrola OPTIONS i CORS

Wysłano żądanie OPTIONS do `/api/notes`, zawierające nagłówki:

```http
Origin: https://example.org
Access-Control-Request-Method: GET
```

Nie dołączano JWT, ciasteczek ani innych danych uwierzytelniających.

**Uzyskany wynik:**

|            Element                |       Wartość             |
|-----------------------------------|---------------------------|
| Status HTTP                       | 405 Method Not Allowed    |
| Allow                             | GET, POST                 |
| Access-Control-Allow-Origin       | Brak                      |
| Access-Control-Allow-Credentials  | Brak                      |
| Cache-Control                     | no-store                  |
| Pozostałe nagłówki bezpieczeństwa | Obecne                    |

W badanym przypadku aplikacja nie zezwoliła na udostępnienie odpowiedzi obcemu pochodzeniu za pomocą nagłówków CORS. Sama odpowiedź 405 nie stanowi podatności.

**ZAP-03: PASS — nie wykazano niepożądanej konfiguracji CORS w badanym przypadku.**

![ZAP-03 — kontrola OPTIONS i CORS](screenshots/zap-retest/zap-03-options-notes.png)

### 6.3. ZAP-04 — Obsługa błędów HTTP

Wykonano serię ręcznych żądań sprawdzających odpowiedzi błędów, obecność nagłówków bezpieczeństwa oraz możliwość przypadkowego ujawnienia danych technicznych.

|   ID    |             Żądanie                     |    Wynik HTTP |                        Rezultat                                                     |
|---------|-----------------------------------------|---------------|-------------------------------------------------------------------------------------|
| ZAP-04A | GET `/api/nonexistent-security-check`   | 404           | Pusta odpowiedź; nagłówki bezpieczeństwa obecne — PASS                              |
| ZAP-04B | GET `/api/notes` bez uwierzytelnienia   | 401           | Brak danych prywatnych; `WWW-Authenticate: Bearer` — PASS                           |
| ZAP-04C | GET `/api/notes/invalid-id`             | 404           | Brak ujawnienia szczegółów technicznych — PASS w zakresie obsługi błędnego adresu   |
| ZAP-04D | POST `/api/notes` bez uwierzytelnienia  | 401           | Żądanie odrzucone; brak ujawnienia danych — PASS w zakresie odpowiedzi błędu        |
| ZAP-04E | GET `/nonexistent-security-check.html`  | 404           | Pusta odpowiedź; nagłówki bezpieczeństwa obecne — PASS                              |

W odpowiedziach API potwierdzono `Cache-Control: no-store`. Zachowane zostały również restrykcyjna polityka CSP, `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY` oraz pozostałe skonfigurowane nagłówki.

W odpowiedzi 404 dla nieistniejącej strony HTML nie stwierdzono `Cache-Control: no-store`, co jest zgodne z zakresem poprawki REM-01 obejmującej ścieżki `/api`.

**Ograniczenie interpretacyjne:** żądanie ZAP-04C nie stanowiło pełnego testu walidacji identyfikatora, ponieważ odpowiedź 404 mogła wynikać z niedopasowania trasy. W ZAP-04D serwer odrzucił nieuwierzytelnione żądanie, a zamierzona niepoprawna treść JSON nie została przekazana jako właściwe body. Nie oznaczono więc tego przypadku jako zaliczonego testu parsera JSON.

**ZAP-04: PASS w zakresie wykonanych kontroli obsługi odpowiedzi błędów.** Nie stwierdzono ujawnienia prywatnych notatek, stosów wyjątków ani ścieżek systemowych w badanych odpowiedziach.

**Materiały dowodowe:**

![ZAP-04A — nieistniejący endpoint API](screenshots/zap-retest/zap-04a-api-404.png)

![ZAP-04B — nieuwierzytelniony dostęp](screenshots/zap-retest/zap-04b-api-401.png)

![ZAP-04C — nieprawidłowy adres notatki](screenshots/zap-retest/zap-04c-invalid-note-id.png)

![ZAP-04D — odrzucone żądanie POST](screenshots/zap-retest/zap-04d-post-unauthorized.png)

![ZAP-04E — nieistniejąca strona HTML](screenshots/zap-retest/zap-04e-html-404.png)

### 6.4. ZAP-05 — Kontrola udostępniania zasobów

Sprawdzono dostępność wybranych plików konfiguracyjnych, zasobu repozytorium Git, dokumentacji OpenAPI oraz przykładowego endpointu diagnostycznego.

Żądania wykonywano metodą GET bez uwierzytelnienia.

|   ID    |         Badany zasób            | Wynik HTTP |                          Ocena                                          |
|---------|---------------------------------|------------|-------------------------------------------------------------------------|
| ZAP-05A | `/.env`                         | 404        | Plik nie został udostępniony — PASS                                     |
| ZAP-05B | `/appsettings.json`             | 404        | Konfiguracja nie została udostępniona — PASS                            |
| ZAP-05C | `/appsettings.Development.json` | 404        | Konfiguracja Development nie została udostępniona — PASS                |
| ZAP-05D | `/.git/config`                  | 404        | Konfiguracja Git nie została udostępniona — PASS                        |
| ZAP-05E | `/swagger/v1/swagger.json`      | 200        | Dokumentacja OpenAPI dostępna zgodnie z konfiguracją Development        |
| ZAP-05F | `/health`                       | 404        | Nie stwierdzono udostępnienia badanego endpointu diagnostycznego — PASS |

W badanych odpowiedziach 404 nie stwierdzono ujawnienia zawartości plików ani szczegółów technicznych. Nagłówki bezpieczeństwa pozostały aktywne.

Endpoint `/swagger/v1/swagger.json` zwrócił dokument OpenAPI 3.0.1 z typem `application/json; charset=utf-8`. Potwierdzono obecność nagłówków bezpieczeństwa. Widoczny fragment dokumentu zawierał standardowe informacje o strukturze API; nie przeprowadzano pełnego przeglądu wszystkich wartości dokumentu pod kątem sekretów.

Dostępność OpenAPI JSON w Development potwierdza możliwość korzystania z dokumentacji API bez osłabiania globalnej polityki CSP na potrzeby interfejsu Swagger UI.

**ZAP-05: 6/6 przypadków zgodnych z oczekiwaniami.**

**Materiały dowodowe:**

![ZAP-05A — kontrola pliku .env](screenshots/zap-retest/zap-05a-env-not-exposed.png)

![ZAP-05B — kontrola appsettings.json](screenshots/zap-retest/zap-05b-appsettings-not-exposed.png)

![ZAP-05C — kontrola konfiguracji Development](screenshots/zap-retest/zap-05c-development-config-not-exposed.png)

![ZAP-05D — kontrola konfiguracji Git](screenshots/zap-retest/zap-05d-git-config-not-exposed.png)

![ZAP-05E — dokumentacja OpenAPI](screenshots/zap-retest/zap-05e-openapi-development.png)

![ZAP-05F — kontrola endpointu health](screenshots/zap-retest/zap-05f-health-not-exposed.png)

### 6.5. ZAP-06 — Ograniczony Spider

Przeprowadzono automatyczne odkrywanie zasobów aplikacji za pomocą modułu Spider w OWASP ZAP 2.17.0.

Celem testu było sprawdzenie publicznej struktury aplikacji SecureNotes oraz zweryfikowanie, czy ruch wygenerowany przez Spider spowoduje pojawienie się nowych alertów analizy pasywnej.

**Konfiguracja skanowania:**

|   Parametr                    |           Ustawienie                                  |
|-------------------------------|-------------------------------------------------------|
| Tryb ZAP                      | Protected Mode                                        |
| Punkt początkowy              | `https://localhost:7028`                              |
| Kontekst                      | Domyślny kontekst, ograniczony do lokalnej aplikacji  |
| Użytkownik uwierzytelniony    | Brak                                                  |
| Maximum Depth to Crawl        | 3                                                     |
| Maximum Children to Crawl     | 10                                                    |
| Maximum Duration              | 2 minuty                                              |
| Maximum Parse Size            | 2621440 bajtów                                        |
| Accept Cookies                | Wyłączone                                             |
| Process Forms                 | Wyłączone                                             |
| POST Forms                    | Wyłączone                                             |
| Parse HTML Comments           | Włączone                                              |
| Parse robots.txt              | Włączone                                              |
| Parse sitemap.xml             | Włączone                                              |
| Parse SVN Metadata            | Wyłączone                                             |
| Parse Git Metadata            | Wyłączone                                             |
| Parse .DS_Store Files         | Wyłączone                                             |
| Handle OData Parameters       | Wyłączone                                             |
| Logout Avoidance              | Wyłączone                                             |   
Zastosowane ograniczenia miały umożliwić przeszukiwanie publicznej części SecureNotes bez automatycznego przetwarzania formularzy i wykonywania operacji modyfikujących notatki.

#### Wynik Spidera

|       Wskaźnik                | Wartość |
|-------------------------------|---------|
| Postęp                        | 100%    |
| URLs Found                    | 15      |
| Nodes Added                   | 3       |
| Aktualnie skanowane adresy    | 0       |

Wszystkie 15 adresów przedstawionych na liście miało oznaczenie `Seed`, dlatego nie interpretowano tego wyniku jako odkrycia 15 nowych zasobów.

W zakładce **Added Nodes** widoczne były trzy węzły:

- `/api/auth`;
- `/api/auth/login`;
- `/swagger/v1`.

Wszystkie należały do lokalnej aplikacji i odpowiadały znanym obszarom SecureNotes. Nie zaobserwowano nowej, nieznanej ścieżki wymagającej natychmiastowej poprawki.

#### Kontrola alertów po zakończeniu Spidera

Po zakończeniu skanowania sprawdzono listę alertów analizy pasywnej.

Nadal występowało sześć kategorii:

|               Kategoria                               |                                   Wynik analizy                                                   |
|-------------------------------------------------------|---------------------------------------------------------------------------------------------------|
| Cross-Domain Misconfiguration                         | Zgłoszenie dotyczące zewnętrznego zasobu Edge                                                     |
| Strict-Transport-Security Header Not Set (Systemic)   | Lokalny Development oraz zasób zewnętrzny                                                         |
| X-Content-Type-Options Header Missing                 | Dwa wystąpienia dotyczące domen zewnętrznych                                                      |
| Authentication Request Identified                     | Informacyjne rozpoznanie endpointu logowania                                                      |
| Re-examine Cache-control Directives (Systemic)        | Wcześniej przeanalizowany alert informacyjny; potwierdzono `no-store` dla badanych odpowiedzi API |
| Session Management Response Identified                | Informacyjne rozpoznanie mechanizmów sesyjnych                                                    |

Nie zaobserwowano nowej kategorii alertów względem ZAP-02.

Nie oznacza to wyeliminowania wszystkich możliwych podatności — wynik odnosi się do ograniczonego zakresu przeszukiwania oraz ruchu przeanalizowanego przez skaner pasywny.

**ZAP-06: PASS — ograniczony Spider zakończył działanie, a w badanym zakresie nie wykryto nowych kategorii alertów.**

#### Materiały dowodowe

**Zakończony Spider — postęp 100%:**

![ZAP-06 — zakończone skanowanie Spider](screenshots/zap-retest/zap-06-spider-completed.png)

**Trzy węzły dodane do drzewa aplikacji:**

![ZAP-06 — Added Nodes](screenshots/zap-retest/zap-06-spider-added-nodes.png)

**Końcowa lista alertów analizy pasywnej:**

![ZAP-06 — lista alertów po zakończeniu Spidera](screenshots/zap-retest/zap-06-final-alerts.png)

---

### 6.6. Podsumowanie rozszerzonej serii testów

W ramach dodatkowej weryfikacji po działaniach naprawczych wykonano cztery scenariusze ZAP-03–ZAP-06.

| Scenariusz |                  Zakres                                  |                   Wynik końcowy                       |
|------------|----------------------------------------------------------|-------------------------------------------------------|
| ZAP-03     | Metoda OPTIONS i nagłówki CORS                           | PASS                                                  |
| ZAP-04     | Obsługa błędów HTTP i kontrola nagłówków bezpieczeństwa  | PASS w zakresie wykonanych przypadków                 |
| ZAP-05     | Dostępność wybranych plików i zasobów diagnostycznych    | 6/6 wyników zgodnych z oczekiwaniami                  |
| ZAP-06     | Ograniczony Spider i kontrola alertów pasywnych          | PASS — skan zakończony, bez nowych kategorii alertów  |

#### Najważniejsze wnioski

1. W badanym żądaniu OPTIONS nie stwierdzono niepożądanego udostępnienia endpointu `/api/notes` obcemu pochodzeniu za pomocą nagłówków CORS.
2. Odpowiedzi błędów HTTP zachowały skonfigurowane nagłówki bezpieczeństwa. W badanych przypadkach nie stwierdzono ujawnienia prywatnych notatek, stosów wyjątków ani wewnętrznych ścieżek systemowych.
3. Wybrane pliki konfiguracyjne i zasoby repozytorium nie były publicznie udostępniane przez HTTP.
4. Dokumentacja OpenAPI JSON była dostępna zgodnie z konfiguracją środowiska Development, bez konieczności osłabiania globalnej polityki CSP.
5. Ograniczony Spider zakończył działanie i nie spowodował pojawienia się nowych kategorii alertów pasywnych względem ZAP-02.
6. Nie zidentyfikowano nowego problemu wymagającego natychmiastowej modyfikacji kodu SecureNotes.

#### Ograniczenia testów

Wyniki dotyczą wyłącznie sprawdzonych endpointów, wybranych ścieżek i skonfigurowanego zakresu Spidera.

Spider nie korzystał z uwierzytelnionej sesji użytkownika. Wyłączono przyjmowanie ciasteczek oraz automatyczne przetwarzanie i wysyłanie formularzy.

Nie przeprowadzano Active Scan ani fuzzingu.

Zakończenie rozszerzonej serii testów nie jest równoznaczne z potwierdzeniem całkowitej odporności aplikacji na wszystkie możliwe zagrożenia.

**Ocena końcowa ZAP-03–ZAP-06: PASS w zakresie wykonanych testów i udokumentowanych wyników.**

**Data wykonania:** 11.10.2026.

## 7. Dodatkowe testy uwierzytelnione i fuzzing — ZAP-07–ZAP-08

### 7.1. Cel i przygotowanie środowiska

Po zakończeniu testów ZAP-03–ZAP-06 przeprowadzono dodatkową weryfikację bezpieczeństwa SecureNotes obejmującą uwierzytelnionego Spidera oraz ograniczony fuzzing.

Testy wykonano w OWASP ZAP 2.17.0 na oddzielnej instancji aplikacji:

|       Element             |       Konfiguracja                    |
|---------------------------|---------------------------------------|
| Adres aplikacji           | `https://localhost:7038`              |
| Środowisko ASP.NET Core   | Development                           |
| Baza danych               | `securenotes_zap_db`                  |
| Użytkownik MariaDB        | `securenotes_zap_user`                |
| Kontekst OWASP ZAP        | `SecureNotes-ZAP-Testing`             |
| Tryb OWASP ZAP            | Protected Mode                        |
| Konto aplikacji           | Oddzielne konto z fikcyjnymi danymi   |
| Dane testowe              | Dwie fikcyjne notatki                 |

Nową bazę utworzono w phpMyAdmin. Użytkownikowi MariaDB przyznano uprawnienia ograniczone do `securenotes_zap_db`, bez globalnych uprawnień administracyjnych.

API uruchomiono osobno, z wykorzystaniem tymczasowych zmiennych środowiskowych, bez modyfikowania dotychczasowego `appsettings.Development.json`.

Istniejące migracje Entity Framework Core poprawnie utworzyły strukturę nowej bazy. Potwierdzono działanie aplikacji oraz dostępność dokumentacji `/swagger/v1/swagger.json`.

Nie korzystano z głównej bazy SecureNotes ani z bazy `securenotes_test_db`, przeznaczonej dla testów infrastruktury.

---

### 7.2. ZAP-07 — Spider z uwierzytelnioną sesją

**Cel:** sprawdzenie publicznych i wymagających uwierzytelnienia zasobów aplikacji za pomocą ograniczonego Spidera.

#### Przygotowanie uwierzytelnienia

W kontekście `SecureNotes-ZAP-Testing` skonfigurowano:

- zakres ograniczony do `https://localhost:7038`;
- ręczną metodę uwierzytelniania (Manual Authentication);
- zarządzanie sesją oparte na ciasteczkach (Cookie-Based Session Management).

Ciasteczko `SecureNotes.Auth`, przenoszące JWT, oznaczono w OWASP ZAP jako Session Token. Nie oznaczano w ten sposób osobnego ciasteczka `SecureNotes.Csrf`.

Po ponownym zalogowaniu fikcyjnego użytkownika zarejestrowaną sesję HTTP ustawiono jako aktywną.

Przed rozpoczęciem Spidera wykonano kontrolne żądanie `GET /api/notes` za pomocą ZAP Requester. Po usunięciu z edytowanego żądania wcześniej przechwyconego nagłówka `Cookie` uzyskano HTTP 200 OK i dwie fikcyjne notatki. Wynik potwierdził wykorzystanie aktywnej sesji podczas tego żądania generowanego przez ZAP.

#### Konfiguracja Spidera

|       Parametr            |               Ustawienie                      |
|---------------------------|-----------------------------------------------|
| Punkt początkowy          | Kontekst `SecureNotes-ZAP-Testing`            |
| Kontekst                  | `SecureNotes-ZAP-Testing`                     |
| Użytkownik ZAP            | Nie wskazano — wykorzystano aktywną sesję HTTP|
| Maximum Depth to Crawl    | 3                                             |
| Maximum Children to Crawl | 10                                            |
| Maximum Duration          | 2 minuty                                      |
| Maximum Parse Size        | 2621440 bajtów                                |
| Accept Cookies            | Włączone                                      |
| Process Forms             | Wyłączone                                     |
| POST Forms                | Wyłączone                                     |
| Spider Subtree Only       | Wyłączone                                     |
| Parse HTML Comments       | Włączone                                      |
| Parse robots.txt          | Włączone                                      |
| Parse sitemap.xml         | Włączone                                      |
| Parse SVN/Git/.DS_Store   | Wyłączone                                     |

Nie uruchamiano Active Scan. Wyłączono automatyczne przetwarzanie oraz wysyłanie formularzy.

#### Wynik Spidera

|           Wskaźnik            | Wynik |
|-------------------------------|------:|
| Postęp                        | 100%  |
| URLs Found                    | 21    |
| Nodes Added                   | 9     |
| Aktualnie skanowane adresy    | 0     |

Wszystkie 21 pozycji w zakładce adresów URL miało oznaczenie `Seed`. Nie traktowano ich więc jako 21 nowo odkrytych endpointów.

#### Potwierdzenie uwierzytelnienia podczas skanowania

W zakładce **Spider → Messages**, zawierającej żądania wykonane podczas tego konkretnego przebiegu, potwierdzono:

|               Żądanie         |       Status HTTP         |                   Znaczenie                           |
|-------------------------------|---------------------------|-------------------------------------------------------|
| `GET /api/notes`              | 200 OK                    | Dostęp do prywatnych notatek użytkownika testowego    |
| `GET /api/auth/me`            | 200 OK                    | Potwierdzenie działania uwierzytelnionej sesji        |
| `GET /api/auth/csrf-token`    | 200 OK                    | Prawidłowa odpowiedź endpointu CSRF                   |
| `GET /api/auth/login`         | 405 Method Not Allowed    | Endpoint logowania nie obsługuje metody GET           |
| `GET /api/auth/logout`        | 405 Method Not Allowed    | Endpoint wylogowania nie obsługuje metody GET         |

Odpowiedź `GET /api/notes` miała rozmiar body 313 bajtów, zgodny z wcześniejszą odpowiedzią zawierającą dwie fikcyjne notatki.

Uzyskane wyniki potwierdzają, że w badanym przebiegu Spider skutecznie korzystał z uwierzytelnionej sesji.

**ZAP-07: PASS — ograniczony uwierzytelniony Spider zakończył pracę, a chronione endpointy `/api/notes` oraz `/api/auth/me` odpowiedziały HTTP 200 podczas skanowania.**

#### Materiały dowodowe

**Zakończenie skanowania — postęp 100%, URLs Found 21, Nodes Added 9:**

![ZAP-07 — zakończony uwierzytelniony Spider](screenshots/zap-retest/zap-07-spider-completed.png)

**Żądania wykonane podczas Spidera i potwierdzenie uwierzytelnienia:**

![ZAP-07 — uwierzytelnione żądania Spidera](screenshots/zap-retest/zap-07-authenticated-messages.png)

---

### 7.3. ZAP-08 — Ograniczony fuzzing parametru zapytania

**Cel:** sprawdzenie zachowania endpointu API wobec dodatkowych, nietypowych wartości parametru zapytania.

Do testu wykorzystano istniejący endpoint:

`GET https://localhost:7038/api/notes`

Ponieważ nie potwierdzono dostępności osobnego endpointu `GET /api/notes/{id}`, zrezygnowano z pierwotnego pomysłu fuzzingu identyfikatora notatki.

Zamiast tego dodano pomocniczy parametr `probe`:

```http
GET /api/notes?probe=TEST HTTP/1.1
```

Pojedyncze żądanie kontrolne zwróciło HTTP 200 OK i dwie fikcyjne notatki.

#### Konfiguracja Fuzzera

Jako jedyną lokalizację fuzzingu wskazano cztery znaki `TEST`, stanowiące wartość parametru `probe`.

Nie modyfikowano nazwy parametru, ścieżki endpointu, tokenu JWT ani pozostałych nagłówków.

Zastosowano własną, niewielką listę typu Strings, obejmującą następujące wartości:

```text
abc
1234567890
0000000000
!@#$%^&*
../../test
' OR '1'='1
<test>
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
```

Wartości dobrano w celu sprawdzenia reakcji aplikacji na liczby, znaki specjalne, ciąg przypominający ścieżkę, fragment tekstu przypominający składnię SQL, znaczniki oraz dłuższy ciąg znaków.

Zawartość parametru `probe` nie była interpretowana jako zadeklarowany parametr biznesowy endpointu.

Ustawienia wykonania:

|               Parametr                |   Wartość     |
|---------------------------------------|--------------:|
| Concurrent Scanning Threads per Scan  | 1             |
| Delay when Fuzzing                    | 300 ms        |
| Retries on IO Error                   | 0             |
| Limit maximum errors                  | Włączone      |
| Max. Errors Allowed                   | 3             |
| Payload Replacement Strategy          | Cluster Bomb  |
| Follow Redirects                      | Wyłączone     |

Nie korzystano z dużych bibliotek payloadów ani z Active Scan. Test obejmował wyłącznie żądania GET kierowane do izolowanej instancji aplikacji.

#### Wyniki fuzzingu

|           Wskaźnik                    |                Wynik                      |
|---------------------------------------|-------------------------------------------|
| Postęp                                | 100%                                      |
| Messages Sent                         | 10                                        |
| Errors                                | 0                                         |
| Odpowiedź bazowa                      | 200 OK                                    |
| Odpowiedzi dla testowanych wartości   | Wszystkie 200 OK                          |
| Rozmiar body                          | 313 bajtów w każdej widocznej odpowiedzi  |
| HTTP 500                              | Nie zaobserwowano                         |
| Nowe alerty w tabeli Fuzzera          | Nie zaobserwowano                         |

Faktyczna liczba wysłanych wiadomości wyniosła 10. Wynik obejmował również dodatkowe pozycje z pustą wartością payloadu widoczne w tabeli Fuzzera. W dokumentacji przyjęto licznik rzeczywiście wykonanych żądań, a nie pierwotnie planowaną liczbę ośmiu.

W badanej próbie nietypowe wartości dodatkowego parametru nie spowodowały błędu HTTP 500 ani przerwania komunikacji.

**ZAP-08: PASS w ograniczonym zakresie — wszystkie wykonane żądania zakończyły się HTTP 200, bez błędów Fuzzera.**

Wynik nie stanowi potwierdzenia odporności aplikacji na SQL Injection, XSS ani błędy walidacji właściwych parametrów biznesowych. Badany parametr `probe` nie jest częścią zadeklarowanego kontraktu endpointu.

#### Materiał dowodowy

![ZAP-08 — zakończony ograniczony fuzzing](screenshots/zap-retest/zap-08-fuzzer-results.png)

---

### 7.4. Kontrola alertów po ZAP-07 i ZAP-08

Po zakończeniu obu scenariuszy sprawdzono końcową listę alertów OWASP ZAP.

W badanym zakresie nie zaobserwowano nowych kategorii alertów dotyczących izolowanej instancji `https://localhost:7038`.

Brak nowych kategorii alertów odnosi się do ruchu wygenerowanego podczas opisanych testów i nie jest równoznaczny z całkowitym brakiem podatności aplikacji.

---

### 7.5. Podsumowanie

| Scenariusz    |                       Zakres                                      |            Ocena              |
|---------------|-------------------------------------------------------------------|-------------------------------|
| ZAP-07        | Ograniczony Spider korzystający z aktywnej sesji HTTP             | PASS                          |
| ZAP-08        | Fuzzing dodatkowego parametru `probe` — 10 wiadomości, 0 błędów   | PASS w ograniczonym zakresie  |

Najważniejsze ustalenia:

1. Oddzielna instancja SecureNotes działała poprawnie na HTTPS 7038 i korzystała z dedykowanej bazy MariaDB.
2. Podczas uwierzytelnionego Spidera chronione endpointy `/api/notes` i `/api/auth/me` odpowiedziały HTTP 200.
3. Spider zakończył pracę przy ograniczonej głębokości i czasie, bez automatycznego wysyłania formularzy.
4. Ograniczony fuzzing dodatkowego parametru zapytania nie wywołał błędów serwera w badanym zestawie wartości.
5. Po obu testach nie zaobserwowano nowych kategorii alertów dla izolowanej instancji.

#### Ograniczenia i ochrona danych

Testy wykonano wyłącznie na fikcyjnym koncie i danych testowych.

Nie uruchamiano Active Scan ani pełnoskalowego fuzzingu. Nie testowano w ten sposób odporności wszystkich parametrów wejściowych aplikacji.

Do repozytorium należy dodawać wyłącznie zanonimizowane materiały dowodowe. Wartości JWT, ciasteczek, tokenów CSRF oraz surowe sesje OWASP ZAP nie są częścią dokumentacji.

Po zakończeniu testów należy zamknąć aktywną sesję ZAP i wylogować konto testowe. Ponieważ token uwierzytelniający był widoczny na roboczych screenshotach, nie należy wykorzystywać go ponownie; samo wylogowanie nie musi gwarantować natychmiastowego unieważnienia wcześniej skopiowanego JWT.

**Ocena końcowa ZAP-07–ZAP-08: PASS w zakresie wykonanych scenariuszy i udokumentowanych wyników.**

**Data wykonania:** 11.10.2026.