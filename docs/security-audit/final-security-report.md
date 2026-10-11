# SecureNotes — końcowy raport audytu bezpieczeństwa

**Data audytu i retestów:** 11.10.2026  
**Projekt:** SecureNotes  
**Zakres:** lokalna aplikacja webowa i API, badanie funkcjonalnych zabezpieczeń oraz wybranych aspektów konfiguracji HTTP  
**Etapy w repozytorium:** `test/security-audit` (audyt pierwotny), `fix/security-hardening` (poprawki i retesty)  
**Status:** zakończono zaplanowane scenariusze opisane w dokumentacji; wymagania przedwdrożeniowe wskazano osobno

## 1. Streszczenie wykonawcze

Przeprowadzono audyt zabezpieczeń aplikacji SecureNotes, obejmujący uwierzytelnianie, kontrolę dostępu do notatek, ochronę CSRF, pojedynczy scenariusz Stored XSS, nagłówki HTTP, ustawienia ciasteczka uwierzytelniającego oraz analizę pasywną OWASP ZAP. Wszystkie 16 pierwotnie zaplanowanych scenariuszy uzyskało ocenę PASS; w przypadku ZAP-01 ocena ta oznacza zakończenie analizy, w której wykryto i sklasyfikowano siedem kategorii alertów, a nie brak ustaleń.

Najważniejszym zidentyfikowanym problemem wymagającym zmiany kodu był brak jawnego `Cache-Control` w odpowiedzi `GET /api/notes`, zawierającej prywatne dane użytkownika. Dodano `Cache-Control: no-store` dla odpowiedzi API i potwierdzono poprawkę w retestach. Skonfigurowano ponadto HSTS dla środowisk innych niż Development, z polityką `max-age=2592000`, bez `includeSubDomains` i `preload`. Trzy testy izolowanego potoku HSTS zakończyły się sukcesem. Ostateczny wynik testów automatycznych po zmianach wyniósł 85/85 PASS.

Rozszerzona weryfikacja ZAP-02–ZAP-08 objęła ponowną analizę pasywną, test OPTIONS/CORS, odpowiedzi błędów, sprawdzenie wybranych zasobów, nieautoryzowanego i uwierzytelnionego Spidera oraz niewielki fuzzing dodatkowego parametru zapytania. W badanych zakresach nie zaobserwowano nowej kategorii alertów związanych z izolowaną instancją końcowych testów.

**Ocena końcowa:** badane zabezpieczenia działały zgodnie z oczekiwaniami, a wskazane działania naprawcze zostały potwierdzone w opisanym zakresie. Wyniki nie stanowią gwarancji całkowitej odporności aplikacji. Przed rzeczywistym wdrożeniem należy zweryfikować konfigurację HTTPS i HSTS na docelowym środowisku.

## 2. Przedmiot, zakres i środowisko audytu

SecureNotes to aplikacja do zarządzania prywatnymi notatkami, oparta na ASP.NET Core (.NET 8) i MariaDB. Badanie dotyczyło lokalnie uruchomionej aplikacji, jej API, mechanizmów sesji JWT w ciasteczku `SecureNotes.Auth`, zabezpieczeń anty-CSRF oraz wybranych odpowiedzi HTTP. Testy odczytu i modyfikacji cudzych notatek wykonywano na dwóch odrębnych fikcyjnych kontach A i B.

|		Element				|		Audyt pierwotny i część retestów					|				Rozszerzone testy ZAP-07–08						|
|---------------------------|-----------------------------------------------------------|---------------------------------------------------------------|
| Adres						| `https://localhost:7028`									| `https://localhost:7038`										|
| Środowisko ASP.NET Core	| Development | Development									|																|
| Baza						| Lokalna baza projektu										| Oddzielna `securenotes_zap_db`								|
| Dostęp do bazy			| Dotychczasowa konfiguracja aplikacji						| `securenotes_zap_user`, uprawnienia ograniczone do bazy ZAP	|
| Dane						| Konta i notatki testowe									| Oddzielne fikcyjne konto i dwie notatki						|
| Narzędzia					| Opera GX DevTools, Visual Studio 2022, OWASP ZAP 2.17.0	| OWASP ZAP 2.17.0, phpMyAdmin, terminal PowerShell				|

Prace realizowano w gałęziach `test/security-audit` i `fix/security-hardening`. Audyt nie obejmował rzeczywistego wdrożenia produkcyjnego, zewnętrznych domen przeglądarki ani pełnego testu penetracyjnego wszystkich wejść aplikacji.

## 3. Metodyka i zasady dowodowe

