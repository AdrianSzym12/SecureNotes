# SecureNotes — Wyniki napraw i retestów bezpieczeñstwa

## 1. Cel dokumentu

Dokument przedstawia dzia³ania naprawcze podjête po zakoñczeniu audytu bezpieczeñstwa SecureNotes oraz wyniki ponownej weryfikacji zabezpieczeñ.

Pierwotne wyniki audytu znajduj¹ siê w `results.md`. Nie s¹ nadpisywane, aby zachowaæ mo¿liwoœæ porównania stanu aplikacji przed napraw¹ i po niej.

**Branch naprawczy:** `fix/security-hardening`

**Data rozpoczêcia retestów:** 11.10.2026

---

## 2. REM-01 — Cache-Control dla odpowiedzi API

### Problem wykryty podczas audytu

W ramach analizy pasywnej OWASP ZAP (ZAP-01) dodatkowo sprawdzono odpowiedŸ `GET /api/notes`.

Serwer zwróci³ HTTP 200 OK z prywatnymi notatkami zalogowanego u¿ytkownika, ale w nag³ówkach odpowiedzi nie stwierdzono obecnoœci `Cache-Control`.

Brak tego nag³ówka nie stanowi³ samodzielnie dowodu wycieku danych, lecz wskazywa³ na potrzebê jawnego okreœlenia zasad przechowywania wra¿liwych odpowiedzi przez pamiêci podrêczne.

### Zastosowana poprawka

Zmodyfikowano:

`SecureNotes.API/Middleware/SecurityHeadersMiddleware.cs`

W istniej¹cym mechanizmie `Response.OnStarting()` dodano warunek:

```csharp
if (context.Request.Path.StartsWithSegments("/api"))
{
    headers["Cache-Control"] = "no-store";
}
```

Poprawka obejmuje odpowiedzi kierowane do œcie¿ek `/api` i ich podœcie¿ek, bez wprowadzania tej samej polityki dla publicznych plików statycznych.

Pozosta³e nag³ówki bezpieczeñstwa, w tym restrykcyjna polityka CSP, zosta³y zachowane.

### Wyniki retestów rêcznych

| ID | Sprawdzony zasób | Wynik oczekiwany | Wynik rzeczywisty | Status |
|---|---|---|---|---|
| CACHE-01 | GET `/api/notes` | HTTP 200, `Cache-Control: no-store` | HTTP 200, `no-store` | PASS |
| CACHE-02 | GET `/api/auth/me` | HTTP 200, `Cache-Control: no-store` | HTTP 200, `no-store` | PASS |
| CACHE-03 | GET `/api/auth/csrf-token` | HTTP 200, `Cache-Control: no-store` | HTTP 200, `no-store` | PASS |
| CACHE-04 | GET `/css/style.css` | HTTP 200, bez nowego `Cache-Control: no-store` | HTTP 200, brak `no-store` | PASS |

### Materia³y dowodowe

**CACHE-01 — prywatne notatki:**

![GET api/notes — Cache-Control no-store](screenshots/retest/cache-01-notes-no-store.png)

**Dodatkowa kontrola dzia³ania aplikacji po wy³¹czeniu Hot Reload:**

![GET api/notes — ponowna weryfikacja](screenshots/retest/cache-02-notes-after-hot-reload-disabled.png)

**CACHE-02 — informacje o zalogowanym u¿ytkowniku:**

![GET api/auth/me — Cache-Control no-store](screenshots/retest/cache-03-auth-me-no-store.png)

**CACHE-03 — endpoint tokenu CSRF:**

![GET api/auth/csrf-token — Cache-Control no-store](screenshots/retest/cache-04-csrf-token-no-store.png)

**CACHE-04 — kontrola pliku statycznego:**

![GET css/style.css — bez dodatkowego no-store](screenshots/retest/cache-05-static-css-unaffected.png)

