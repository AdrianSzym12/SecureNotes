# Rejestr zmian BEFORE względem AFTER

## Zakończenie implementacji CSRF — 06F, 17.10.2026

CHG-CSRF-02: **IMPLEMENTED / PARTIALLY VERIFIED — BEFORE, formularz cross-origin / same-site**. Obie poniższe próby wykonał i potwierdził użytkownik 17.10.2026, nie Codex.

- Pierwotny formularz z https://localhost:7443 wysłał POST do https://localhost:7028/api/notes: utworzono `SN-T-CSRF-B01`, API zwróciło JSON nowego rekordu i notatka była widoczna w UI. Główna karta przeszła do JSON.
- Po dodaniu target=csrf-result-frame, ukrytego iframe i frame-src https://localhost:7028 wyłącznie do CSP serwera demonstracyjnego użytkownik potwierdził pozostanie na stronie demonstracyjnej, utworzenie `SN-T-CSRF-B02`, widoczność w UI i nowy rekord w MariaDB.

Nie udokumentowano dokładnych kodów HTTP ani rzeczywistych nagłówków tych prób; nie dopisujemy ich. Nie zgłoszono konkretnego komunikatu blokady iframe ani niezależnego pomiaru cookies. Strona demonstracyjna nie odczytuje odpowiedzi API; zapis potwierdzono niezależnie. Frame-ancestors none i X-Frame-Options DENY SecureNotes pozostają bez zmian: ewentualna blokada wyświetlenia odpowiedzi w ramce nie oznacza automatycznie odrzucenia POST.

Oba przebiegi należą do **T-CSRF-B01 — PARTIAL — BEFORE**. Nazwa notatki SN-T-CSRF-B02 jest znacznikiem danych, a nie wykonaniem scenariusza T-CSRF-B02 (formularz bez uwierzytelnienia). T-CSRF-B02 i T-CSRF-B03 nadal NOT RUN. W całej serii jest **6 PARTIAL — BEFORE / 9 NOT RUN**; wcześniejsze obserwacje XSS i JSON pozostają zachowane, bez pełnego PASS dla porównań z AFTER.

Cross-origin localhost:7443 → localhost:7028: potwierdzony przez użytkownika. Same-site: oba originy to HTTPS localhost, różnią się portem. Obejście SameSite=Strict: nieudowodnione. Atak z niezależnej domeny oraz porównanie AFTER: nieprzetestowane. Nie zmieniano JWT, cookies, SameSite, CORS ani CSP SecureNotes w ramach demonstracji. Pozostałe chronione operacje zachowują Antiforgery. Obie zmiany XSS i CHG-CSRF-01 pozostają wdrożone i częściowo zweryfikowane przez użytkownika w dotychczasowym zakresie.

Screenshoty są zgłoszone w rozmowie; fizyczna archiwizacja niepotwierdzona. Nie nadano rzeczywistych nazw, ścieżek ani ID dowodów. Przed publikacją ukryć cookies/tokeny, identyfikatory użytkowników i niepotrzebne dane phpMyAdmin. Uzupełnić rzeczywiste metadane prób i pełny payload XSS; nie przypisywać przyszłego SHA commita wstecz wcześniejszym uruchomieniom. Porównania z AFTER pozostają planem próby generalnej na końcu projektu, nie zadaniem 06F.

Odtworzenie po osobnym zleceniu: uruchomić BEFORE przez start-before na securenotes_before_lab, zalogować własne fikcyjne konto na HTTPS localhost:7028, w drugiej sesji uruchomić osobny serwer demonstracyjny na HTTPS localhost:7443. Otworzyć go jako główną kartę w tym samym profilu; nadać unikalny fikcyjny tytuł i świadomie wysłać natywny formularz URL-encoded do ukrytego iframe, bez JS/fetch/tokenu. Sprawdzić nowy zapis w UI/GET BEFORE i osobno w bazie przez uprawnionego użytkownika. Nie odczytywać odpowiedzi z originu demonstracyjnego, nie uznawać samego braku błędu za sukces. Zachować zanonimizowane dowody i usunąć wyłącznie własną notatkę przez prawidłowo uwierzytelnione UI z tokenem. Serwer loopback nie czyta ani nie loguje cookies, które mogą przyjść na 7443; nie uruchamiać proxy ani przechwytywania sesji.