Dla każdego scenariusza opisano cel, warunki początkowe, wykonane czynności, oczekiwany i rzeczywisty rezultat, ocenę i dowód. Zastosowano testy ręczne w przeglądarce i ZAP, testy automatyczne .NET oraz pasywną analizę ruchu. W kontrolowanych rozszerzeniach użyto Spidera i ograniczonego Fuzzera; nie uruchamiano Active Scan.

Ocena **PASS** oznacza zgodność obserwacji z oczekiwaniem w danym scenariuszu. Nie jest równoważna z dowodem braku innych podatności. Alerty dotyczące zewnętrznych zasobów Microsoft Edge sklasyfikowano jako poza zakresem aplikacji, a informacyjne rozpoznanie logowania czy sesji nie zostało utożsamione z wyciekiem danych. Materiały dowodowe przeznaczone do repozytorium nie powinny zawierać wartości JWT, ciasteczek, tokenów CSRF, haseł, sekretów ani prywatnej treści notatek.

## 4. Wyniki audytu pierwotnego

### 4.1. Zestawienie scenariuszy

| Obszar | Scenariusze | Wynik i najważniejsza obserwacja |
|---|---|---|
| Uwierzytelnianie i wylogowanie		| AUTH-01–04			| 4/4 PASS; dostęp bez sesji i po standardowym wylogowaniu: 401, po logowaniu: 200														|
| Kontrola właściciela / IDOR			| IDOR-01–03			| 3/3 PASS; odczyt, aktualizacja i usunięcie notatki A przez B: 404; dane A pozostały nienaruszone										|
| CSRF									| CSRF-01–03			| 3/3 PASS; brak lub zły token: 400 bez utworzenia notatki; poprawny token: 201															|
| Stored XSS							| XSS-01				| 1/1 PASS; zapisany testowy HTML/JavaScript wyświetlono dosłownie jako tekst, bez wykonania skryptu i bez dodatkowych elementów DOM	|
| Nagłówki								| HEAD-01–03			| 3/3 PASS; potwierdzono restrykcyjną CSP, `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`									|
| Ciasteczko uwierzytelniające			| COOKIE-01				| 1/1 PASS; `SecureNotes.Auth`: `HttpOnly`, `Secure`, `SameSite=Strict`																	|
| Pasywna analiza OWASP ZAP				| ZAP-01				| 1/1 PASS z ustaleniami; siedem kategorii alertów przeanalizowano i sklasyfikowano														|
| **Razem**								| **16 scenariuszy**	| **16/16 PASS, w tym ZAP-01 z ustaleniami**																							|

AUTH-04 dotyczył standardowego wylogowania przez interfejs, nie osobnego unieważniania wcześniej skopiowanego tokenu JWT. 
Test XSS obejmował konkretną treść notatki i nie rozstrzyga o wszystkich możliwych odmianach XSS. 
Testy nagłówków potwierdziły ich obecność i wartości na wybranych odpowiedziach, nie zaś każdy możliwy wariant działania przeglądarki.

### 4.2. Ustalenia ZAP-01

W Safe Mode wykonano ręczną eksplorację przez proxy, a po zakończeniu kolejka analizy pasywnej wynosiła zero. Stwierdzono siedem kategorii:

|					Kategoria					|									Klasyfikacja w kontekście SecureNotes												|
|-----------------------------------------------|-----------------------------------------------------------------------------------------------------------------------|
| Cross-Domain Misconfiguration					| Zewnętrzny zasób Edge; poza zakresem																					|
| Strict-Transport-Security Header Not Set		| Brak HSTS na lokalnym Development; temat konfiguracji przedwdrożeniowej												|
| X-Content-Type-Options Header Missing			| Jedno wystąpienie lokalnego pomocniczego `aspnetcore-browser-refresh.js`, dwa zewnętrzne								|
| Authentication Request Identified				| Informacyjne rozpoznanie `POST /api/auth/login`																		|
| Information Disclosure – Suspicious Comments	| Komentarz pomocniczego skryptu Browser Refresh; brak wykazanego sekretu												|
| Re-examine Cache-control Directives			| Potrzeba przeglądu buforowania; dodatkowe sprawdzenie wykazało brak `Cache-Control` dla prywatnego `GET /api/notes`	|
| Session Management Response Identified		| Informacyjne rozpoznanie logowania, tokenu CSRF i ciasteczek															|

Brak `Cache-Control` nie stanowił samodzielnego dowodu wycieku danych, ale uzasadniał wprowadzenie jawnej polityki dla API. Obserwacje ze środowiska Development i zewnętrznych domen oddzielono od problemów własnego kodu aplikacji.

## 5. Działania naprawcze

### REM-01 — polityka Cache-Control dla API