Materia³y dowodowe ograniczono do informacji niezbêdnych do potwierdzenia wyników. Nie publikowano wartoœci JWT, tokenów CSRF ani prywatnej treœci odpowiedzi API.

### Testy regresyjne

Po wprowadzeniu poprawki wykonano:

- Rebuild Solution — zakoñczony sukcesem.
- Test Explorer — 82/82 testy automatyczne zakoñczone sukcesem.
- Rêczn¹ kontrolê dzia³ania strony notatek — aplikacja wyœwietla³a zapisane notatki poprawnie.

### Wynik koñcowy REM-01

**PASS — poprawka potwierdzona dla wszystkich czterech sprawdzonych przypadków.**

Badane odpowiedzi API otrzymuj¹ `Cache-Control: no-store`, natomiast sprawdzony publiczny plik CSS nie zosta³ objêty now¹ polityk¹.

---

## 3. REM-02 — Konfiguracja HSTS

### Problem wykryty podczas audytu

Podczas analizy pasywnej OWASP ZAP (ZAP-01) wykryto brak nag³ówka `Strict-Transport-Security` w odpowiedziach lokalnej aplikacji uruchomionej w œrodowisku Development.

### Zastosowana poprawka

W pliku `SecureNotes.API/Program.cs` zarejestrowano politykê HSTS:

```csharp
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(30);
    options.IncludeSubDomains = false;
    options.Preload = false;
});
```

Middleware HSTS w³¹czono warunkowo poza œrodowiskiem Development, przed `UseHttpsRedirection()`:

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
```

Nie zmieniano istniej¹cych profili lokalnego uruchamiania ani polityki CSP.

### Weryfikacja

|   ID    |             Sprawdzany warunek          |          Wynik                            | Status |
|---------|-----------------------------------------|-------------------------------------------|--------|
| HSTS-01 | Izolowany potok Production + HTTPS      | `max-age=2592000`                         | PASS   |
| HSTS-02 | Izolowany potok Development + HTTPS     | Brak nag³ówka HSTS                        | PASS   |
| HSTS-03 | Izolowana polityka Production           | Brak `includeSubDomains` oraz `preload`   | PASS   |

Zweryfikowano równie¿ zawartoœæ rzeczywistego `Program.cs` oraz konfiguracjê `launchSettings.json`.

Testy HSTS wykorzystuj¹ izolowany potok HTTP odtwarzaj¹cy konfiguracjê aplikacji. Nie uruchamiaj¹ rzeczywistego `Program.Main()` ani nie korzystaj¹ z MariaDB.

### Ograniczenia

Nie przeprowadzono testu rzeczywistego wdro¿enia Production.

Przed publikacj¹ aplikacji nale¿y sprawdziæ dzia³anie HSTS na docelowej domenie HTTPS, poprawnoœæ certyfikatu oraz przekierowania HTTP do HTTPS.

Brak HSTS podczas lokalnego uruchomienia w œrodowisku Development jest zgodny z przyjêt¹ konfiguracj¹.

### Testy regresyjne

Po zastosowaniu poprawki wykonano:

-Rebuild Solution — zakoñczony sukcesem.

-Run All Tests — 85/85 testów zakoñczonych sukcesem.

-Nie stwierdzono niepowodzeñ w istniej¹cych testach automatycznych.

**Wynik:** 85/85 PASS.

**Dowód:** `screenshots/retest/hsts-01-automated-tests.png`

![REM-02 — testy automatyczne po konfiguracji HSTS](screenshots/retest/hsts-01-automated-tests.png)

### Wynik koñcowy REM-02

PASS — implementacja i izolowane testy konfiguracji HSTS zakoñczone sukcesem.

Weryfikacja rzeczywistego wdro¿enia produkcyjnego pozostaje zadaniem przedwdro¿eniowym.

Data retestu: 11.10.2026.

---

## 4. REM-03 — Ocena ograniczeñ œrodowiska Development

### ASP.NET Core Browser Refresh

W ramach kontroli œrodowiska Development wy³¹czono Hot Reload w Visual Studio 2022.

Po ponownym uruchomieniu aplikacji potwierdzono, ¿e pomocniczy mechanizm Browser Refresh nie podejmuje ju¿ obserwowanych wczeœniej prób po³¹czenia WebSocket z portem 44341.

Aplikacja SecureNotes nadal dzia³a³a prawid³owo, a odpowiedŸ GET `/api/notes` zwraca³a HTTP 200 oraz `Cache-Control: no-store`.

Wy³¹czenie Hot Reload jest lokalnym ustawieniem œrodowiska programistycznego, a nie zmian¹ zabezpieczeñ aplikacji.

### Swagger UI i Content Security Policy

W trakcie audytu stwierdzono ograniczenie dzia³ania interfejsu Swagger UI wynikaj¹ce z restrykcyjnej polityki CSP.

Podjêto decyzjê o pozostawieniu globalnej polityki CSP bez zmian. Nie dodawano dyrektyw `unsafe-inline` ani wyj¹tków os³abiaj¹cych zabezpieczenia w³aœciwej aplikacji.

Swagger UI pozostaje dostêpny konfiguracyjnie wy³¹cznie w œrodowisku Development. Do przegl¹dania dokumentacji API mo¿na równie¿ wykorzystaæ endpoint `/swagger/v1/swagger.json`.

Ewentualne tymczasowe wy³¹czenie CSP na potrzeby lokalnego debugowania nie stanowi konfiguracji docelowej i nie jest uwzglêdniane jako dowód zaliczenia testów bezpieczeñstwa.

### Ocena REM-03

**ZAMKNIÊTE — œwiadomie zaakceptowano ograniczenie narzêdzia deweloperskiego.**

Nie stwierdzono potrzeby modyfikowania globalnej polityki CSP. Nie wprowadzono zmian w kodzie aplikacji w ramach REM-03.

---

## 5. ZAP-02 — Kontrolna analiza pasywna po naprawach

### Cel

Ponowna analiza aplikacji SecureNotes przy u¿yciu OWASP ZAP 2.17.0 w celu zweryfikowania dzia³añ naprawczych po pierwotnym audycie ZAP-01.

Test wykonano w trybie Safe Mode, korzystaj¹c z Manual Explore i lokalnej aplikacji uruchomionej w œrodowisku Development pod adresem `https://localhost:7028`.