Starsze sekcje 06D/06C/06B poniżej opisują ówczesny stan przygotowania i wyniki, a nie aktualne statusy 06F. Codex w 06F nie uruchamiał aplikacji, serwerów, HTTP, SQL, migracji ani testów integracyjnych.

## Aktualizacja 06C — CHG-CSRF-02

**IMPLEMENTED / NOT TESTED.** Punkt wyjścia: `bf2b6421926f7979e7200009e40c7b7c6239a25e`, branch demo/security-before. W NotesController dodano tylko rozdzielenie formatów i adapter formularza, bez nowej ścieżki URL:

- `POST /api/notes`, `Create`, linie 88–92: `[Consumes("application/json", "text/json", "application/*+json")]` i dotychczasowy `[FromBody] CreateNoteRequest`.
- Ten sam POST, `CreateFromForm`, linie 129–136: `[Consumes("application/x-www-form-urlencoded")]`, `[FromForm] CreateNoteRequest`, pola title/content. Akcja zwraca `Create(request, cancellationToken)`.

Rozłączne typy w Consumes pozwalają mechanizmowi wyboru akcji MVC wybrać odpowiedni POST na podstawie Content-Type. Nie dodano innego routingu. JSON zachowuje także text/json i typy z sufiksem +json; formularz nie przechwytuje JSON. To ocena statyczna — rzeczywisty routing, statusy i wiązanie rekordu wymagają testów HTTP. Typy spoza zadeklarowanych zestawów nie otrzymują nowej obsługi.

Obie akcje podlegają klasowemu [Authorize]; nie mają ValidateAntiForgeryToken. Bezpośrednia delegacja do Create współdzieli pobranie właściciela z claims, kontrole danych, NoteService.CreateAsync, zapis i obsługę ArgumentException oraz CreatedAtAction(nameof(GetById), new { id = note.Id }, note). Oczekiwana odpowiedź to 201 z Location prowadzącym do GetById; nie jest to wykonany pomiar. Wywołanie metody C# nie uruchamia ponownie routingu, model binding ani filtrów MVC: te dotyczą wybranej akcji formularza. [ApiController] i walidacja modelu pozostają aktywne. DTO jest rekordem CreateNoteRequest(Title, Content); domena nadal odrzuca pusty/biały tytuł, tytuł ponad 150 znaków, null treści oraz treść ponad 10000 znaków.

Ryzyko: zwykły formularz może utworzyć notatkę w uwierzytelnionej sesji bez tokenu żądania. Nie potwierdzono jeszcze ataku. JWT, HttpOnly/Secure/SameSite=Strict, CORS, PUT/DELETE, register/login/logout i ich Antiforgery pozostają bez zmian. Obie zmiany Stored XSS i CHG-CSRF-01 zachowano. Docelowa demonstracja z https://localhost:7443 do https://localhost:7028 jest cross-origin, ale same-site. Port nie izoluje cookies; standardowa nawigacja formularza nie wymaga preflight jak fetch JSON. Przesłanie cookie, uwierzytelnienie i zapis wymagają pomiaru. Nie jest to dowód obejścia SameSite ani ataku z niezależnej domeny cross-site. Lokalny serwer demonstracyjny może otrzymać Auth przy otwarciu strony i musi być kontrolowany, ograniczony do localhost, bez zapisywania/publikowania nagłówków Cookie.

Statusy scenariuszy pozostają bez zmian: 5 PARTIAL — BEFORE (T-XSS-02/04, T-CSRF-A01/A02/A03), 10 NOT RUN, w tym T-CSRF-B01/B02/B03 i T-REG-04. W 06C nie uruchamiano aplikacji, HTTP, SQL, migracji ani testów integracyjnych. Nie utworzono HTML/serwera demonstracyjnego. Kontroler skompilowano odrębnie lokalnym kompilatorem C# z referencjami .NET/ASP.NET Core 8 i istniejącą biblioteką Application; wynik PASS, artefakty poza repozytorium. To kontrola kompilowalności, nie test integracji.