W `SecureNotes.API/Middleware/SecurityHeadersMiddleware.cs` dodano w `Response.OnStarting()` ustawienie `Cache-Control: no-store` dla ścieżek zaczynających się od `/api`. Ręcznie potwierdzono wynik na `GET /api/notes`, `GET /api/auth/me` i `GET /api/auth/csrf-token`; sprawdzony publiczny plik `/css/style.css` nie otrzymał nowej polityki `no-store`. Wynik: **4/4 PASS**. Po tej poprawce wykonano również 82/82 testy automatyczne.

### REM-02 — konfiguracja HSTS

W `SecureNotes.API/Program.cs` dodano `AddHsts` z okresem 30 dni (`max-age=2592000`), bez `includeSubDomains` i `preload`. Middleware `UseHsts()` wykonuje się poza Development, przed `UseHttpsRedirection()`. Trzy testy odtworzonego, izolowanego potoku potwierdziły odpowiednio nagłówek w Production/HTTPS, jego brak w Development/HTTPS i zakres dyrektyw. **3/3 PASS; końcowa regresja: 85/85 PASS.** Testy te nie uruchamiały rzeczywistego `Program.Main()` i nie zastępują kontroli rzeczywistego wdrożenia.

### REM-03 — ograniczenia Development i CSP

Wyłączono lokalny Hot Reload, eliminując obserwowane próby pomocniczego połączenia Browser Refresh. Zachowano globalną restrykcyjną CSP, mimo że ogranicza ona działanie interfejsu Swagger UI. Dokumentacja JSON pod `/swagger/v1/swagger.json` pozostała dostępna w Development. W ramach REM-03 nie osłabiano zabezpieczeń ani nie zmieniano kodu aplikacji.

## 6. Wyniki retestów i rozszerzonej weryfikacji

|	ID	 |						Zakres										|															Wynik															|
|--------|------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------|
| ZAP-02 | Kontrolna analiza pasywna po poprawkach							| PASS z ustaleniami; sześć kategorii alertów wobec siedmiu w ZAP-01; `GET /api/notes` zwrócił `Cache-Control: no-store`	|
| ZAP-03 | OPTIONS `/api/notes` z obcym `Origin`							| 405, `Allow: GET, POST`, bez ACAO i ACAC; PASS dla badanego przypadku														|
| ZAP-04 | Pięć kontroli odpowiedzi błędów API i HTML						| PASS w wykonanym zakresie; 401/404 z oczekiwanymi nagłówkami, bez wykazanego ujawnienia danych technicznych				|
| ZAP-05 | Sześć wybranych zasobów											| 6/6 zgodnych z oczekiwaniami; `.env`, `appsettings*`, `.git/config`, `/health`: 404; OpenAPI JSON: 200 w Development		|
| ZAP-06 | Ograniczony Spider bez uwierzytelnienia							| 100%, 15 pozycji `Seed`, 3 dodane węzły; brak nowej kategorii alertów														|
| ZAP-07 | Ograniczony Spider z aktywną sesją HTTP							| 100%, 21 pozycji `Seed`, 9 dodanych węzłów; podczas skanowania `GET /api/notes` i `/api/auth/me`: 200						|
| ZAP-08 | Ograniczony fuzzing dodatkowego `probe` przy GET `/api/notes`	| 10 wysłanych wiadomości, 0 błędów, wszystkie widoczne odpowiedzi 200; PASS w ograniczonym zakresie						|

W ZAP-02 lokalny brak HSTS nadal odpowiadał uruchomieniu w Development. Dwa pozostające wystąpienia alertu `X-Content-Type-Options Header Missing` i alert CORS dotyczyły zasobów zewnętrznych. Obecność informacyjnego alertu cache-control, także przy odpowiedzi `/api/auth/me` zawierającej dowód `no-store`, nie obalała retestu REM-01. Brak ponownej obserwacji komentarzy Browser Refresh dotyczył badanego ruchu, a nie gwarantował ich nieobecności w każdym środowisku.

ZAP-04 nie obejmował pełnego sprawdzenia parsera JSON: nieuwierzytelnione żądanie POST zostało odrzucone, zanim prawidłowo przekazano planowaną błędną treść. Wynik GET `/api/notes/invalid-id` mógł wynikać z niedopasowania trasy, a nie z walidacji identyfikatora.

W ZAP-07 wykorzystano izolowaną bazę `securenotes_zap_db`, oddzielne konto i dwie fikcyjne notatki. Ciasteczko `SecureNotes.Auth` oznaczono jako token sesji ZAP i aktywowano rozpoznaną sesję HTTP. Bezpośrednim dowodem uwierzytelnionego działania były odpowiedzi 200 zarejestrowane w zakładce **Spider → Messages**, nie jedynie wcześniejsze odpowiedzi przeglądarki. Po ZAP-07 i ZAP-08 nie zaobserwowano nowych kategorii alertów dla lokalnej instancji `7038`.

