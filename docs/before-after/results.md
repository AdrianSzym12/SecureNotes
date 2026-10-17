# Wyniki i rejestr dowodów

Stan na 17.10.2026, 06F. Bazowy SHA: `21be7bb6aadd69c729250eb5d5248cabb114de04`. Wyniki użytkownika dotyczą BEFORE: Stored XSS, JSON bez walidacji tokenu oraz formularz cross-origin / same-site. T-XSS-02/04, T-CSRF-A01/A02/A03 i T-CSRF-B01 mają PARTIAL — BEFORE; pozostałe 9 scenariuszy NOT RUN. Nie badano AFTER. Codex nie wykonywał prób ani SQL.

`NOT RUN` jest statusem wykonania, a nie wynikiem bezpieczeństwa. Po próbach zgodność oceniać jako zgodny/niezgodny/niejednoznaczny; skuteczność jako skuteczna/nieskuteczna/nieoceniona w danym zakresie. Wykrycie podatności w BEFORE może być zgodne z hipotezą, ale oznacza nieskuteczną kontrolę. Osobne protokoły dla BEFORE i AFTER muszą wskazywać rzeczywiście badany SHA.

## Matryca

| Test | Status | Oczekiwane BEFORE | Oczekiwane AFTER | Zaobserwowane BEFORE / AFTER | Zgodność z oczekiwaniem | Skuteczność zabezpieczenia | Dowód | Uwagi i ograniczenia |
|---|---|---|---|---|---|---|---|---|
| T-XSS-01 | NOT RUN | Zapis i odczyt treści | Zapis i odczyt treści | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono; to kontrola danych | Brak | Porównywać odkodowany JSON |
| T-XSS-02 | PARTIAL — BEFORE, HTML i wykonanie JS | HTML i wykonanie handlera po CHG-XSS-01/02 | Tekst, brak wykonania | BEFORE: strong w 05B; w 05D alert z zapisanego img/onerror, dokładna treść poniżej; AFTER: nie badano | Wykonanie JS w BEFORE zgodne; pełne porównanie nieocenione | W BEFORE potwierdzono wykonanie Stored XSS; AFTER nieocenione | Screenshoty w rozmowie według użytkownika; archiwizacja niepotwierdzona, bez ID | 17.10.2026, obserwacja użytkownika; niepełny protokół i pełny payload do uzupełnienia |
| T-XSS-03 | NOT RUN | Ponowne wykonanie po GET | Ponownie tekst | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono | Brak | Dowód trwałości, nie ręczny DOM |
| T-XSS-04 | PARTIAL — BEFORE | Etap pośredni: HTML, handler blokowany | Tekst, brak wykonania | BEFORE: img powstało, obraz 404, brak alertu, Console wskazała blokadę script-src-attr none; AFTER: nie badano | Część BEFORE zgodna z oczekiwaniem; pełne porównanie nieocenione | CSP skuteczna dla konkretnego demonstracyjnego inline handlera w tej próbie | Screenshoty zgłoszone przez użytkownika; archiwizacja niepotwierdzona, bez ID | 17.10.2026; inny nieszkodliwy payload niż bazowy przykład; zachować commit pośredni |
| T-CSRF-A01 | PARTIAL — BEFORE | 201 i zapis | 201 i zapis | BEFORE: UI, X-CSRF-TOKEN obecny, HTTP 201; rekord CSRF-TEST-VALID potwierdzony przez użytkownika w MariaDB; AFTER: nie badano | Część BEFORE zgodna; porównanie nieocenione | Kontrola pozytywna; sama nie dowodzi walidacji | Screenshoty w rozmowie, archiwizacja niepotwierdzona, bez ID | 17.10.2026, relacja użytkownika |
| T-CSRF-A02 | PARTIAL — BEFORE | 201 i zapis | 400, brak zapisu | BEFORE: uwierzytelniony JSON z błędnym tokenem; zapis CSRF-TEST-INVALID-1792246089074 potwierdzony przez użytkownika w securenotes_before_lab.notes; kod HTTP udanej próby nieudokumentowany; AFTER: nie badano | Przyjęcie i zapis zgodne; dokładny status i AFTER nieocenione | Brak skutecznej kontroli tokenu na Create BEFORE dla tej próby | Screenshoty w rozmowie, archiwizacja niepotwierdzona, bez ID | Wcześniej 401 o nieustalonej przyczynie, nie wynik Antiforgery |
| T-CSRF-A03 | PARTIAL — BEFORE | 201 i zapis | 400, brak zapisu | BEFORE: uwierzytelniony JSON bez X-CSRF-TOKEN; zapis CSRF-TEST-MISSING-1792246089081 potwierdzony przez użytkownika w securenotes_before_lab.notes; kod HTTP udanej próby nieudokumentowany; AFTER: nie badano | Przyjęcie i zapis zgodne; dokładny status i AFTER nieocenione | Brak skutecznej kontroli tokenu na Create BEFORE dla tej próby | Screenshoty w rozmowie, archiwizacja niepotwierdzona, bez ID | Wcześniej 401 o nieustalonej przyczynie, nie wynik Antiforgery |
| T-CSRF-B01 | PARTIAL — BEFORE | Formularz tworzy notatkę | Brak zapisu; przyczyna do ustalenia | BEFORE: SN-T-CSRF-B01 utworzona, JSON i UI; wariant iframe: SN-T-CSRF-B02 utworzona, UI i rekord MariaDB, główna karta pozostała na stronie demonstracyjnej; AFTER: nie badano | Zapis w BEFORE zgodny; dokładnych kodów HTTP i nagłówków nie udokumentowano; AFTER nieocenione | Tworzenie z innego originu bez tokenu potwierdzone przez użytkownika; nie jest to obejście SameSite ani izolowany pomiar Antiforgery | Screenshoty zgłoszone w rozmowie, archiwizacja niepotwierdzona; bez nazw/ID | 17.10.2026, obie próby dotyczą B01; nazwa notatki B02 nie oznacza testu bez uwierzytelnienia |
| T-CSRF-B02 | NOT RUN | Brak zapisu bez uwierzytelnienia; hipoteza 401 | Brak zapisu; 401 dla właściwej ścieżki lub wcześniejsza odmowa routingu/formatu | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono | Brak | Priorytet: brak zapisu bez Auth; faktyczny kod i przyczyna dopiero po testach |
| T-CSRF-B03 | NOT RUN | Strict: brak Auth i zapisu | Brak zapisu | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono | Brak | Diagnostyka powodu wykluczenia cookie |
| T-CORS-01 | NOT RUN | Brak zgody preflight, bez POST | Tak samo | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono | Brak | Błąd odczytu sam nie wystarcza |
| T-REG-01 | NOT RUN | Poprawny CRUD z tokenami | Poprawny CRUD z tokenami | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono; kontrola funkcjonalna | Brak | Sprzątanie po ID |
| T-REG-02 | NOT RUN | PUT/DELETE bez tokenu odrzucone | Tak samo | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono | Brak | Nie dotyczy osłabionego POST |
| T-REG-03 | NOT RUN | Cudze notatki: 404, brak zmian | Tak samo | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono | Brak | Poprawny token konta B |
| T-REG-04 | NOT RUN | Cookies i nagłówki zachowane; lokalny wyjątek CSP | Oryginalna CSP i flagi | Nie obserwowano / nie obserwowano | Nie oceniono | Nie oceniono | Brak | Zależność HSTS od środowiska |