Poniższe wcześniejsze wpisy opisują historię lub wcześniejszy plan; aktualny status CHG-CSRF-02 określa sekcja 06C.

## Wyniki użytkownika 06B — 17.10.2026

**CHG-CSRF-01 — IMPLEMENTED / PARTIALLY VERIFIED (BEFORE, JSON).** Użytkownik, nie Codex, przeprowadził ręczne próby na laboratoryjnym BEFORE:

| Test | Rzeczywista obserwacja BEFORE | Status scenariusza |
|---|---|---|
| T-CSRF-A01 | Zwykły zapis przez UI; obecny X-CSRF-TOKEN, HTTP 201; rekord `CSRF-TEST-VALID` potwierdzony przez użytkownika w MariaDB | PARTIAL — BEFORE |
| T-CSRF-A02 | Uwierzytelniony POST JSON /api/notes z nieprawidłowym X-CSRF-TOKEN; utworzono `CSRF-TEST-INVALID-1792246089074`, rekord potwierdzony przez użytkownika w securenotes_before_lab.notes; konkretny kod HTTP udanej próby nie został udokumentowany | PARTIAL — BEFORE |
| T-CSRF-A03 | Uwierzytelniony POST JSON /api/notes bez nagłówka X-CSRF-TOKEN; utworzono `CSRF-TEST-MISSING-1792246089081`, rekord potwierdzony przez użytkownika w securenotes_before_lab.notes; konkretny kod HTTP udanej próby nie został udokumentowany | PARTIAL — BEFORE |

Wcześniejsze ręczne próby A02/A03 zwróciły HTTP 401. Później GET /api/notes dał HTTP 200, a ponowione POST doprowadziły do zapisów opisanych wyżej. Nie ustalono jednoznacznej przyczyny wcześniejszych 401; nie interpretujemy ich jako odrzucenia przez Antiforgery ani nie przypisujemy kodu 200 z GET późniejszym POST. Przewidywane 201 z kodu akcji nie zastępuje pomiaru odpowiedzi A02/A03.

Potwierdzono przyjmowanie uwierzytelnionego JSON mimo błędnego lub brakującego tokenu żądania. Nie przeprowadzono porównania AFTER, demonstracji formularza cross-origin ani ataku cross-site; nie wykazano obejścia JWT, SameSite ani CORS. CHG-CSRF-02 pozostaje PLANNED. Statusy całej serii: **5 PARTIAL — BEFORE** (T-XSS-02/04 i T-CSRF-A01/A02/A03), pozostałe **10 NOT RUN**. Historyczne wyniki XSS pozostają bez zmian. Nie nadajemy pełnego PASS scenariuszom obejmującym AFTER.

Źródło: relacja użytkownika z 17.10.2026 i screenshoty dostępne według niego w rozmowie ChatGPT. Codex nie wykonywał prób ani nie sprawdzał rekordów SQL. Archiwizacja screenshotów niepotwierdzona; brak przypisanych rzeczywistych nazw plików, ścieżek i ID dowodów. Przed publikacją zrzutu phpMyAdmin ukryć identyfikatory użytkowników i inne niepotrzebne dane, a także wszelkie sekrety/cookies/tokeny. Nie uzupełniamy nieznanych treści notatek, wartości tokenów, czasów prób ani ID rekordów.

Poniższe sekcje 06A i wcześniejsze zachowano jako historię. Kod 06B pozostaje dokładnie zmianą jednej linii z 06A.

## Aktualizacja 06A — 17.10.2026

**CHG-CSRF-01 — IMPLEMENTED / NOT TESTED.** W `SecureNotes.API/Controllers/NotesController.cs`, akcja `Create`, usunięto tylko `[ValidateAntiForgeryToken]` bezpośrednio po `[HttpPost]` (linia 89 w HEAD `138d02b5f3a6a1b0c3af19404caf3baee6dd6684`; po zmianie deklaracja Create zaczyna się w linii 89, parametr FromBody w 90). To nadal `POST /api/notes` z JSON `CreateNoteRequest(Title, Content)` i odpowiedzią `CreatedAtAction` (201). Nie dodano obsługi formularzy: **CHG-CSRF-02 — PLANNED**.

