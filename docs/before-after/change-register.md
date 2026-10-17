# Rejestr zmian BEFORE względem AFTER

Baza: `21be7bb6aadd69c729250eb5d5248cabb114de04`. Aktualizacja 17.10.2026, etap 05B: **CHG-XSS-01 — IMPLEMENTED / PARTIALLY VERIFIED (interpretacja HTML)**; CHG-XSS-02, CHG-CSRF-01 i CHG-CSRF-02 pozostają **PLANNED**. Stan pośredni zostaje utrwalony commitem `demo(xss): introduce HTML rendering and document CSP blocking`, którego rodzicem jest `714973314721918b3b59cb0b81c8fe270822fede`. Pełny SHA należy odczytać z historii tego commita; nie przypisujemy go wstecz uruchomieniu użytkownika, które odbyło się na stanie roboczym. T-XSS-02 i T-XSS-04 są PARTIAL w zakresie BEFORE; pozostałe 13 scenariuszy NOT RUN. Numery linii dotyczą bazy. Kod aplikacji różni się jedną linią notes.js; bez zmian backendu, CSP, modeli, bazy, uprawnień, cookies, JWT i CORS.

## CHG-XSS-01 — renderowanie treści

- **Plik/metoda:** `SecureNotes.API/wwwroot/js/notes.js`, `createNoteElement`, linia 205.
- **AFTER — VERIFIED:** `content.textContent = note.content`; tytuł w linii 202 także korzysta z `textContent`.
- **BEFORE — IMPLEMENTED / PARTIALLY VERIFIED (05B):** w linii 205 zastąpiono `content.textContent = note.content;` przez `content.innerHTML = note.content;`. Tytuł, edycja, komunikaty i pozostałe elementy pozostają bez zmian. Użytkownik 17.10.2026 potwierdził interpretację zapisanego HTML i blokadę demonstracyjnego handlera przez oryginalną CSP; Codex nie wykonywał tych prób.
- **Uzasadnienie:** pokazanie skutku użycia niebezpiecznego ujścia DOM dla danych przechowywanych w bazie.
- **Wpływ:** niezaufana treść jest przekazywana do interpretera HTML — HTML Injection potwierdzone przez użytkownika. Nie wykazano wykonania Stored XSS ani dostarczenia payloadu innemu użytkownikowi. CSP i SecurityHeadersMiddleware.cs pozostają niezmienione; `script-src-attr 'none'` zablokowało testowy inline event handler. Kolejny etap wykonania tego payloadu wymaga osobno zleconego CHG-XSS-02; nie jest on wdrożony.
- **Testy:** T-XSS-01, T-XSS-02, T-XSS-03, T-XSS-04; regresja T-REG-01.
- **Dowody:** odpowiedź GET zawierająca payload, DOM, porównanie z tekstem w AFTER. Ograniczenie: samo pojawienie się elementu HTML nie potwierdza JavaScript.
- **Zaobserwowane 17.10.2026 przez użytkownika:** `<strong>DEMO-XSS-HTML</strong>` dało pogrubiony napis (część HTML T-XSS-02). Osobne `<img src="/xss-demo-nieistniejacy-obraz.png" onerror="alert('XSS DEMO')">` utworzyło img, wywołało 404 obrazu, bez alertu; Console wskazała blokadę przez `script-src-attr 'none'` (część BEFORE T-XSS-04). To zgodność części pomiaru, nie PASS pełnych scenariuszy. Brak potwierdzonego porównania AFTER, osobnego GET/ID i powtórzenia po przeładowaniu; nie zaliczamy T-XSS-01/03. Szczegóły: [results.md](results.md). Screenshoty zgłoszono w rozmowie, lecz archiwizacja w repo nie jest potwierdzona: brak nadanych nazw/ID/ścieżek. Przed zmianą CSP zachować ten commit i uzupełnić rzeczywiste materiały. Nie używać zewnętrznych adresów ani wykradania danych.

## CHG-XSS-02 — ograniczony wyjątek CSP