Kroki, dane i sprzątanie: [test-plan.md](test-plan.md). Zmiany: [change-register.md](change-register.md).

## Historyczne obserwacje 05B — 17 października 2026 (przed CHG-XSS-02)

Próba A: treść zapisanej notatki `<strong>DEMO-XSS-HTML</strong>` została wyrenderowana jako pogrubiony DEMO-XSS-HTML. Potwierdza to interpretację HTML w treści po CHG-XSS-01. Przypisanie do T-XSS-02 dotyczy tylko renderowania, nie jego warunku docelowego CHG-XSS-02, wykonania kodu i porównania AFTER.

Próba B: treść zapisanej notatki:

```html
<img src="/xss-demo-nieistniejacy-obraz.png" onerror="alert('XSS DEMO')">
```

Użytkownik zobaczył img; brakujący obraz dał HTTP 404, alert nie wystąpił, a Console wyraźnie potwierdziła zablokowanie inline event handlera przez CSP `script-src-attr 'none'`. Te łączne obserwacje pokrywają część BEFORE T-XSS-04. Nie jest to wyłącznie wniosek z braku alertu. Nie zgłoszono zmiany znacznika data-sn-xss ani innych efektów; faktyczny payload nie zawierał tego znacznika.

Nie potwierdzono osobnego GET z porównaniem odkodowanego content/ID (T-XSS-01), nowego pobrania po przeładowaniu (T-XSS-03), testu po CHG-XSS-02 ani powtórzenia w AFTER. Nie przypisujemy tym zakresom PASS. Istniejące procedury pozostają do dokończenia, a wariant payloadu B zapisujemy jawnie bez zastępowania danych planu po fakcie.