Nie wykonywano Active Scan, Spider ani fuzzingu.

### Wyniki porównawcze

|               Kategoria alertu                |   ZAP-01      |       ZAP-02              |                       Ocena                                   |
|-----------------------------------------------|---------------|---------------------------|---------------------------------------------------------------|
| Cross-Domain Misconfiguration                 | Wystêpowa³    | Wystêpuje                 | Zewnêtrzna domena Edge — poza zakresem                        |
| Strict-Transport-Security Header Not Set      | Wystêpowa³    | Wystêpuje                 | Lokalny Development; zgodne z konfiguracj¹ HSTS               |
| X-Content-Type-Options Header Missing         | 3 wyst¹pienia | 2 wyst¹pienia             | W ZAP-02 wy³¹cznie zewnêtrzne domeny                          |
| Authentication Request Identified             | Wystêpowa³    | Wystêpuje                 | Informacyjne rozpoznanie endpointu logowania                  |
| Information Disclosure – Suspicious Comments  | Wystêpowa³    | Nie wystêpuje na liœcie   | Nie zaobserwowano ponownie alertu dotycz¹cego Browser Refresh |
| Re-examine Cache-control Directives           | Wystêpowa³    | Wystêpuje                 | Zweryfikowano indywidualnie odpowiedzi API                    |
| Session Management Response Identified        | 3 wyst¹pienia | 3 wyst¹pienia             | Informacyjne rozpoznanie mechanizmów sesyjnych                |

### Weryfikacja Cache-Control

ZAP-02 przechwyci³ odpowiedŸ:

`GET https://localhost:7028/api/notes`

Potwierdzono:
- HTTP 200 OK;
- `Cache-Control: no-store`;
- obecnoœæ pozosta³ych nag³ówków bezpieczeñstwa, w tym CSP i `X-Content-Type-Options: nosniff`.

Endpoint `/api/notes` nie znajdowa³ siê na liœcie wyst¹pieñ alertu `Re-examine Cache-control Directives (Systemic)` w analizowanej sesji.

Dodatkowo szczegó³y alertu przypisanego do `/api/auth/me` zawiera³y dowód `no-store`. Obecnoœæ ogólnej regu³y informacyjnej nie oznacza³a wiêc braku zastosowanej poprawki.

### Pozosta³e ustalenia

Dwa wyst¹pienia `X-Content-Type-Options Header Missing` w ZAP-02 dotyczy³y wy³¹cznie zewnêtrznych domen `edge-consumer-static.azureedge.net` oraz `xpaywalletcdn-prod.azureedge.net`.

Alert CORS równie¿ dotyczy³ zewnêtrznego zasobu Edge.

HSTS nie by³ wysy³any przez lokalne œrodowisko Development, zgodnie z przyjêt¹ konfiguracj¹. Test rzeczywistego wdro¿enia Production pozostaje wymaganiem przedwdro¿eniowym.

Alerty dotycz¹ce rozpoznania ¿¹dania logowania i mechanizmów sesyjnych zachowa³y charakter informacyjny.

### Materia³y dowodowe

- `screenshots/zap-retest/zap-02-cache-notes-no-store.png`
- `screenshots/zap-retest/zap-02-cache-auth-me-alert.png`
- `screenshots/zap-retest/zap-02-nosniff-external-only.png`
- `screenshots/zap-retest/zap-02-hsts-development.png`
- `screenshots/zap-retest/zap-02-cors-external-domain.png`
- `screenshots/zap-retest/zap-02-auth-session-info.png`

Wszystkie materia³y przeznaczone do repozytorium powinny byæ pozbawione wartoœci JWT, ciasteczek, tokenów CSRF oraz prywatnej treœci notatek.

### Wynik koñcowy

**ZAP-02: PASS z udokumentowanymi ustaleniami.**

Kontrolna analiza pasywna potwierdzi³a skutecznoœæ poprawki Cache-Control dla badanej odpowiedzi zawieraj¹cej prywatne notatki.

Liczba zaobserwowanych kategorii alertów zmieni³a siê z siedmiu podczas ZAP-01 do szeœciu podczas ZAP-02. Wynik ten dotyczy zakresu ruchu przechwyconego w obu sesjach i 
    nie stanowi dowodu wyeliminowania wszystkich mo¿liwych podatnoœci.

**Data retestu:** 11.10.2026.

---

## 6. Rozszerzone testy bezpieczeñstwa HTTP — ZAP-03–ZAP-06

### 6.1. Cel i zakres

Po zakoñczeniu kontrolnej analizy pasywnej ZAP-02 przeprowadzono dodatkowe testy bezpieczeñstwa aplikacji SecureNotes przy u¿yciu OWASP ZAP 2.17.0.

Zakres rozszerzono o:
- kontrolê odpowiedzi na metodê OPTIONS i nag³ówków CORS;
- obs³ugê b³êdów HTTP oraz zachowanie nag³ówków bezpieczeñstwa;
- kontrolê dostêpnoœci wybranych plików konfiguracyjnych i zasobów diagnostycznych;
- ograniczony Spider s³u¿¹cy do automatycznego odkrywania publicznych zasobów aplikacji.

Rêczne ¿¹dania wykonywano przez ZAP Requester w trybie Protected Mode. Zakres ograniczono do lokalnej aplikacji `https://localhost:7028`.

Nie uruchamiano Active Scan ani fuzzingu. Nie wykonywano ponownie pe³nego zestawu wczeœniejszych scenariuszy AUTH, IDOR, CSRF i XSS.