Analiza aktualnego kodu: `Program.cs` rejestruje `AddControllersWithViews()` bez globalnych filtrów Antiforgery oraz `AddAntiforgery()` z nagłówkiem X-CSRF-TOKEN. Rejestracja usługi i konfiguracja cookie same nie wymuszają walidacji wszystkich akcji. Nie znaleziono AutoValidateAntiforgeryToken, globalnej konfiguracji filtrów, UseAntiforgery ani dodatkowych wywołań walidacji żądania. Lokalny atrybut akcji uruchamiał filtr MVC walidujący token przed wykonaniem Create; po usunięciu ten filtr nie jest do niej przypisany. Wydawanie pary tokenów przez GET /api/auth/csrf-token pozostaje bez zmian.

Ryzyko: uwierzytelniony JSON może zostać zapisany bez poprawnego tokenu żądania. To hipoteza do pomiaru, nie potwierdzony wynik CSRF. `[Authorize]`, UseAuthentication/UseAuthorization i JWT z SecureNotes.Auth pozostają aktywne (walidacja podpisu, wystawcy, odbiorcy i czasu życia). Tożsamość właściciela nadal pochodzi z ClaimTypes.NameIdentifier; DTO nie przyjmuje UserId. Walidacja danych i odpowiedzi akcji pozostają bez zmian. Cookies zachowują HttpOnly, Secure i SameSite=Strict; nie zmieniono CORS (brak AddCors/UseCors). JSON z obcego originu nadal wymaga preflight; nie wykazano obejścia CORS, SameSite ani ataku cross-origin/cross-site. Brak JWT nadal uniemożliwia autoryzowany zapis.

PUT i DELETE notatek oraz register/login/logout nadal mają ValidateAntiForgeryToken. CHG-XSS-01 i CHG-XSS-02 pozostają wdrożone bez zmian kodu; użytkownik potwierdził wykonanie JS w 05D, a pełna seria porównawcza nadal jest nieukończona. Statusy 15 scenariuszy bez zmian: T-XSS-02 i T-XSS-04 PARTIAL — BEFORE, pozostałe 13 NOT RUN. Historia wyników 05B/05D pozostaje nienaruszona. Nie uruchamiano aplikacji, testów, SQL ani migracji; brak git add/commit/push.

Plan pomiaru: T-CSRF-A01 (poprawny token), T-CSRF-A02 (błędny token), T-CSRF-A03 (brak nagłówka); regresja T-CORS-01 i T-REG-02. Poniższe wcześniejsze sekcje 05D/05C/05B zachowują historyczny stan tych etapów.

## Wynik użytkownika 05D — 17.10.2026

Użytkownik potwierdził rzeczywiste wykonanie JavaScript z zapisanej treści notatki (Stored XSS) na `https://localhost:7028/notes.html` po CHG-XSS-02. Payload używał elementu img i onerror; wyświetlony alert miał dokładnie treść:

> XSS DEMO - Właśnie zostałeś zaatakowany przez Albańskich Hakerów.

Tekst jest humorystycznym elementem bezpiecznego eksperymentu. Nie znamy pełnego źródła payloadu tej próby; nie utożsamiamy go z wcześniejszym payloadem o krótszym tekście alertu i nie dopisujemy brakujących atrybutów ani znacznika data-sn-xss. Obserwację wykonał użytkownik 17.10.2026, nie Codex. Screenshoty są dostępne według użytkownika w rozmowie, lecz fizyczna archiwizacja nie jest potwierdzona; nie przypisano im nazw, ścieżek ani ID dowodów.

