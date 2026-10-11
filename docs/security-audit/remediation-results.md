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

## 4. REM-02 — Konfiguracja HSTS

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

## 5. REM-03 — Ocena ograniczeñ œrodowiska Development

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

## 6. ZAP-02 — Kontrolna analiza pasywna po naprawach

### Cel

Ponowna analiza aplikacji SecureNotes przy u¿yciu OWASP ZAP 2.17.0 w celu zweryfikowania dzia³añ naprawczych po pierwotnym audycie ZAP-01.

Test wykonano w trybie Safe Mode, korzystaj¹c z Manual Explore i lokalnej aplikacji uruchomionej w œrodowisku Development pod adresem `https://localhost:7028`.

Nie wykonywano Active Scan, Spider ani fuzzingu.

### Wyniki porównawcze

| Kategoria alertu | ZAP-01 | ZAP-02 | Ocena |
|---|---|---|---|
| Cross-Domain Misconfiguration | Wystêpowa³ | Wystêpuje | Zewnêtrzna domena Edge — poza zakresem |
| Strict-Transport-Security Header Not Set | Wystêpowa³ | Wystêpuje | Lokalny Development; zgodne z konfiguracj¹ HSTS |
| X-Content-Type-Options Header Missing | 3 wyst¹pienia | 2 wyst¹pienia | W ZAP-02 wy³¹cznie zewnêtrzne domeny |
| Authentication Request Identified | Wystêpowa³ | Wystêpuje | Informacyjne rozpoznanie endpointu logowania |
| Information Disclosure – Suspicious Comments | Wystêpowa³ | Nie wystêpuje na liœcie | Nie zaobserwowano ponownie alertu dotycz¹cego Browser Refresh |
| Re-examine Cache-control Directives | Wystêpowa³ | Wystêpuje | Zweryfikowano indywidualnie odpowiedzi API |
| Session Management Response Identified | 3 wyst¹pienia | 3 wyst¹pienia | Informacyjne rozpoznanie mechanizmów sesyjnych |

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

Liczba zaobserwowanych kategorii alertów zmieni³a siê z siedmiu podczas ZAP-01 do szeœciu podczas ZAP-02. Wynik ten dotyczy zakresu ruchu przechwyconego w obu sesjach i nie stanowi dowodu wyeliminowania wszystkich mo¿liwych podatnoœci.

**Data retestu:** 11.10.2026.

---



## 3. Pozosta³e dzia³ania naprawcze

---

## 5. Pozosta³e dzia³ania

Po zakoñczeniu REM-01 i REM-02 do dalszej weryfikacji pozostaj¹:

1. Sprawdzenie dzia³ania aplikacji i nag³ówków bezpieczeñstwa bez pomocniczego mechanizmu ASP.NET Core Browser Refresh.
2. Ocena ograniczenia dotycz¹cego Swagger UI przy restrykcyjnej polityce CSP.
3. Koñcowe testy regresyjne po wszystkich zmianach.
4. Weryfikacja konfiguracji rzeczywistego œrodowiska Production przed wdro¿eniem, w tym HTTPS i HSTS.

Pierwotne wyniki audytu pozostaj¹ dostêpne w `results.md` i nie zosta³y nadpisane.