### 6.2. ZAP-03 — Kontrola OPTIONS i CORS

Wys³ano ¿¹danie OPTIONS do `/api/notes`, zawieraj¹ce nag³ówki:

```http
Origin: https://example.org
Access-Control-Request-Method: GET
```

Nie do³¹czano JWT, ciasteczek ani innych danych uwierzytelniaj¹cych.

**Uzyskany wynik:**

|            Element                |       Wartoœæ             |
|-----------------------------------|---------------------------|
| Status HTTP                       | 405 Method Not Allowed    |
| Allow                             | GET, POST                 |
| Access-Control-Allow-Origin       | Brak                      |
| Access-Control-Allow-Credentials  | Brak                      |
| Cache-Control                     | no-store                  |
| Pozosta³e nag³ówki bezpieczeñstwa | Obecne                    |

W badanym przypadku aplikacja nie zezwoli³a na udostêpnienie odpowiedzi obcemu pochodzeniu za pomoc¹ nag³ówków CORS. Sama odpowiedŸ 405 nie stanowi podatnoœci.

**ZAP-03: PASS — nie wykazano niepo¿¹danej konfiguracji CORS w badanym przypadku.**

![ZAP-03 — kontrola OPTIONS i CORS](screenshots/zap-retest/zap-03-options-notes.png)

### 6.3. ZAP-04 — Obs³uga b³êdów HTTP

Wykonano seriê rêcznych ¿¹dañ sprawdzaj¹cych odpowiedzi b³êdów, obecnoœæ nag³ówków bezpieczeñstwa oraz mo¿liwoœæ przypadkowego ujawnienia danych technicznych.

|   ID    |             ¯¹danie                     |    Wynik HTTP |                        Rezultat                                                     |
|---------|-----------------------------------------|---------------|-------------------------------------------------------------------------------------|
| ZAP-04A | GET `/api/nonexistent-security-check`   | 404           | Pusta odpowiedŸ; nag³ówki bezpieczeñstwa obecne — PASS                              |
| ZAP-04B | GET `/api/notes` bez uwierzytelnienia   | 401           | Brak danych prywatnych; `WWW-Authenticate: Bearer` — PASS                           |
| ZAP-04C | GET `/api/notes/invalid-id`             | 404           | Brak ujawnienia szczegó³ów technicznych — PASS w zakresie obs³ugi b³êdnego adresu   |
| ZAP-04D | POST `/api/notes` bez uwierzytelnienia  | 401           | ¯¹danie odrzucone; brak ujawnienia danych — PASS w zakresie odpowiedzi b³êdu        |
| ZAP-04E | GET `/nonexistent-security-check.html`  | 404           | Pusta odpowiedŸ; nag³ówki bezpieczeñstwa obecne — PASS                              |

W odpowiedziach API potwierdzono `Cache-Control: no-store`. Zachowane zosta³y równie¿ restrykcyjna polityka CSP, `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY` oraz pozosta³e skonfigurowane nag³ówki.

W odpowiedzi 404 dla nieistniej¹cej strony HTML nie stwierdzono `Cache-Control: no-store`, co jest zgodne z zakresem poprawki REM-01 obejmuj¹cej œcie¿ki `/api`.

**Ograniczenie interpretacyjne:** ¿¹danie ZAP-04C nie stanowi³o pe³nego testu walidacji identyfikatora, poniewa¿ odpowiedŸ 404 mog³a wynikaæ z niedopasowania trasy. W ZAP-04D serwer odrzuci³ nieuwierzytelnione ¿¹danie, a zamierzona niepoprawna treœæ JSON nie zosta³a przekazana jako w³aœciwe body. Nie oznaczono wiêc tego przypadku jako zaliczonego testu parsera JSON.

**ZAP-04: PASS w zakresie wykonanych kontroli obs³ugi odpowiedzi b³êdów.** Nie stwierdzono ujawnienia prywatnych notatek, stosów wyj¹tków ani œcie¿ek systemowych w badanych odpowiedziach.