CHG-XSS-02: **IMPLEMENTED / PARTIALLY VERIFIED — wykonanie JS w BEFORE potwierdzone przez użytkownika**. T-XSS-02 pozostaje **PARTIAL — BEFORE**, teraz z potwierdzonym wykonaniem JS. Brak pełnego porównania AFTER i pozostałych danych protokołu nie pozwala nadać PASS całemu scenariuszowi. T-XSS-04 pozostaje **PARTIAL — BEFORE** z historycznym wynikiem blokady przy oryginalnej CSP w checkpointcie `9c825ad8d87a63f1134bbfc1e0f64a9b57ea6c11`. Pozostałe 13 scenariuszy nadal **NOT RUN**, w tym T-XSS-01/03 i T-REG-04: nie zgłoszono odrębnego porównania GET/content/ID, próby trwałości po ponownym pobraniu ani pełnego pomiaru nagłówków.

Rzeczywiste nagłówki CSP pozostałych ścieżek, a także zachowanie AFTER wymagają osobnej weryfikacji. Ograniczenie wyjątku do dokładnego `/notes.html` oraz zachowanie pozostałych dyrektyw potwierdza analiza kodu, nie pomiar odpowiedzi HTTP. Nie wykazano dostarczenia payloadu innemu użytkownikowi ani przejęcia konta. Docelowy BEFORE zachowuje podatny renderer i wyjątek CSP; CHG-CSRF-01/02 nadal PLANNED.

Poniższe aktualizacje 05C/05B zachowano jako historię, a nie aktualny status weryfikacji.

Aktualizacja 17.10.2026, etap 05C: **CHG-XSS-02 — IMPLEMENTED / NOT TESTED**. W `SecurityHeadersMiddleware.InvokeAsync`, linie 31–33, ten sam nagłówek CSP wybiera `script-src-attr 'unsafe-inline'` tylko gdy `string.Equals(context.Request.Path.Value, "/notes.html", StringComparison.Ordinal)`. Porównanie obejmuje całą ścieżkę, rozróżnia wielkość liter i nie obejmuje query string. `/api/notes`, `/NOTES.html`, `/notes.html/` i inne ścieżki zachowują `script-src-attr 'none'`. Pozostałe dyrektywy oraz nagłówki pozostają bez zmian. To przegląd kodu, nie potwierdzenie przeglądarkowe. Docelowy BEFORE zachowuje ten wyjątek bez przełącznika i automatycznego przywracania. CHG-CSRF-01/02 nadal PLANNED. Poniższy zapis 05B opisuje historyczny stan pośredni, zachowany w `9c825ad8d87a63f1134bbfc1e0f64a9b57ea6c11`; jego wyniki (2 PARTIAL, 13 NOT RUN) nie zmieniają się.

Baza: `21be7bb6aadd69c729250eb5d5248cabb114de04`. Aktualizacja 17.10.2026, etap 05B: **CHG-XSS-01 — IMPLEMENTED / PARTIALLY VERIFIED (interpretacja HTML)**; CHG-XSS-02, CHG-CSRF-01 i CHG-CSRF-02 pozostają **PLANNED**. Stan pośredni zostaje utrwalony commitem `demo(xss): introduce HTML rendering and document CSP blocking`, którego rodzicem jest `714973314721918b3b59cb0b81c8fe270822fede`. Pełny SHA należy odczytać z historii tego commita; nie przypisujemy go wstecz uruchomieniu użytkownika, które odbyło się na stanie roboczym. T-XSS-02 i T-XSS-04 są PARTIAL w zakresie BEFORE; pozostałe 13 scenariuszy NOT RUN. Numery linii dotyczą bazy. Kod aplikacji różni się jedną linią notes.js; bez zmian backendu, CSP, modeli, bazy, uprawnień, cookies, JWT i CORS.

## CHG-XSS-01 — renderowanie treści

