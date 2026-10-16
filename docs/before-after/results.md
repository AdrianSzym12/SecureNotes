# Wyniki i rejestr dowodów

Stan na 16.10.2026. Bazowy SHA: `21be7bb6aadd69c729250eb5d5248cabb114de04`. **Nie wykonano żadnego nowego testu.** Oczekiwania dotyczą przyszłego BEFORE po zmianach; obecne oba branche mają identyczny kod.

`NOT RUN` jest statusem wykonania, a nie wynikiem bezpieczeństwa. Po próbach zgodność oceniać jako zgodny/niezgodny/niejednoznaczny; skuteczność jako skuteczna/nieskuteczna/nieoceniona w danym zakresie. Wykrycie podatności w BEFORE może być zgodne z hipotezą, ale oznacza nieskuteczną kontrolę. Osobne protokoły dla BEFORE i AFTER muszą wskazywać rzeczywiście badany SHA.

## Matryca

| Test | Status | Oczekiwane BEFORE | Oczekiwane AFTER | Zaobserwowane BEFORE / AFTER | Zgodność z oczekiwaniem | Skuteczność zabezpieczenia | Dowód | Uwagi i ograniczenia |
|---|---|---|---|---|---|---|---|---|
| T-XSS-01 | NOT RUN | Zapis i odczyt treści | Zapis i odczyt treści | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono; to kontrola danych | Brak | Porównywać odkodowany JSON |
| T-XSS-02 | NOT RUN | HTML i wykonanie handlera | Tekst, brak wykonania | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono | Brak | DOM sam nie dowodzi wykonania |
| T-XSS-03 | NOT RUN | Ponowne wykonanie po GET | Ponownie tekst | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono | Brak | Dowód trwałości, nie ręczny DOM |
| T-XSS-04 | NOT RUN | Etap pośredni: HTML, handler blokowany | Tekst, brak wykonania | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono | Brak | Zachować pomiar i SHA etapu innerHTML z oryginalną CSP; dla niezatwierdzonych zmian także diff |
| T-CSRF-A01 | NOT RUN | 201 i zapis | 201 i zapis | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono; kontrola pozytywna | Brak | Token właściwy dla sesji |
| T-CSRF-A02 | NOT RUN | 201 i zapis | 400, brak zapisu | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono | Brak | Wymaga potwierdzenia przyczyny 400 |
| T-CSRF-A03 | NOT RUN | 201 i zapis | 400, brak zapisu | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono | Brak | Cookie Csrf obecne, brak tokenu żądania |
| T-CSRF-B01 | NOT RUN | Formularz tworzy notatkę | Brak zapisu; przyczyna do ustalenia | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono; brak izolacji Antiforgery | Brak | Asymetria obsługi formatów |
| T-CSRF-B02 | NOT RUN | Brak zapisu bez uwierzytelnienia; hipoteza 401 | Brak zapisu; 401 dla właściwej ścieżki lub wcześniejsza odmowa routingu/formatu | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono | Brak | Priorytet: brak zapisu bez Auth; faktyczny kod i przyczyna dopiero po testach |
| T-CSRF-B03 | NOT RUN | Strict: brak Auth i zapisu | Brak zapisu | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono | Brak | Diagnostyka powodu wykluczenia cookie |
| T-CORS-01 | NOT RUN | Brak zgody preflight, bez POST | Tak samo | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono | Brak | Błąd odczytu sam nie wystarcza |
| T-REG-01 | NOT RUN | Poprawny CRUD z tokenami | Poprawny CRUD z tokenami | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono; kontrola funkcjonalna | Brak | Sprzątanie po ID |
| T-REG-02 | NOT RUN | PUT/DELETE bez tokenu odrzucone | Tak samo | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono | Brak | Nie dotyczy osłabionego POST |
| T-REG-03 | NOT RUN | Cudze notatki: 404, brak zmian | Tak samo | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono | Brak | Poprawny token konta B |
| T-REG-04 | NOT RUN | Cookies i nagłówki zachowane; lokalny wyjątek CSP | Oryginalna CSP i flagi | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono | Brak | Zależność HSTS od środowiska |

Kroki, dane i sprzątanie: [test-plan.md](test-plan.md). Zmiany: [change-register.md](change-register.md).

## Indeks dowodów

Katalogi [before](evidence/before/) i [after](evidence/after/) zawierają obecnie wyłącznie pliki podtrzymujące strukturę. Nie są to dowody testowe. Nie utworzono screenshotów, nagrań, logów ani fikcyjnych odpowiedzi.

Każdy rzeczywisty wpis powinien zawierać: ID dowodu, test, wariant, SHA, numer próby, datę i czas ze strefą, przeglądarkę, środowisko, ścieżkę pliku, opis obserwacji, zakres anonimizacji i autora rejestracji. Nazwa: `E-<ID-testu>-<BEFORE|AFTER>-R01-01.<rozszerzenie>`. Surowe tokeny, hasła i sesje proxy nie mogą trafić do publicznego repozytorium.

## Granica między ustaleniem a wynikiem

`VERIFIED` w architekturze oznacza odczyt kodu, nie pomiar jego działania. Historyczne README podaje 85/85 testów automatycznych, a `docs/security-audit/` zawiera starsze wyniki i zrzuty. Nie stanowią one dowodu wykonania tej serii i nie są kopiowane do tabeli.

**DO UZUPEŁNIENIA PO TESTACH:** rzeczywiste SHA wariantów, daty i protokoły prób, odpowiedzi, stan danych, przyczyny odrzuceń, zgodność, skuteczność, identyfikatory dowodów oraz ryzyko rezydualne. Do tego czasu nie wolno formułować empirycznej oceny skuteczności AFTER.
