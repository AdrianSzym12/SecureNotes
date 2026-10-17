# Wyniki i rejestr dowodów

Stan na 17.10.2026, 05B. Bazowy SHA: `21be7bb6aadd69c729250eb5d5248cabb114de04`. Użytkownik wykonał dwie próby na BEFORE z CHG-XSS-01 i oryginalną CSP. Pokrywają one części T-XSS-02 i T-XSS-04, oznaczone PARTIAL; nie zaliczamy całych scenariuszy ani porównania AFTER. Pozostałe 13 scenariuszy pozostaje NOT RUN. Codex nie uruchamiał aplikacji ani prób. Pełne wykonanie JavaScript w Stored XSS nie zostało potwierdzone.

`NOT RUN` jest statusem wykonania, a nie wynikiem bezpieczeństwa. Po próbach zgodność oceniać jako zgodny/niezgodny/niejednoznaczny; skuteczność jako skuteczna/nieskuteczna/nieoceniona w danym zakresie. Wykrycie podatności w BEFORE może być zgodne z hipotezą, ale oznacza nieskuteczną kontrolę. Osobne protokoły dla BEFORE i AFTER muszą wskazywać rzeczywiście badany SHA.

## Matryca

| Test | Status | Oczekiwane BEFORE | Oczekiwane AFTER | Zaobserwowane BEFORE / AFTER | Zgodność z oczekiwaniem | Skuteczność zabezpieczenia | Dowód | Uwagi i ograniczenia |
|---|---|---|---|---|---|---|---|---|
| T-XSS-01 | NOT RUN | Zapis i odczyt treści | Zapis i odczyt treści | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono; to kontrola danych | Brak | Porównywać odkodowany JSON |
| T-XSS-02 | PARTIAL — BEFORE, część HTML | Docelowo HTML i wykonanie handlera po CHG-XSS-01/02 | Tekst, brak wykonania | BEFORE: strong z zapisanej treści dało pogrubiony DEMO-XSS-HTML; AFTER: nie badano | Zgodna tylko część interpretacji HTML; wykonanie i pełne porównanie nieocenione | Bezpieczne renderowanie tekstu w BEFORE naruszone; wykonania JS nie potwierdzono | Screenshoty zgłoszone przez użytkownika; archiwizacja niepotwierdzona, bez ID | 17.10.2026, obserwacja użytkownika; CHG-XSS-02 nie wdrożono |
| T-XSS-03 | NOT RUN | Ponowne wykonanie po GET | Ponownie tekst | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono | Brak | Dowód trwałości, nie ręczny DOM |
| T-XSS-04 | PARTIAL — BEFORE | Etap pośredni: HTML, handler blokowany | Tekst, brak wykonania | BEFORE: img powstało, obraz 404, brak alertu, Console wskazała blokadę script-src-attr none; AFTER: nie badano | Część BEFORE zgodna z oczekiwaniem; pełne porównanie nieocenione | CSP skuteczna dla konkretnego demonstracyjnego inline handlera w tej próbie | Screenshoty zgłoszone przez użytkownika; archiwizacja niepotwierdzona, bez ID | 17.10.2026; inny nieszkodliwy payload niż bazowy przykład; zachować commit pośredni |
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

## Obserwacje użytkownika — 17 października 2026

Próba A: treść zapisanej notatki `<strong>DEMO-XSS-HTML</strong>` została wyrenderowana jako pogrubiony DEMO-XSS-HTML. Potwierdza to interpretację HTML w treści po CHG-XSS-01. Przypisanie do T-XSS-02 dotyczy tylko renderowania, nie jego warunku docelowego CHG-XSS-02, wykonania kodu i porównania AFTER.

Próba B: treść zapisanej notatki:

```html
<img src="/xss-demo-nieistniejacy-obraz.png" onerror="alert('XSS DEMO')">
```

Użytkownik zobaczył img; brakujący obraz dał HTTP 404, alert nie wystąpił, a Console wyraźnie potwierdziła zablokowanie inline event handlera przez CSP `script-src-attr 'none'`. Te łączne obserwacje pokrywają część BEFORE T-XSS-04. Nie jest to wyłącznie wniosek z braku alertu. Nie zgłoszono zmiany znacznika data-sn-xss ani innych efektów; faktyczny payload nie zawierał tego znacznika.

Nie potwierdzono osobnego GET z porównaniem odkodowanego content/ID (T-XSS-01), nowego pobrania po przeładowaniu (T-XSS-03), testu po CHG-XSS-02 ani powtórzenia w AFTER. Nie przypisujemy tym zakresom PASS. Istniejące procedury pozostają do dokończenia, a wariant payloadu B zapisujemy jawnie bez zastępowania danych planu po fakcie.

Źródło: relacja użytkownika i zgłoszone screenshoty w rozmowie ChatGPT, nie wykonanie/niezależna inspekcja Codexa. Fizyczna archiwizacja obrazów w repo nie jest potwierdzona; nie przypisano rzeczywistych nazw plików, ścieżek ani ID. Nie utożsamiamy potwierdzenia interpretacji HTML z wykonaniem Stored XSS. Stan kodu utrwala commit etapu 05B `demo(xss): introduce HTML rendering and document CSP blocking` na rodzicu `714973314721918b3b59cb0b81c8fe270822fede`; jego SHA jest identyfikatorem utrwalonego kodu, nie retroaktywnym SHA wcześniejszego uruchomienia.

## Indeks dowodów i stan archiwizacji

Katalogi [before](evidence/before/) i [after](evidence/after/) zawierają obecnie wyłącznie pliki podtrzymujące strukturę. Nie są to dowody testowe. Użytkownik zgłosił screenshoty w rozmowie, ale nie potwierdzono ich archiwizacji; Codex nie tworzył zastępczych obrazów, nagrań, logów ani odpowiedzi. Poniższa konwencja dotyczy przyszłego porządkowania rzeczywistych plików, nie nadania ID obecnym niezarchiwizowanym materiałom.

Każdy rzeczywisty wpis powinien zawierać: ID dowodu, test, wariant, SHA, numer próby, datę i czas ze strefą, przeglądarkę, środowisko, ścieżkę pliku, opis obserwacji, zakres anonimizacji i autora rejestracji. Nazwa: `E-<ID-testu>-<BEFORE|AFTER>-R01-01.<rozszerzenie>`. Surowe tokeny, hasła i sesje proxy nie mogą trafić do publicznego repozytorium.

## Granica między ustaleniem a wynikiem

`VERIFIED` w architekturze oznacza odczyt kodu, nie pomiar jego działania. Historyczne README podaje 85/85 testów automatycznych, a `docs/security-audit/` zawiera starsze wyniki i zrzuty. Nie stanowią one dowodu wykonania tej serii i nie są kopiowane do tabeli.

**DO UZUPEŁNIENIA PO TESTACH:** rzeczywiste SHA wariantów, daty i protokoły prób, odpowiedzi, stan danych, przyczyny odrzuceń, zgodność, skuteczność, identyfikatory dowodów oraz ryzyko rezydualne. Do tego czasu nie wolno formułować empirycznej oceny skuteczności AFTER.