**Materia³y dowodowe:**

![ZAP-04A — nieistniej¹cy endpoint API](screenshots/zap-retest/zap-04a-api-404.png)

![ZAP-04B — nieuwierzytelniony dostêp](screenshots/zap-retest/zap-04b-api-401.png)

![ZAP-04C — nieprawid³owy adres notatki](screenshots/zap-retest/zap-04c-invalid-note-id.png)

![ZAP-04D — odrzucone ¿¹danie POST](screenshots/zap-retest/zap-04d-post-unauthorized.png)

![ZAP-04E — nieistniej¹ca strona HTML](screenshots/zap-retest/zap-04e-html-404.png)

### 6.4. ZAP-05 — Kontrola udostêpniania zasobów

Sprawdzono dostêpnoœæ wybranych plików konfiguracyjnych, zasobu repozytorium Git, dokumentacji OpenAPI oraz przyk³adowego endpointu diagnostycznego.

¯¹dania wykonywano metod¹ GET bez uwierzytelnienia.

|   ID    |         Badany zasób            | Wynik HTTP |                          Ocena                                          |
|---------|---------------------------------|------------|-------------------------------------------------------------------------|
| ZAP-05A | `/.env`                         | 404        | Plik nie zosta³ udostêpniony — PASS                                     |
| ZAP-05B | `/appsettings.json`             | 404        | Konfiguracja nie zosta³a udostêpniona — PASS                            |
| ZAP-05C | `/appsettings.Development.json` | 404        | Konfiguracja Development nie zosta³a udostêpniona — PASS                |
| ZAP-05D | `/.git/config`                  | 404        | Konfiguracja Git nie zosta³a udostêpniona — PASS                        |
| ZAP-05E | `/swagger/v1/swagger.json`      | 200        | Dokumentacja OpenAPI dostêpna zgodnie z konfiguracj¹ Development        |
| ZAP-05F | `/health`                       | 404        | Nie stwierdzono udostêpnienia badanego endpointu diagnostycznego — PASS |

W badanych odpowiedziach 404 nie stwierdzono ujawnienia zawartoœci plików ani szczegó³ów technicznych. Nag³ówki bezpieczeñstwa pozosta³y aktywne.

Endpoint `/swagger/v1/swagger.json` zwróci³ dokument OpenAPI 3.0.1 z typem `application/json; charset=utf-8`. Potwierdzono obecnoœæ nag³ówków bezpieczeñstwa. Widoczny fragment dokumentu zawiera³ standardowe informacje o strukturze API; nie przeprowadzano pe³nego przegl¹du wszystkich wartoœci dokumentu pod k¹tem sekretów.

Dostêpnoœæ OpenAPI JSON w Development potwierdza mo¿liwoœæ korzystania z dokumentacji API bez os³abiania globalnej polityki CSP na potrzeby interfejsu Swagger UI.

**ZAP-05: 6/6 przypadków zgodnych z oczekiwaniami.**

**Materia³y dowodowe:**

![ZAP-05A — kontrola pliku .env](screenshots/zap-retest/zap-05a-env-not-exposed.png)

![ZAP-05B — kontrola appsettings.json](screenshots/zap-retest/zap-05b-appsettings-not-exposed.png)

![ZAP-05C — kontrola konfiguracji Development](screenshots/zap-retest/zap-05c-development-config-not-exposed.png)

![ZAP-05D — kontrola konfiguracji Git](screenshots/zap-retest/zap-05d-git-config-not-exposed.png)

![ZAP-05E — dokumentacja OpenAPI](screenshots/zap-retest/zap-05e-openapi-development.png)

![ZAP-05F — kontrola endpointu health](screenshots/zap-retest/zap-05f-health-not-exposed.png)

### 6.5. ZAP-06 — Ograniczony Spider