Źródło: relacja użytkownika i zgłoszone screenshoty w rozmowie ChatGPT, nie wykonanie/niezależna inspekcja Codexa. Fizyczna archiwizacja obrazów w repo nie jest potwierdzona; nie przypisano rzeczywistych nazw plików, ścieżek ani ID. Nie utożsamiamy potwierdzenia interpretacji HTML z wykonaniem Stored XSS. Stan kodu utrwala commit etapu 05B `demo(xss): introduce HTML rendering and document CSP blocking` na rodzicu `714973314721918b3b59cb0b81c8fe270822fede`; jego SHA jest identyfikatorem utrwalonego kodu, nie retroaktywnym SHA wcześniejszego uruchomienia.

## Wynik użytkownika 05D — 17.10.2026

Użytkownik potwierdził rzeczywiste wykonanie JavaScript z zapisanej treści notatki (Stored XSS) na `https://localhost:7028/notes.html` po CHG-XSS-02. Payload używał elementu img i onerror; wyświetlony alert miał dokładnie treść:

> XSS DEMO - Właśnie zostałeś zaatakowany przez Albańskich Hakerów.

Tekst jest humorystycznym elementem bezpiecznego eksperymentu. Nie znamy pełnego źródła payloadu tej próby; nie utożsamiamy go z wcześniejszym payloadem o krótszym tekście alertu i nie dopisujemy brakujących atrybutów ani znacznika data-sn-xss. Obserwację wykonał użytkownik 17.10.2026, nie Codex. Screenshoty są dostępne według użytkownika w rozmowie, lecz fizyczna archiwizacja nie jest potwierdzona; nie przypisano im nazw, ścieżek ani ID dowodów.

CHG-XSS-02: **IMPLEMENTED / PARTIALLY VERIFIED — wykonanie JS w BEFORE potwierdzone przez użytkownika**. T-XSS-02 pozostaje **PARTIAL — BEFORE**, teraz z potwierdzonym wykonaniem JS. Brak pełnego porównania AFTER i pozostałych danych protokołu nie pozwala nadać PASS całemu scenariuszowi. T-XSS-04 pozostaje **PARTIAL — BEFORE** z historycznym wynikiem blokady przy oryginalnej CSP w checkpointcie `9c825ad8d87a63f1134bbfc1e0f64a9b57ea6c11`. Pozostałe 13 scenariuszy nadal **NOT RUN**, w tym T-XSS-01/03 i T-REG-04: nie zgłoszono odrębnego porównania GET/content/ID, próby trwałości po ponownym pobraniu ani pełnego pomiaru nagłówków.

Rzeczywiste nagłówki CSP pozostałych ścieżek, a także zachowanie AFTER wymagają osobnej weryfikacji. Ograniczenie wyjątku do dokładnego `/notes.html` oraz zachowanie pozostałych dyrektyw potwierdza analiza kodu, nie pomiar odpowiedzi HTTP. Nie wykazano dostarczenia payloadu innemu użytkownikowi ani przejęcia konta. Docelowy BEFORE zachowuje podatny renderer i wyjątek CSP; CHG-CSRF-01/02 nadal PLANNED.

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