ZAP-08 testował **niezadeklarowany parametr `probe`**. Nie jest to test walidacji właściwego identyfikatora notatki ani potwierdzenie odporności na SQL Injection czy XSS. Active Scan, duże słowniki fuzzingu i automatyczne wysyłanie formularzy nie były częścią tego przebiegu.

## 7. Ryzyko rezydualne i ograniczenia

1. **Wdrożenie Production niezweryfikowane.** Należy potwierdzić certyfikat, przekierowanie HTTP do HTTPS oraz faktyczny nagłówek HSTS na docelowej domenie.
2. **Ograniczony zakres wejść.** Test XSS obejmował jeden przykład Stored XSS, a fuzzing dotyczył nieobsługiwanego parametru `probe`; nie zastępują pełnego testowania walidacji, kodowania i wszystkich klas ataku.
3. **Brak Active Scan.** Nie wykonywano szerokiego aktywnego skanowania podatności, a Spider miał ograniczoną głębokość i czas.
4. **Sesje JWT.** Potwierdzono standardowe zachowanie po wylogowaniu w przeglądarce, nie osobne unieważnienie wcześniej skopiowanego tokenu. Tokeny widoczne na roboczych zrzutach nie powinny być dalej używane; surowych sesji ZAP ani zdjęć z sekretami nie należy umieszczać w Git.
5. **Narzędzia Development.** Swagger UI jest ograniczany przez zachowaną politykę CSP. Nie jest to podstawa do osłabienia globalnych dyrektyw dla aplikacji.
6. **Interpretacja alertów.** Liczba kategorii ZAP zależy od przechwyconego ruchu; spadek z siedmiu do sześciu nie stanowi ogólnego wskaźnika braku podatności.

## 8. Rekomendacje końcowe

Przed publikacją aplikacji przeprowadzić kontrolę rzeczywistej konfiguracji Production i udokumentować HTTPS/HSTS. Zachować `Cache-Control: no-store` na odpowiedziach API z prywatnymi danymi oraz dotychczasową restrykcyjną CSP. Utrzymywać automatyczne testy regresyjne i dokumentować kolejne zmiany zabezpieczeń. Ewentualny przyszły Active Scan wykonywać wyłącznie na izolowanym środowisku z odtwarzalną bazą i fikcyjnymi danymi. Zweryfikować, że końcowe materiały dowodowe są pozbawione sekretów; nie publikować eksportów sesji proxy. Rozważyć odrębny test zachowania przechwyconego JWT po wylogowaniu, jeśli wymaga tego model zagrożeń wdrożenia.

## 9. Wnioski końcowe

W ramach zdefiniowanego lokalnego audytu potwierdzono oczekiwane zachowanie mechanizmów uwierzytelniania, autoryzacji właściciela notatki, ochrony CSRF, wyświetlania testowej treści bez wykonania JavaScript, wybranych nagłówków bezpieczeństwa i atrybutów ciasteczka sesyjnego. Ustalenia związane z brakiem jawnego cache-control dla prywatnej odpowiedzi i konfiguracją HSTS zostały objęte poprawkami oraz opisanymi retestami. Końcowe 85/85 testów automatycznych zakończyło się sukcesem.

**Wynik końcowy: pozytywna ocena w granicach wykonanego audytu, z otwartym wymaganiem weryfikacji Production.** Raport nie poświadcza całkowitej odporności na wszystkie możliwe zagrożenia.

## 10. Źródła i materiały dowodowe

Dokumenty przechowywane w `docs/security-audit/`:

- `test-cases.md` — plan pierwotnych scenariuszy i oczekiwane rezultaty;
- `evidence-guide.md` — zasady zbierania i anonimizacji dowodów;
- `results.md` — szczegółowe wyniki audytu pierwotnego (zachowane bez nadpisywania);
- `remediation-results.md` — poprawki REM-01–03 i wyniki ZAP-02–08;
- `screenshots/auth/`, `screenshots/idor/`, `screenshots/csrf/`, `screenshots/xss/`, `screenshots/headers/`, `screenshots/zap/` — materiał pierwotnego audytu;
- `screenshots/retest/` i `screenshots/zap-retest/` — materiał ponownej weryfikacji.

Szczegółowe żądania, warunki i przyporządkowanie poszczególnych zrzutów do testów znajdują się w dwóch dokumentach wynikowych. Raport syntetyzuje je bez ponownego publikowania wartości tokenów i prywatnych odpowiedzi.