Przeprowadzono automatyczne odkrywanie zasobów aplikacji za pomoc¹ modu³u Spider w OWASP ZAP 2.17.0.

Celem testu by³o sprawdzenie publicznej struktury aplikacji SecureNotes oraz zweryfikowanie, czy ruch wygenerowany przez Spider spowoduje pojawienie siê nowych alertów analizy pasywnej.

**Konfiguracja skanowania:**

|   Parametr                    |           Ustawienie                                  |
|-------------------------------|-------------------------------------------------------|
| Tryb ZAP                      | Protected Mode                                        |
| Punkt pocz¹tkowy              | `https://localhost:7028`                              |
| Kontekst                      | Domyœlny kontekst, ograniczony do lokalnej aplikacji  |
| U¿ytkownik uwierzytelniony    | Brak                                                  |
| Maximum Depth to Crawl        | 3                                                     |
| Maximum Children to Crawl     | 10                                                    |
| Maximum Duration              | 2 minuty                                              |
| Maximum Parse Size            | 2621440 bajtów                                        |
| Accept Cookies                | Wy³¹czone                                             |
| Process Forms                 | Wy³¹czone                                             |
| POST Forms                    | Wy³¹czone                                             |
| Parse HTML Comments           | W³¹czone                                              |
| Parse robots.txt              | W³¹czone                                              |
| Parse sitemap.xml             | W³¹czone                                              |
| Parse SVN Metadata            | Wy³¹czone                                             |
| Parse Git Metadata            | Wy³¹czone                                             |
| Parse .DS_Store Files         | Wy³¹czone                                             |
| Handle OData Parameters       | Wy³¹czone                                             |
| Logout Avoidance              | Wy³¹czone                                             |   
Zastosowane ograniczenia mia³y umo¿liwiæ przeszukiwanie publicznej czêœci SecureNotes bez automatycznego przetwarzania formularzy i wykonywania operacji modyfikuj¹cych notatki.

#### Wynik Spidera

|       WskaŸnik                | Wartoœæ |
|-------------------------------|---------|
| Postêp                        | 100%    |
| URLs Found                    | 15      |
| Nodes Added                   | 3       |
| Aktualnie skanowane adresy    | 0       |

Wszystkie 15 adresów przedstawionych na liœcie mia³o oznaczenie `Seed`, dlatego nie interpretowano tego wyniku jako odkrycia 15 nowych zasobów.

W zak³adce **Added Nodes** widoczne by³y trzy wêz³y:

- `/api/auth`;
- `/api/auth/login`;
- `/swagger/v1`.

Wszystkie nale¿a³y do lokalnej aplikacji i odpowiada³y znanym obszarom SecureNotes. Nie zaobserwowano nowej, nieznanej œcie¿ki wymagaj¹cej natychmiastowej poprawki.

#### Kontrola alertów po zakoñczeniu Spidera

Po zakoñczeniu skanowania sprawdzono listê alertów analizy pasywnej.

Nadal wystêpowa³o szeœæ kategorii:

|               Kategoria                               |                                   Wynik analizy                                                   |
|-------------------------------------------------------|---------------------------------------------------------------------------------------------------|
| Cross-Domain Misconfiguration                         | Zg³oszenie dotycz¹ce zewnêtrznego zasobu Edge                                                     |
| Strict-Transport-Security Header Not Set (Systemic)   | Lokalny Development oraz zasób zewnêtrzny                                                         |
| X-Content-Type-Options Header Missing                 | Dwa wyst¹pienia dotycz¹ce domen zewnêtrznych                                                      |
| Authentication Request Identified                     | Informacyjne rozpoznanie endpointu logowania                                                      |
| Re-examine Cache-control Directives (Systemic)        | Wczeœniej przeanalizowany alert informacyjny; potwierdzono `no-store` dla badanych odpowiedzi API |
| Session Management Response Identified                | Informacyjne rozpoznanie mechanizmów sesyjnych                                                    |

Nie zaobserwowano nowej kategorii alertów wzglêdem ZAP-02.