- **Plik/metoda:** `SecureNotes.API/Middleware/SecurityHeadersMiddleware.cs`, `InvokeAsync`, linie 28–40, szczególnie 31.
- **AFTER — VERIFIED:** `script-src 'self'; script-src-attr 'none'`; pozostała polityka ogranicza obrazy, połączenia, formularze, ramki i bazowy URL.
- **BEFORE — PLANNED:** wyłącznie dla odpowiedzi dokumentu `/notes.html` zastosować `script-src-attr 'unsafe-inline'`. Zachować `script-src 'self'` i resztę polityki. Nie zmieniać CSP globalnie ani dodawać wyjątku dla Swaggera.
- **Uzasadnienie:** oddzielenie niebezpiecznego renderera od drugiej bariery, która blokuje handler zdarzenia.
- **Wpływ:** celowe dopuszczenie handlerów na stronie notatek; wyjątek nie ogranicza się technicznie do jednego payloadu. Dlatego jest dopuszczalny wyłącznie w lokalnym wariancie demonstracyjnym.
- **Testy:** T-XSS-02, T-XSS-03, T-XSS-04 oraz T-REG-04.
- **Dowody:** nagłówek dokumentu, alert i atrybut DOM ustawiony przez payload. Test etapu pośredniego wymaga oryginalnej CSP i samego CHG-XSS-01.

## CHG-CSRF-01 — tworzenie JSON bez Antiforgery

- **Plik/metoda:** `SecureNotes.API/Controllers/NotesController.cs`, `Create`, linia 89; `[FromBody]` w linii 91.
- **AFTER — VERIFIED:** `[Authorize]` na kontrolerze oraz `[ValidateAntiForgeryToken]` na akcji tworzenia.
- **BEFORE — PLANNED:** usunąć walidację Antiforgery wyłącznie z tworzenia notatki JSON. Pozostawić ją dla PUT, DELETE i operacji logowania, rejestracji, wylogowania.
- **Uzasadnienie:** umożliwienie porównania tego samego kontraktu JSON dla poprawnego, błędnego i brakującego tokenu.
- **Wpływ:** brak walidacji tokenu przy tej operacji. Samo to nie zapewnia skuteczności formularza, ponieważ nadal obowiązuje format danych, uwierzytelnianie i polityki przeglądarki.
- **Testy:** T-CSRF-A01, T-CSRF-A02, T-CSRF-A03, T-CORS-01, T-REG-02.
- **Dowody:** zanonimizowane żądania/odpowiedzi i lista notatek przed/po. Ważne uwierzytelnienie i poprawny JSON muszą być warunkami stałymi.

## CHG-CSRF-02 — laboratoryjna obsługa formularza

- **Plik/metoda:** `SecureNotes.API/Controllers/NotesController.cs`, nowa planowana akcja `CreateFromForm` obok `Create`, obecny obszar linii 88–127.
- **AFTER — VERIFIED:** istnieje tylko tworzenie z `[FromBody]`; brak akcji `[FromForm]`.
- **BEFORE — PLANNED:** dodać mały adapter `[FromForm] CreateNoteRequest` pod tym samym `POST /api/notes`, bez Antiforgery. Rozdzielić akcje przez `[Consumes]`, zachowując dotychczasowe typy JSON, także odpowiednie `+json`. Adapter deleguje do istniejącej logiki tworzenia. Nazwa nowej metody jest propozycją, nie istniejącym symbolem.
- **Uzasadnienie:** standardowy formularz URL-encoded nie wysyła JSON, a cross-origin JSON wymaga preflight. Adapter zapewnia wykonalny model bez osłabiania cookies i CORS.
- **Wpływ:** nowa, jawnie laboratoryjna powierzchnia ataku. Nadal obowiązuje `[Authorize]`, właściciel z claims i walidacja domeny. Nie przyjmujemy `UserId` od klienta.
- **Testy:** T-CSRF-B01, T-CSRF-B02, T-CSRF-B03, T-CORS-01, T-REG-01, T-REG-03.
- **Dowody:** pochodzenie formularza, faktyczny POST, brak tokenu i zapis notatki. AFTER może odrzucić żądanie przez Antiforgery albo nieobsługiwany format; przyczyna wymaga sprawdzenia.

## Kryterium kontroli zakresu

Przed wdrożeniem należy rozwiązać kwestie izolacji i HTTPS z [opisu środowiska](environment.md). Po każdej zmianie porównać diff z powyższym zakresem. Brak dodatkowych zmian jest kontrolą zakresu, nie dowodem bezpieczeństwa. Realizacja i SHA commitów zostaną wpisane dopiero po rzeczywistym wykonaniu. Wyniki testów trafiają do [rejestru wyników](results.md), nie zastępują statusu wykonania zmiany.