- **Plik/metoda:** `SecureNotes.API/wwwroot/js/notes.js`, `createNoteElement`, linia 205.
- **AFTER — VERIFIED:** `content.textContent = note.content`; tytuł w linii 202 także korzysta z `textContent`.
- **BEFORE — IMPLEMENTED / PARTIALLY VERIFIED (05B):** w linii 205 zastąpiono `content.textContent = note.content;` przez `content.innerHTML = note.content;`. Tytuł, edycja, komunikaty i pozostałe elementy pozostają bez zmian. Użytkownik 17.10.2026 potwierdził interpretację zapisanego HTML i blokadę demonstracyjnego handlera przez oryginalną CSP; Codex nie wykonywał tych prób.
- **Uzasadnienie:** pokazanie skutku użycia niebezpiecznego ujścia DOM dla danych przechowywanych w bazie.
- **Wpływ:** niezaufana treść jest przekazywana do interpretera HTML — HTML Injection potwierdzone przez użytkownika. W historycznym etapie 05B nie wykazano wykonania Stored XSS ani dostarczenia payloadu innemu użytkownikowi. Wówczas CSP i SecurityHeadersMiddleware.cs pozostawały niezmienione; `script-src-attr 'none'` zablokowało testowy inline event handler. CHG-XSS-02 wdrożono następnie w 05C, a użytkownik potwierdził wykonanie JS w 05D, jak opisano wyżej.
- **Testy:** T-XSS-01, T-XSS-02, T-XSS-03, T-XSS-04; regresja T-REG-01.
- **Dowody:** odpowiedź GET zawierająca payload, DOM, porównanie z tekstem w AFTER. Ograniczenie: samo pojawienie się elementu HTML nie potwierdza JavaScript.
- **Zaobserwowane 17.10.2026 przez użytkownika:** `<strong>DEMO-XSS-HTML</strong>` dało pogrubiony napis (część HTML T-XSS-02). Osobne `<img src="/xss-demo-nieistniejacy-obraz.png" onerror="alert('XSS DEMO')">` utworzyło img, wywołało 404 obrazu, bez alertu; Console wskazała blokadę przez `script-src-attr 'none'` (część BEFORE T-XSS-04). To zgodność części pomiaru, nie PASS pełnych scenariuszy. Brak potwierdzonego porównania AFTER, osobnego GET/ID i powtórzenia po przeładowaniu; nie zaliczamy T-XSS-01/03. Szczegóły: [results.md](results.md). Screenshoty zgłoszono w rozmowie, lecz archiwizacja w repo nie jest potwierdzona: brak nadanych nazw/ID/ścieżek. Przed zmianą CSP zachować ten commit i uzupełnić rzeczywiste materiały. Nie używać zewnętrznych adresów ani wykradania danych.

## CHG-XSS-02 — ograniczony wyjątek CSP

- **Plik/metoda:** `SecureNotes.API/Middleware/SecurityHeadersMiddleware.cs`, `InvokeAsync`, CSP w liniach 28–42, warunek w 31–33 (numeracja po 05C).
- **AFTER — VERIFIED:** `script-src 'self'; script-src-attr 'none'`; pozostała polityka ogranicza obrazy, połączenia, formularze, ramki i bazowy URL.
- **BEFORE — IMPLEMENTED / PARTIALLY VERIFIED (05D, relacja użytkownika):** dokładna ścieżka `/notes.html` otrzymuje `script-src-attr 'unsafe-inline'`; pozostałe ścieżki nadal `script-src-attr 'none'`. Zachowano `script-src 'self'` i resztę pojedynczej polityki; brak wyjątku dla Swaggera, przełącznika i automatycznego przywracania. Wykonanie JS na stronie notatek potwierdził użytkownik; faktyczne nagłówki pozostałych ścieżek i AFTER wymagają osobnego pomiaru.
- **Uzasadnienie:** oddzielenie niebezpiecznego renderera od drugiej bariery, która blokuje handler zdarzenia.
- **Wpływ:** celowe dopuszczenie handlerów na stronie notatek; wyjątek nie ogranicza się technicznie do jednego payloadu. Dlatego jest dopuszczalny wyłącznie w lokalnym wariancie demonstracyjnym.
- **Testy:** T-XSS-02, T-XSS-03, T-XSS-04 oraz T-REG-04.
- **Dowody:** nagłówek dokumentu, alert i atrybut DOM ustawiony przez payload. Test etapu pośredniego wymaga oryginalnej CSP i samego CHG-XSS-01.