Nie oznacza to wyeliminowania wszystkich mo¿liwych podatnoœci — wynik odnosi siê do ograniczonego zakresu przeszukiwania oraz ruchu przeanalizowanego przez skaner pasywny.

**ZAP-06: PASS — ograniczony Spider zakoñczy³ dzia³anie, a w badanym zakresie nie wykryto nowych kategorii alertów.**

#### Materia³y dowodowe

**Zakoñczony Spider — postêp 100%:**

![ZAP-06 — zakoñczone skanowanie Spider](screenshots/zap-retest/zap-06-spider-completed.png)

**Trzy wêz³y dodane do drzewa aplikacji:**

![ZAP-06 — Added Nodes](screenshots/zap-retest/zap-06-spider-added-nodes.png)

**Koñcowa lista alertów analizy pasywnej:**

![ZAP-06 — lista alertów po zakoñczeniu Spidera](screenshots/zap-retest/zap-06-final-alerts.png)

---

### 7.6. Podsumowanie rozszerzonej serii testów

W ramach dodatkowej weryfikacji po dzia³aniach naprawczych wykonano cztery scenariusze ZAP-03–ZAP-06.

| Scenariusz |                  Zakres                                  |                   Wynik koñcowy                       |
|------------|----------------------------------------------------------|-------------------------------------------------------|
| ZAP-03     | Metoda OPTIONS i nag³ówki CORS                           | PASS                                                  |
| ZAP-04     | Obs³uga b³êdów HTTP i kontrola nag³ówków bezpieczeñstwa  | PASS w zakresie wykonanych przypadków                 |
| ZAP-05     | Dostêpnoœæ wybranych plików i zasobów diagnostycznych    | 6/6 wyników zgodnych z oczekiwaniami                  |
| ZAP-06     | Ograniczony Spider i kontrola alertów pasywnych          | PASS — skan zakoñczony, bez nowych kategorii alertów  |

#### Najwa¿niejsze wnioski

1. W badanym ¿¹daniu OPTIONS nie stwierdzono niepo¿¹danego udostêpnienia endpointu `/api/notes` obcemu pochodzeniu za pomoc¹ nag³ówków CORS.
2. Odpowiedzi b³êdów HTTP zachowa³y skonfigurowane nag³ówki bezpieczeñstwa. W badanych przypadkach nie stwierdzono ujawnienia prywatnych notatek, stosów wyj¹tków ani wewnêtrznych œcie¿ek systemowych.
3. Wybrane pliki konfiguracyjne i zasoby repozytorium nie by³y publicznie udostêpniane przez HTTP.
4. Dokumentacja OpenAPI JSON by³a dostêpna zgodnie z konfiguracj¹ œrodowiska Development, bez koniecznoœci os³abiania globalnej polityki CSP.
5. Ograniczony Spider zakoñczy³ dzia³anie i nie spowodowa³ pojawienia siê nowych kategorii alertów pasywnych wzglêdem ZAP-02.
6. Nie zidentyfikowano nowego problemu wymagaj¹cego natychmiastowej modyfikacji kodu SecureNotes.

#### Ograniczenia testów

Wyniki dotycz¹ wy³¹cznie sprawdzonych endpointów, wybranych œcie¿ek i skonfigurowanego zakresu Spidera.

Spider nie korzysta³ z uwierzytelnionej sesji u¿ytkownika. Wy³¹czono przyjmowanie ciasteczek oraz automatyczne przetwarzanie i wysy³anie formularzy.

Nie przeprowadzano Active Scan ani fuzzingu.

Zakoñczenie rozszerzonej serii testów nie jest równoznaczne z potwierdzeniem ca³kowitej odpornoœci aplikacji na wszystkie mo¿liwe zagro¿enia.

**Ocena koñcowa ZAP-03–ZAP-06: PASS w zakresie wykonanych testów i udokumentowanych wyników.**

**Data wykonania:** 11.10.2026.