## Formularz cross-origin — wynik użytkownika 06F, 17.10.2026

Użytkownik uruchomił BEFORE na https://localhost:7028 i serwer demonstracyjny na https://localhost:7443. Pierwotny formularz POST do /api/notes utworzył `SN-T-CSRF-B01`, API zwróciło JSON nowej notatki, a zapis był widoczny w UI. Następnie formularz z target=csrf-result-frame, ukrytym iframe i wyjątkiem frame-src https://localhost:7028 w CSP serwera demonstracyjnego utworzył `SN-T-CSRF-B02`; użytkownik potwierdził pozostanie na stronie demonstracyjnej, nową notatkę w UI i nowy rekord MariaDB. Nie podano dokładnych kodów HTTP ani rzeczywistych nagłówków; nie dopisujemy ich ani konkretnej obserwacji blokady iframe.

Oba przebiegi pokrywają BEFORE T-CSRF-B01. T-CSRF-B02 bada brak uwierzytelnienia i nie został wykonany; T-CSRF-B03 bada inne site i także pozostaje NOT RUN. Strona demonstracyjna nie odczytuje odpowiedzi API, a zapis potwierdzono niezależnie. Ewentualna blokada wyświetlenia odpowiedzi przez frame-ancestors none lub X-Frame-Options DENY nie dowodzi odrzucenia POST. Cross-origin jest potwierdzony przez użytkownika, oba originy pozostają same-site; obejścia SameSite=Strict nie udowodniono. Zewnętrznej domeny i AFTER nie testowano. Pełny PASS nie przysługuje scenariuszowi z niewykonanym porównaniem AFTER.

Źródło: relacja użytkownika z 17.10.2026 i screenshoty zgłoszone w rozmowie, nie wykonanie prób przez Codexa. Fizyczna archiwizacja niepotwierdzona, bez rzeczywistych nazw, ścieżek i ID dowodów. Zanonimizować dane sesji, identyfikatory użytkowników oraz niepotrzebne dane phpMyAdmin przed publikacją. Historyczne obserwacje 05B/05D/06B powyżej pozostają zapisami tamtych etapów. Aktualny bilans: 6 PARTIAL — BEFORE / 9 NOT RUN. Porównania AFTER planuje się na końcową próbę generalną.

## Indeks dowodów i stan archiwizacji

Katalogi [before](evidence/before/) i [after](evidence/after/) zawierają obecnie wyłącznie pliki podtrzymujące strukturę. Nie są to dowody testowe. Użytkownik zgłosił screenshoty w rozmowie, ale nie potwierdzono ich archiwizacji; Codex nie tworzył zastępczych obrazów, nagrań, logów ani odpowiedzi. Poniższa konwencja dotyczy przyszłego porządkowania rzeczywistych plików, nie nadania ID obecnym niezarchiwizowanym materiałom.

Każdy rzeczywisty wpis powinien zawierać: ID dowodu, test, wariant, SHA, numer próby, datę i czas ze strefą, przeglądarkę, środowisko, ścieżkę pliku, opis obserwacji, zakres anonimizacji i autora rejestracji. Nazwa: `E-<ID-testu>-<BEFORE|AFTER>-R01-01.<rozszerzenie>`. Surowe tokeny, hasła i sesje proxy nie mogą trafić do publicznego repozytorium.

## Granica między ustaleniem a wynikiem

`VERIFIED` w architekturze oznacza odczyt kodu, nie pomiar jego działania. Historyczne README podaje 85/85 testów automatycznych, a `docs/security-audit/` zawiera starsze wyniki i zrzuty. Nie stanowią one dowodu wykonania tej serii i nie są kopiowane do tabeli.

**DO UZUPEŁNIENIA PO TESTACH:** rzeczywiste SHA wariantów, daty i protokoły prób, odpowiedzi, stan danych, przyczyny odrzuceń, zgodność, skuteczność, identyfikatory dowodów oraz ryzyko rezydualne. Do tego czasu nie wolno formułować empirycznej oceny skuteczności AFTER.