## CHG-CSRF-01 — tworzenie JSON bez Antiforgery

- **Plik/metoda:** `SecureNotes.API/Controllers/NotesController.cs`, `Create`: usunięty atrybut w linii 89 HEAD 138d02b; obecnie deklaracja w 89, `[FromBody]` w 90.
- **AFTER — VERIFIED:** `[Authorize]` na kontrolerze oraz `[ValidateAntiForgeryToken]` na akcji tworzenia.
- **BEFORE — IMPLEMENTED / PARTIALLY VERIFIED (06B, BEFORE JSON):** usunięto wyłącznie lokalny `[ValidateAntiForgeryToken]` z Create JSON. Pozostał dla PUT, DELETE i operacji logowania, rejestracji, wylogowania. Brak globalnego filtra wymuszającego walidację tej akcji; użytkownik potwierdził zapis z poprawnym, błędnym i brakującym tokenem. Kod 201 potwierdzono dla A01; kodów udanych A02/A03 nie udokumentowano. AFTER pozostaje do zbadania.
- **Uzasadnienie:** umożliwienie porównania tego samego kontraktu JSON dla poprawnego, błędnego i brakującego tokenu.
- **Wpływ:** brak walidacji tokenu przy tej operacji. Samo to nie zapewnia skuteczności formularza, ponieważ nadal obowiązuje format danych, uwierzytelnianie i polityki przeglądarki.
- **Testy:** T-CSRF-A01, T-CSRF-A02, T-CSRF-A03, T-CORS-01, T-REG-02.
- **Dowody:** zanonimizowane żądania/odpowiedzi i lista notatek przed/po. Ważne uwierzytelnienie i poprawny JSON muszą być warunkami stałymi.

## CHG-CSRF-02 — laboratoryjna obsługa formularza

- **Plik/metoda:** `SecureNotes.API/Controllers/NotesController.cs`: Consumes JSON w linii 89, akcja `CreateFromForm` z atrybutami w liniach 129–136 (06C).
- **AFTER — VERIFIED:** istnieje tylko tworzenie z `[FromBody]`; brak akcji `[FromForm]`.
- **BEFORE — IMPLEMENTED / PARTIALLY VERIFIED (06F, relacja użytkownika):** dodano `CreateFromForm` z `[FromForm] CreateNoteRequest` pod tym samym `POST /api/notes`, bez Antiforgery. Rozłączne `[Consumes]` rozdzielają JSON i URL-encoded; zachowano także text/json i application/*+json. Adapter zwraca wywołanie istniejącej metody Create. Użytkownik potwierdził zapis przez formularz cross-origin / same-site w dwóch wariantach; kody HTTP/nagłówki i porównanie AFTER pozostają nieudokumentowane.
- **Uzasadnienie:** standardowy formularz URL-encoded nie wysyła JSON, a cross-origin JSON wymaga preflight. Adapter zapewnia wykonalny model bez osłabiania cookies i CORS.
- **Wpływ:** nowa, jawnie laboratoryjna powierzchnia ataku. Nadal obowiązuje `[Authorize]`, właściciel z claims i walidacja domeny. Nie przyjmujemy `UserId` od klienta.
- **Testy:** T-CSRF-B01, T-CSRF-B02, T-CSRF-B03, T-CORS-01, T-REG-01, T-REG-03.
- **Dowody:** pochodzenie formularza, faktyczny POST, brak tokenu i zapis notatki. AFTER może odrzucić żądanie przez Antiforgery albo nieobsługiwany format; przyczyna wymaga sprawdzenia.

## Kryterium kontroli zakresu

Przed wdrożeniem należy rozwiązać kwestie izolacji i HTTPS z [opisu środowiska](environment.md). Po każdej zmianie porównać diff z powyższym zakresem. Brak dodatkowych zmian jest kontrolą zakresu, nie dowodem bezpieczeństwa. Realizacja i SHA commitów zostaną wpisane dopiero po rzeczywistym wykonaniu. Wyniki testów trafiają do [rejestru wyników](results.md), nie zastępują statusu wykonania zmiany.
