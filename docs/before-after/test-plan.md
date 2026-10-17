# Plan testów BEFORE/AFTER

Procedury: PLANNED do dokończenia. Stan 06F, 17.10.2026: **T-XSS-02/04, T-CSRF-A01/A02/A03 i T-CSRF-B01 — PARTIAL — BEFORE; pozostałe 9 scenariuszy NOT RUN**. Wszystkie cztery zmiany wdrożono i częściowo zweryfikowano przez użytkownika w opisanym zakresie BEFORE. AFTER nie badano; porównania wykonamy dopiero podczas końcowej próby generalnej. Bazowy SHA: `21be7bb6aadd69c729250eb5d5248cabb114de04`. Faktyczne wyniki i ograniczenia: [results.md](results.md).

## Próba generalna 07A — plan, bez wykonania

Punkty odniesienia kodu: BEFORE `6d1eddd149fb66b4cde2699da5f5a5399cbc1e43`, AFTER `21be7bb6aadd69c729250eb5d5248cabb114de04`. Przed 07A oba worktree były czyste. Dopisanie tego planu zmienia dokumentację BEFORE, a nie kod; przy przyszłej próbie zapisać rzeczywisty HEAD oraz ewentualny diff. Nie zmieniać czterech CHG ani kodu/config AFTER, nie wprowadzać przełączników. Poniższe oczekiwania są hipotezami; **wyniki nadal 6 PARTIAL — BEFORE / 9 NOT RUN**, zgodnie z [results.md](results.md). Plan nie nadaje wyników AFTER i nie upoważnia do uruchomienia testów w 07A.

### Warunki wspólne, dane i ponowienie

- Po osobnym zleceniu wykonania potwierdzić zgodę na fikcyjne zapisy w istniejącej bazie AFTER i na usuwanie wyłącznie własnych danych tej próby. BEFORE korzysta z securenotes_before_lab, AFTER z istniejącej securenotes_db; securenotes_test_db nie jest używana. Żadnych resetów baz, SQL ani ręcznego uruchamiania migracji. Start Development wywołuje MigrateAsync: przed startem potwierdzić docelową bazę i brak oczekujących zmian schematu w zatwierdzonym środowisku. Nie zmieniać współdzielonych User Secrets ani konfiguracji AFTER.
- A: właściciel fikcyjnych notatek; B: drugie fikcyjne konto wyłącznie do T-REG-03. Przykładowe adresy do przyszłego utworzenia: sn-a@example.test i sn-b@example.test, osobno w każdej bazie, bez rzeczywistych danych. Nie tworzyć ich w 07A; haseł nie zapisywać w dokumentacji. Profile przeglądarki BEFORE-A, BEFORE-B, AFTER-A, AFTER-B oraz jeden czysty profil ANON bez Auth. To osobne profile, nie tylko karty lub okna prywatne współdzielące sesję.
- `RUN=<data-czas-ze-strefą>-Rnn` oznacza przyszły rzeczywisty przebieg, nie już wykonany pomiar. Wybrać kolejny wolny numer w rejestrze, nie nadpisywać starych prób. Tytuł `SN-<ID-testu>-<RUN>`, do 150 znaków. Ta sama treść/title dla danego testu w BEFORE i AFTER, odrębne ID rekordów w odrębnych bazach. W protokole zapisać konto/alias, wariant, ID notatki i stan przed próbą; nie publikować identyfikatorów użytkowników.
- Zwykła treść: `Kontrolna notatka JSON`; edycja: `Tekst po edycji`. T-REG-03 używa własnej zwykłej notatki A i poprawnego tokenu B. XSS używa dokładnego payloadu z sekcji Dane XSS; dodatkowe strong DEMO-XSS-HTML pokazuje tylko interpretację HTML w zakresie T-XSS-02, nie jest nowym scenariuszem. Historycznego tekstu alertu użytkownika nie zmieniać i nie przypisywać mu znacznika DOM z nowego payloadu.
- Dla prób JSON korzystać z originu aplikacji: poprawny Content-Type, ważne Auth/Csrf i świeży token żądania pobrany po logowaniu. Zmieniać tylko wariant nagłówka X-CSRF-TOKEN (poprawny, invalid-lab-token, brak). Nie używać apiRequest w A02/A03. Dla porównania zachować identyczne body; ważne tokeny/cookies muszą pochodzić z sesji właściwej wersji, nie być kopiowane między wersjami.
- Włączyć Preserve log i Disable cache, pokazywać Document oraz API/OPTIONS. Po każdym żądaniu zmieniającym dane wykonać nowy GET/listę i porównać ID/treść. Zamknąć alert i przeładować cały dokument, aby usunąć poprzedni data-sn-xss. Używać nowego konta/wyłącznie własnych świeżych danych zamiast kasowania starych notatek z payloadem; wiele aktywnych notatek XSS powoduje wiele alertów.
- Sprzątanie: prowadzić listę ID utworzonych w tym RUN. Dopiero po zachowaniu dowodów A usuwa te konkretne rekordy przez UI/DELETE z poprawnym świeżym tokenem, po sprawdzeniu ID i własności; potwierdzić GET. Nie usuwać automatycznie danych AFTER, danych historycznych, wszystkich rekordów z prefiksem, kont ani baz. Przy nieznanym właścicielu/ID przerwać sprzątanie i wyjaśnić. Przy powtórzeniu nowy RUN/ID; odrzucenia dokumentować także jako „brak danych do usunięcia”.

### Kolejność blokami i izolacja

Przed startem rozstrzygnąć trzy warunki szczególne poniżej. Następnie cały blok docelowego BEFORE, jedno przełączenie, cały blok AFTER; w każdym bloku kolejno:

1. T-REG-01 — zwykły CRUD i kontrola sprawności.
2. T-REG-04 — flagi cookies i nagłówki bez aktywnego payloadu; sesję utrzymać do końca bloku, wylogowanie wykonać przy jego zamknięciu.
3. T-XSS-01 — zapis/GET payloadu; po dowodach sprzątnąć.
4. T-XSS-02 — HTML oraz wykonanie; po dowodach sprzątnąć.
5. T-XSS-03 — nowy zapis z własnym ID, przeładowanie bez ponownego POST; sprzątnąć.
6. T-CSRF-A01 — kontrola poprawnego tokenu.
7. T-CSRF-A02 — token błędny.
8. T-CSRF-A03 — brak tokenu.
9. T-CSRF-B01 — natywny formularz z localhost:7443 do 7028, docelowo ukryty iframe, nie fetch.
10. T-REG-02 — PUT/DELETE bez tokenu.
11. T-REG-03 — konto B przeciw własnej notatce A, potem kontrola A.
12. T-CORS-01 — warunkowo, z przygotowanego originu diagnostycznego 7443, nie z obecnej blokującej CSP.
13. T-CSRF-B03 — warunkowo, formularz jako najwyższa strona z HTTPS 127.0.0.1:7443.
14. T-CSRF-B02 — odrębny profil ANON bez Auth; kontrolny odczyt danych w profilu A.
15. T-XSS-04 — porównanie zachowanego pomiaru checkpointu pośredniego z częścią AFTER; ewentualne odtworzenie checkpointu to osobny blok, nigdy przełączanie docelowego BEFORE.

To 15 istniejących ID, nie nowe scenariusze. T-REG-04 można zakończyć po sprawdzeniu nagłówków na początku i sesji na końcu. Dowód AFTER dla T-XSS-04 można współdzielić z odpowiednim odczytem T-XSS-02, oznaczając zakres zamiast udawać dwa pomiary. Serwery diagnostyczne 7443 uruchamiać kolejno i zatrzymywać między ich odmianami.

**Przełączenie BEFORE → AFTER:** zachować i zanonimizować dowody, zakończyć własne testowe zapisy, wylogować A/B z poprawnym tokenem, zamknąć karty 7028/7443, zatrzymać BEFORE oraz demo/diagnostykę, potwierdzić zwolnienie 7028 i 7443. Poczekać na zakończenie procesu start-before i jego finally. Zamknąć konsolę z procesowymi Api__*; AFTER uruchamiać własnym dotychczasowym profilem z czystej sesji (np. niezależnego Visual Studio), bez dziedziczenia bazy/hasła BEFORE. Użyć AFTER-A/B; przy ponownym użyciu profilu najpierw usunąć dane localhost i zalogować się od nowa. Cookies nie izolują portów, więc samo zamknięcie karty nie wystarcza. Ponownie potwierdzić ścieżkę procesu, SHA, HTTPS 7028, bazę AFTER, /me i świeży token; nie uruchamiać obu aplikacji równocześnie. Nigdy nie kopiować JWT/tokenów z BEFORE.

### Warunki szczególne — bez zmieniania definicji testów

- **T-XSS-04:** dotyczy wyłącznie checkpointu `9c825ad8d87a63f1134bbfc1e0f64a9b57ea6c11` (innerHTML + oryginalna CSP). Najpierw spróbować zarchiwizować prawdziwe wcześniejsze materiały użytkownika wraz z kontekstem/diffem; nie zastępować ich wynikiem docelowego BEFORE ani screenshotem samego kodu. Jeżeli brak pełnego pomiaru, nadal PARTIAL. Powtórzenie wymaga osobno zatwierdzonego odłączonego worktree tego SHA oraz odrębnej izolacji danych i startu; istniejący start-before odrzuca detached HEAD. W 07A niczego nie tworzymy i nie obchodzimy kontroli. Nie resetować żadnego docelowego worktree, nie dodawać przełącznika podatności.
- **T-CSRF-B03:** obecny serwer demo odrzuca Host 127.0.0.1, a certyfikat localhost nie potwierdza certyfikatu dla IP. B03 wymaga uprzednio zatwierdzonego, osobnego lokalnego stanowiska diagnostycznego poza kodem obu aplikacji: HTTPS na loopback 127.0.0.1:7443, zaufany certyfikat z IP SAN, formularz najwyższego dokumentu kierowany bezpośrednio do localhost:7028, bez przekierowań i bez logowania sesji. Nie modyfikować bieżącego serwera demo ani cookies SecureNotes. Przy braku takiego stanowiska B03 pozostaje NOT RUN z jawną przyczyną; błąd TLS/Host nie dowodzi działania Strict.
- **T-CORS-01:** CSP obecnego demo ma default-src none i nie dopuszcza connect-src, więc może zatrzymać fetch przed OPTIONS. Nie zaliczać tego jako CORS. Przed próbą przygotować i osobno zatwierdzić kontrolowaną stronę diagnostyczną na tym samym originie localhost:7443, która dopuszcza połączenie do localhost:7028 i wykonanie pojedynczego żądania JSON (np. ręcznie w konsoli), bez zapisu Cookie. Może być częścią osobnego stanowiska powyżej, uruchamianego kolejno na 7443; bez zmian kodu/config obu SecureNotes i bez osłabiania bieżącego demo. W 07A tylko specyfikacja, bez tworzenia hosta/plików/certyfikatów. Jeśli nieprzygotowane, test NOT RUN; nie wyłączać CSP przeglądarki i nie używać no-cors, file:// ani originu aplikacji jako zamiennika.

### Macierz wszystkich 15 scenariuszy — procedury i hipotezy

W poniższych wierszach obowiązują wspólne warunki/dane RUN i zasady oceny. Numer ID i cel pozostają zgodne z definicjami poniżej. „Próba” nie oznacza wykonania w 07A.

| ID i cel | Warunki, konto i dane | Procedura BEFORE → oczekiwanie | Procedura AFTER → oczekiwanie | Wymagany dowód i kryterium szczegółowe | Bezpieczne ponowienie |
|---|---|---|---|---|---|
| T-XSS-01 — zapis i odczyt | A, token poprawny, własny tytuł ID/RUN, payload z Dane XSS | POST JSON, zapisać ID, niezależny GET po ID → 201, GET 200, odkodowane content identyczne | Identyczne body i własny token AFTER → 201/GET 200 i ta sama treść | POST i GET/ID/content; PASS tylko przy zgodności w obu wariantach, nie na podstawie odpowiedzi POST lub escape w surowym JSON | Nowy tytuł/ID; po odczycie usunąć tylko ten rekord |
| T-XSS-02 — HTML a wykonanie | A, nowa notatka payloadu, świeży Document; pomocniczo osobne strong | Zapisać, nowy GET/Document, zbadać DOM i CSP → img, alert i automatyczny data-sn-xss=executed; strong daje HTML | Ten sam payload, nowy Document/GET → dosłowny tekst, brak img z treści/alertu/znacznika | Treść GET, CSP, DOM, nagranie alertu/znacznika; PASS wymaga obu wariantów i związku efektu z zapisem. HTML bez JS to częściowy zakres | Zamknąć alert; usunąć ID, przeładować cały dokument, nowy RUN bez starego znacznika |
| T-XSS-03 — trwałość | A, odrębny zapis/ID z tym samym payloadem | Zamknąć formularz, przeładować lub nowa karta, nowy GET bez nowego POST → ponowne wykonanie | Tak samo → ponownie tekst bez wykonania | Nagranie nowego Document/GET → to samo ID/content → DOM/efekt; samo odświeżenie listy bez dowodu pobrania nie wystarcza | Zachować ID przez pomiar trwałości, potem usunąć i potwierdzić brak; nowe ID dla nowego RUN |
| T-XSS-04 — rola samej CSP | Checkpoint pośredni, A i zapis payloadu; warunek szczególny powyżej | Wyłącznie historyczny/odtworzony checkpoint: GET, Document/CSP, img i błąd obrazu → blokada onerror przez script-src-attr none | Zwykły AFTER, ten sam payload → tekst i brak wykonania; brak komunikatu CSP jest dopuszczalny | Pełny SHA/diff, GET/DOM, nagłówek Document i komunikat CSP łącznie; docelowy BEFORE nie zastępuje etapu. Brak materiału = PARTIAL | Ponowienie tylko na zatwierdzonym oddzielnym stanowisku checkpointu; bez cofania kodu docelowego; własne dane |
| T-CSRF-A01 — kontrola poprawnego tokenu | A po logowaniu, świeży token, Auth/Csrf, poprawny JSON ID/RUN | Same-origin POST z poprawnym X-CSRF-TOKEN, następnie GET → 201 i zapis | Własny token AFTER, identyczne body → 201 i zapis | Network z zamazanym tokenem, status i nowy GET/ID. To kontrola funkcjonalna, akceptacja BEFORE nie dowodzi walidacji | Nowy znacznik; po próbie poprawny DELETE, token ponownie po zmianie konta |
| T-CSRF-A02 — błędny token | Udany A01 w tej sesji, cookie Csrf pozostaje, invalid-lab-token, poprawne body | Same-origin JSON z błędnym nagłówkiem → hipoteza 201 i zapis | Identyczny JSON, ważna sesja, błędny nagłówek → hipoteza 400 Antiforgery i brak zapisu | Faktyczny status, kontrola A01, żądanie i lista przed/po; przyczynę odmowy ustalić z bezpiecznej diagnostyki/dostępnych informacji. Sam 400 nie daje PASS kontroli | Nowy znacznik, kasować tylko ewentualny własny zapis; przy 401 odtworzyć sesję i nowa próba |
| T-CSRF-A03 — brak tokenu | Jak A01, cookie Csrf zostaje, brak nagłówka i pola tokenu | Same-origin JSON bez X-CSRF-TOKEN → hipoteza 201 i zapis | Ten sam kontrakt i ważna sesja → hipoteza 400 Antiforgery i brak zapisu | Faktyczny brak nagłówka, poprawny JSON/A01, stan przed/po i przyczyna odmowy; 401 nie jest wynikiem CSRF | Nowy znacznik i własny poprawny DELETE, bez używania apiRequest do wysyłki |
| T-CSRF-B01 — formularz innego originu, same-site | A na 7028, bieżący demo 7443 w tym samym profilu, URL-encoded title/content, bez tokenu | Kliknąć natywny formularz do iframe, sprawdzić POST oraz nowy GET w aplikacji → oczekiwane 201 i zapis A | Ta sama strona/body, własna sesja AFTER → brak zapisu, status/przyczyna do ustalenia | Pasek 7443/Origin, request/form data, cookies bez wartości, ID i świeży GET. Odmowa przez format/routing nie jest izolacją Antiforgery; blokada wyświetlenia w iframe nie oznacza odmowy POST | Nowy tytuł i ID; strona nie ogłasza sukcesu; usuwać tylko własny zapis; SN-T-CSRF-B02 z historii to nadal przebieg B01 |
| T-CSRF-B02 — formularz bez uwierzytelnienia | ANON bez Auth/Authorization, osobny profil A do kontroli, unikalny tytuł | Formularz z 7443 bez uwierzytelnienia → brak zapisu, hipoteza 401 | Tak samo → brak zapisu; możliwa wcześniejsza odmowa formatu/routingu | Potwierdzony brak Auth i sprawność A01, faktyczny status/przyczyna, kontrola listy A. Brak zapisu jest głównym kryterium; odmowa formatu nie dowodzi sama Authorize | Utrzymywać ANON odrębnie, nie wylogowywać A przypadkowo; nieoczekiwany rekord zabezpieczyć i ustalić właściciela |
| T-CSRF-B03 — kontrola cross-site | A z ważną sesją localhost; osobne stanowisko IP/certyfikat opisane wyżej | Formularz z najwyższej strony HTTPS IP do localhost → oczekiwane wykluczenie Auth przez Strict i brak zapisu | Powtórzyć dla własnej sesji AFTER → brak zapisu; przyczynę rozdzielić od formatu/auth | Origin IP, powód wykluczenia cookie z DevTools, status i brak nowego rekordu; brak sesji od początku albo błąd TLS = niewykonana/niepełna próba | Nowy RUN, bez zmiany SameSite; zakończyć host IP przed powrotem do demo, własne dane |
| T-CORS-01 — preflight JSON | A, origin diagnostyczny 7443 z dozwolonym connect-src, świeży cache preflight, poprawny JSON bez tokenu | Z originu 7443 fetch JSON z credentials include → oczekiwany OPTIONS bez zgody CORS, brak POST i zapisu | Identyczny kontrakt → to samo oczekiwanie | OPTIONS request/response i brak właściwego POST/rekordu; sam błąd fetch albo blokada CSP nie potwierdza CORS. Status OPTIONS zanotować, nie zakładać | Nowy profil/cache preflight i nowy RUN, sprawna sesja; usunąć wyłącznie ewentualny nieoczekiwany zapis |
| T-REG-01 — zwykły CRUD | A, zwykła treść, ważne tokeny, ID/RUN | UI: utwórz → odczytaj → edytuj → przeładuj → usuń → GET → hipoteza 201/200/200/204 i brak danych po usunięciu | Ta sama sekwencja → identyczna funkcjonalność | Jedno ID, content po edycji, odpowiedzi i końcowy odczyt; pełny CRUD w obu wariantach | Delete jest częścią testu; przerwanie pozostawia ID w rejestrze do świadomego sprzątania |
| T-REG-02 — PUT/DELETE bez tokenu | A, własna zwykła notatka, Auth/Csrf, poprawne body PUT | PUT bez tokenu → GET, DELETE bez tokenu → GET → hipoteza 400/400 i brak zmian | Identyczne kroki → ta sama hipoteza | Oba żądania/statusy, niezmieniony content/istnienie po każdym, poprawna sesja/format. Odmowa modelu nie rozstrzyga tokenu | Poprawny token tylko do utworzenia/sprzątania; własne ID i nowy RUN |
| T-REG-03 — cudze notatki | A/B w odrębnych profilach; A tworzy zwykłą notatkę, B ma jej ID i własny poprawny token | B: GET/PUT/DELETE ID A; potem A: GET → hipoteza 404/404/404, oryginał niezmieniony | To samo we własnej bazie AFTER → identyczne oczekiwanie | Kontrola tożsamości przez /me bez publikacji ID użytkownika/JWT, udany odczyt A przed/po, trzy odpowiedzi B. Błędny token B maskuje cel | A usuwa własne ID, B niczego nie sprząta za A; nowe ID przy powtórzeniu |
| T-REG-04 — cookies i zakres nagłówków | A, świeże logowanie/token, znane środowisko i brak payloadu | Auth/Csrf flagi i odpowiedzi notes.html, /, login.html, register.html, api/notes → Strict/Secure/HttpOnly, Path=/, wyjątek script-src-attr tylko notes.html, API no-store | Te same zasoby → wszędzie script-src-attr none i zachowane inne nagłówki/flagi | Tabela zasób→CSP/nosniff/DENY/no-referrer/Permissions-Policy; wartości cookies ukryte. HSTS ocenić względem środowiska; brak w Development nie jest FAIL | GET bez nowych notatek; wylogowanie na końcu bloku, świeży profil/token w następnym wariancie |

### Ocena i materiały

**PASS** oznacza zgodność przeprowadzonego pełnego scenariusza z oczekiwaniami obu wariantów, z wymaganymi dowodami i rozstrzygniętą przyczyną; oddzielnie zapisać skuteczność zabezpieczenia (wykonanie ataku w BEFORE nie oznacza bezpiecznej aplikacji). **FAIL**: przy poprawnych warunkach uzyskano udokumentowaną sprzeczność z oczekiwaniem, np. wykonanie XSS w AFTER, nieautoryzowany zapis lub modyfikację cudzej notatki; nie ukrywać jej pod PARTIAL. **PARTIAL**: zbadano tylko wariant/część celu lub nie ustalono przyczyny odmowy/kompletu dowodów. **NOT RUN**: nie wykonano właściwej próby, w tym przez brak hosta IP/TLS/stanowiska CORS; awaria nie dowodzi skuteczności ochrony. Oczekiwanego kodu nigdy nie wpisywać jako rzeczywiście zmierzonego.

Archiwizować dopiero prawdziwe, zanonimizowane pliki w [evidence/before](evidence/before/) i [evidence/after](evidence/after/). Przyszła nazwa: `E-<ID-testu>-<BEFORE|AFTER>-<RUN>-<nn>.<png|mp4|txt>`; dla T-XSS-04 użyć BEFORE i dopisać checkpoint w protokole. To propozycja, nie lista istniejących plików. Wpis musi zawierać ID, wariant/rolę checkpointu, datę/czas/strefę, wynik rzeczywisty, nazwę faktycznie zapisanego pliku, opis, pełny SHA/diff, przeglądarkę, alias konta, status i przyczynę, kontrolę danych przed/po, anonimizację i sprzątanie. Nie publikować Cookie/Set-Cookie, Authorization/JWT, tokenów CSRF, haseł, ID użytkowników ani surowego HAR/pełnych logów. Wartości można zasłonić, zachowując nazwy nagłówków, flagi i diagnostykę. Bez fizycznych plików pozostają miejsca do uzupełnienia; nie przypisywać ID screenshotom z rozmowy.

Szacunek organizacyjny: przygotowanie 10–15 min, BEFORE 35–45 min, przełączenie 5–10 min, AFTER 35–45 min, selekcja/anonimizacja 20–30 min: **105–145 min** dla gotowego głównego stanowiska. Dodatkowe bloki checkpointu/IP/CORS: orientacyjnie **30–60 min** po przygotowaniu, łącznie około **135–205 min**; samo rozwiązanie brakujących certyfikatów/hostów jest poza tą estymacją. Nie obiecywać pełnych 15 pomiarów, jeśli trzy warunki szczególne nie są spełnione. Prezentacja pozostaje osobnym 10–12-minutowym skrótem z rzeczywistymi dowodami, a nie pełną serią na żywo.

## Zasady wspólne

Przed próbą zapisać SHA faktycznie badanego wariantu, wersję przeglądarki, środowisko, originy, profil użytkownika i numer próby. Warunki przygotować według [environment.md](environment.md). Nie uruchamiać obu wariantów równocześnie z tym samym profilem i cookies. Potwierdzić laboratorium i skutki automatycznych migracji przed startem.

Każdy test zaczyna się od listy identyfikatorów własnych notatek. Tytuł ma prefiks `SN-<ID-testu>-R01`; treść nie zawiera danych rzeczywistych. W testach tokenu utrzymać poprawny JSON, ważne uwierzytelnienie oraz cookie Antiforgery; zmieniać tylko nagłówek tokenu. Poprawny token pobrać po zalogowaniu. Nie używać `apiRequest` do prób brakującego/błędnego tokenu, ponieważ automatycznie dodaje prawidłowy token. Można wykonać bezpośredni `fetch` z originu aplikacji i przechowywać token tylko w pamięci, bez wypisywania jego wartości.

Dowody oznaczać `E-<ID-testu>-<wersja>-R01-01`. Zrzuty i eksporty muszą być zanonimizowane; nie publikować surowego HAR ani cookies. Zawsze potwierdzać zmianę lub brak zmiany przez ponowny odczyt API. Status HTTP sam nie dowodzi przyczyny. Sprzątanie odbywa się przez API właściciela z poprawnym tokenem; usuwać tylko zapisane identyfikatory próby. Brak danych do usunięcia również odnotować.

**Zgodność eksperymentu** i **skuteczność zabezpieczenia** oceniamy osobno. Wykonanie payloadu w BEFORE może potwierdzać hipotezę eksperymentu, ale oznacza nieskuteczne zabezpieczenie dla tej ścieżki, nie pozytywną ocenę bezpieczeństwa.

## Sześć testów głównych do prezentacji

Zgodnie z kartą projektu wybrano 3–6 kluczowych testów; tutaj sześć: **T-XSS-02, T-CSRF-A01, T-CSRF-A02, T-CSRF-A03, T-CSRF-B01, T-REG-04**. T-XSS-01 i T-XSS-03 dostarczają warunków i dowodu trwałości dla pokazu XSS. Pozostałe testy są pomocnicze lub regresyjne. W 11-minutowej prezentacji pokazujemy wybrane kroki na żywo, a pełne wyniki serii w tabeli, jeżeli rzeczywiście zostaną uzyskane.

## Dane XSS

Tytuł: znacznik testu. Treść:

```html
<img src="/__sn_xss_probe_missing__.png" onerror="document.documentElement.dataset.snXss='executed';alert('SN-XSS-01')">
```

Potwierdzić, że adres obrazu nie istnieje. Payload ustawia lokalny atrybut DOM i pokazuje alert; nie odczytuje sekretów ani nie wysyła danych. Nie używamy znacznika `script` jako dowodu wykonania przez `innerHTML`, ponieważ jego samo wstawienie nie zapewnia wykonania.

### T-XSS-01 — zapis i odczyt danych (pomocniczy)

1. **Cel:** sprawdzić zachowanie treści w zapisie i ponownym odczycie, niezależnie od renderera.
2. **Warunki:** konto A zalogowane, poprawny token, działające laboratorium, brak notatki tej próby.
3. **Kroki:** wysłać POST JSON; zapisać ID; wykonać GET `/api/notes/{id}` i porównać odkodowaną treść JSON z wejściem. Nie oceniać trwałości na podstawie samej odpowiedzi POST.
4. **Dane:** payload powyżej i tytuł `SN-T-XSS-01-R01`.
5. **BEFORE:** oczekiwana odpowiedź 201 i zachowana treść w GET.
6. **AFTER:** takie samo zachowanie zapisu; bezpieczna aplikacja może przechowywać dosłowny tekst HTML.
7. **Potwierdzenie:** zgodność pola `content` po odkodowaniu JSON, nie identyczność zapisu znaków escape w surowym JSON.
8. **Dowody:** zanonimizowany POST/GET, ID, porównanie treści.
9. **Przywrócenie:** usunąć notatkę właściciela z poprawnym tokenem; potwierdzić brak w liście.

### T-XSS-02 — interpretacja i wykonanie (główny)

**Stan 05D: PARTIAL — BEFORE.** Po wcześniejszym potwierdzeniu HTML w 05B użytkownik 17.10.2026 potwierdził w 05D wykonanie JS z zapisanego img/onerror na `/notes.html` i alert `XSS DEMO - Właśnie zostałeś zaatakowany przez Albańskich Hakerów.`. To wynik BEFORE, nie pełny PASS: brak porównania AFTER i pełnego protokołu, w tym pełnego źródła faktycznego payloadu oraz archiwizacji. Nie dopisujemy efektu data-sn-xss. Poniższa docelowa procedura nie jest zapisem wykonanych kroków; wyniki i ograniczenia: [results.md](results.md).

1. **Cel:** odróżnić interpretację HTML od wykonania JavaScript i porównać warianty.
2. **Warunki:** notatka z payloadem zapisana; docelowe BEFORE ma CHG-XSS-01/02; AFTER pozostaje oryginalne. Przed próbą przeładować dokument, aby usunąć wcześniejszy znacznik DOM.
3. **Kroki:** otworzyć `/notes.html`; zarejestrować CSP odpowiedzi dokumentu; obejrzeć DOM treści, alert i atrybut `data-sn-xss`; sprawdzić komunikaty CSP. Powtórzyć w drugim wariancie z tym samym payloadem.
4. **Dane:** payload XSS, tytuł `SN-T-XSS-02-R01`.
5. **BEFORE:** element `img` i wykonanie handlera; alert oraz atrybut `executed`.
6. **AFTER:** widoczny dosłowny tekst payloadu, brak elementu z tej treści, brak alertu i znacznika wykonania.
7. **Potwierdzenie:** sam element dowodzi tylko HTML; alert i znacznik ustawiony automatycznie po odczycie dowodzą wykonania. Nie ustawiać znacznika ręcznie w konsoli.
8. **Dowody:** DOM, nagłówek CSP, nagranie/alert, odpowiedź API z treścią; pomocniczo konsola.
9. **Przywrócenie:** usunąć notatkę; przeładować dokument i potwierdzić brak wykonania.

### T-XSS-03 — trwałość po ponownym pobraniu (pomocniczy)

1. **Cel:** wykazać, że źródłem jest zapis, a nie chwilowy stan formularza lub ręczna modyfikacja DOM.
2. **Warunki:** zapisana notatka XSS, odnotowane ID, zamknięte alerty.
3. **Kroki:** przeładować dokument lub otworzyć nową kartę w tej samej sesji; potwierdzić nowy GET; bez wpisywania payloadu obserwować efekt.
4. **Dane:** notatka `SN-T-XSS-03-R01` z identycznym payloadem.
5. **BEFORE:** ponowne wykonanie po odczycie zapisanej treści.
6. **AFTER:** ponownie tekst, brak wykonania.
7. **Potwierdzenie:** kolejność GET → budowa DOM → efekt, przy tym samym ID notatki.
8. **Dowody:** nagranie od przeładowania, Network i DOM.
9. **Przywrócenie:** usunąć własną notatkę i przeładować; zanotować zakończenie próby.

### T-XSS-04 — rola samej CSP (pomocniczy, etap pośredni)

**Stan 05B: PARTIAL — część BEFORE zgodna.** Użytkownik 17.10.2026 użył lokalnego img z onerror alert('XSS DEMO'), opisanego dokładnie w [results.md](results.md). Potwierdził img, 404 obrazu, brak alertu i komunikat blokady script-src-attr none. To wariant nieszkodliwych danych wejściowych dla istniejącego scenariusza, nie nowy ID. Brak potwierdzonego AFTER i zarchiwizowanych materiałów; pełnego PASS nie przypisano. Zachować commit pośredni, a przy dokończeniu jawnie dokumentować faktyczny payload i porównanie.

1. **Cel:** sprawdzić, czy oryginalna CSP blokuje handler mimo interpretacji HTML.
2. **Warunki:** wyłącznie pośrednie BEFORE z CHG-XSS-01, jeszcze bez CHG-XSS-02: renderer `innerHTML` z oryginalną CSP. Koniecznie zachować pomiar i SHA tego stanu pośredniego przed dalszymi zmianami; dla niezatwierdzonych zmian sam SHA nie wystarcza i należy dodatkowo zachować dokładny diff. Nie zastępować pomiaru dowodem z docelowego BEFORE i nie cofać kodu AFTER.
3. **Kroki:** zapisać payload, przeładować `/notes.html`, sprawdzić element `img`, brak efektu handlera i komunikat naruszenia CSP.
4. **Dane:** `SN-T-XSS-04-R01` i payload XSS.
5. **BEFORE pośrednie:** HTML interpretowany, handler blokowany. Docelowe BEFORE po wyjątku nie jest przedmiotem tej próby.
6. **AFTER:** tekst, brak wykonania; komunikat blokady handlera nie jest wymagany, ponieważ handler nie powstaje.
7. **Potwierdzenie:** łącznie DOM i polityka `script-src-attr 'none'`; brak alertu sam nie dowodzi CSP.
8. **Dowody:** SHA etapu, CSP dokumentu, DOM i konsola.
9. **Przywrócenie:** usunąć notatkę; zakończyć pomiar przed przejściem do następnej planowanej zmiany. Bez resetów referencyjnego worktree.

## Doświadczenie A — JSON i Antiforgery

W trzech testach użyć `POST /api/notes`, `Content-Type: application/json` i tego samego body w porównywanych wariantach, np. `{"title":"SN-T-CSRF-A02-R01","content":"Kontrolna notatka JSON"}`. Cookies i poprawny token są właściwe dla danej sesji, nie muszą być identyczne bajtowo. Cookie Antiforgery pozostaje obecne także w próbie brakującego tokenu żądania.

### T-CSRF-A01 — poprawny token (główny)

**Stan 06B: PARTIAL — BEFORE.** 17.10.2026: UI z X-CSRF-TOKEN, HTTP 201 i rekord CSRF-TEST-VALID w MariaDB potwierdzone przez użytkownika. Nie wykonano porównania AFTER; pełnego PASS nie nadano. Nazwy rzeczywistych notatek różnią się od planowanych znaczników poniżej; nie zmieniamy procedury po fakcie. Szczegóły i ograniczenia: [results.md](results.md).

1. **Cel:** potwierdzić poprawność ścieżki pozytywnej i warunków porównania.
2. **Warunki:** konto A zalogowane, świeży token po logowaniu, poprawny JSON, BEFORE z CHG-CSRF-01.
3. **Kroki:** pobrać token bez wypisywania; wysłać bezpośredni fetch tego samego originu z cookie i nagłówkiem `X-CSRF-TOKEN`; odczytać notatkę po ID. Powtórzyć w AFTER.
4. **Dane:** tytuł `SN-T-CSRF-A01-R01`, treść kontrolna.
5. **BEFORE:** 201 i zapis; przy braku walidacji akceptacja nie dowodzi sprawdzania tokenu.
6. **AFTER:** 201 i zapis.
7. **Potwierdzenie:** odczyt notatki i zgodność właściciela przez widoczność na koncie A.
8. **Dowody:** struktura żądania z zamazaną wartością tokenu, odpowiedź i GET.
9. **Przywrócenie:** usunąć utworzoną notatkę z poprawnym tokenem.

### T-CSRF-A02 — błędny token (główny)

**Stan 06B: PARTIAL — BEFORE.** 17.10.2026: Uwierzytelniony JSON z błędnym tokenem zapisał CSRF-TEST-INVALID-1792246089074 w securenotes_before_lab.notes według użytkownika. Kod HTTP udanej próby nieudokumentowany. Nie wykonano porównania AFTER; pełnego PASS nie nadano. Nazwy rzeczywistych notatek różnią się od planowanych znaczników poniżej; nie zmieniamy procedury po fakcie. Wcześniejsze 401 miały nieustaloną przyczynę i nie dowodzą walidacji CSRF. Szczegóły i ograniczenia: [results.md](results.md).

1. **Cel:** odizolować reakcję na niepoprawny token żądania.
2. **Warunki:** jak A01; poprawna sesja i cookie Antiforgery pozostają bez zmian.
3. **Kroki:** wysłać poprawny JSON z `X-CSRF-TOKEN: invalid-lab-token`; sprawdzić status, źródło odrzucenia i listę danych przed/po; powtórzyć w AFTER.
4. **Dane:** `SN-T-CSRF-A02-R01`, treść kontrolna, stały niesekretny błędny token.
5. **BEFORE:** 201 i zapis po usunięciu walidacji.
6. **AFTER:** oczekiwane 400 z powodu Antiforgery i brak zapisu.
7. **Potwierdzenie:** prawidłowe body, ważna tożsamość i porównanie z A01; w razie niejasności diagnostyka kategorii błędu bez sekretów. Nie przypisywać dowolnego 400 Antiforgery.
8. **Dowody:** żądanie, odpowiedź, bezpieczny komunikat diagnostyczny jeżeli potrzebny, lista notatek.
9. **Przywrócenie:** usunąć ewentualną notatkę BEFORE; w AFTER potwierdzić, że nie ma danych do usunięcia.

### T-CSRF-A03 — brak tokenu (główny)

**Stan 06B: PARTIAL — BEFORE.** 17.10.2026: Uwierzytelniony JSON bez nagłówka X-CSRF-TOKEN zapisał CSRF-TEST-MISSING-1792246089081 w securenotes_before_lab.notes według użytkownika. Kod HTTP udanej próby nieudokumentowany. Nie wykonano porównania AFTER; pełnego PASS nie nadano. Nazwy rzeczywistych notatek różnią się od planowanych znaczników poniżej; nie zmieniamy procedury po fakcie. Wcześniejsze 401 miały nieustaloną przyczynę i nie dowodzą walidacji CSRF. Szczegóły i ograniczenia: [results.md](results.md).

1. **Cel:** odizolować brak tokenu żądania.
2. **Warunki:** jak A01; cookie Antiforgery obecne, brak tokenu w nagłówku i formularzu.
3. **Kroki:** wysłać ten sam strukturalnie JSON bez `X-CSRF-TOKEN`; zachować ważne uwierzytelnienie; sprawdzić status i dane; powtórzyć w AFTER.
4. **Dane:** `SN-T-CSRF-A03-R01`, treść kontrolna.
5. **BEFORE:** 201 i zapis.
6. **AFTER:** oczekiwane 400 z powodu Antiforgery i brak zapisu.
7. **Potwierdzenie:** Network dowodzi braku tokenu; A01 stanowi kontrolę poprawności pozostałych warunków.
8. **Dowody:** zanonimizowane nagłówki, odpowiedź i stan danych.
9. **Przywrócenie:** usunąć tylko ewentualny zapis tej próby z poprawnym tokenem.

## Doświadczenie B — formularz w przeglądarce

W etapie 06D przygotowano [lokalny serwer i stronę demonstracyjną](../../scripts/lab/csrf-demo/README.md); w 06F zapisano rzeczywiste wyniki użytkownika T-CSRF-B01 (PARTIAL — BEFORE), zarówno z nawigacją głównej karty, jak i ukrytego iframe. Aktualny formularz ma widoczne pola title/content, target=csrf-result-frame oraz świadomy przycisk, bez JavaScript i tokenu. Poniższy przykład pozostaje specyfikacją pierwotnego żądania, nie dokładnym protokołem ani bieżącym HTML:

```html
<form method="post" action="https://localhost:7028/api/notes"
      enctype="application/x-www-form-urlencoded">
  <input type="hidden" name="Title" value="SN-T-CSRF-B01-R01">
  <input type="hidden" name="Content" value="Kontrolna notatka z formularza">
  <button type="submit">Uruchom demonstrację</button>
</form>
```

### T-CSRF-B01 — inny origin, ten sam site (główny)

**Stan 06F: PARTIAL — BEFORE.** Użytkownik 17.10.2026 potwierdził `SN-T-CSRF-B01` (JSON i UI) oraz wariant iframe z `SN-T-CSRF-B02` (pozostanie na stronie, UI i rekord MariaDB). To dwa przebiegi B01, nie wykonanie T-CSRF-B02. Dokładnych kodów HTTP/nagłówków nie udokumentowano. AFTER nie badano; pełnego PASS nie nadano. Ewentualna blokada wyświetlenia w ramce nie dowodzi odrzucenia POST. Szczegóły: [results.md](results.md).

1. **Cel:** wykazać faktyczny zapis przez przeglądarkę z drugiego originu bez tokenu.
2. **Warunki:** aplikacja HTTPS na 7028, kontrolowana strona HTTPS na 7443, ten sam host `localhost`, zaufane certyfikaty, konto A zalogowane; BEFORE ma CHG-CSRF-01/02.
3. **Kroki do odtworzenia/dokończenia:** zapisać listę notatek; otworzyć stronę 7443 jako najwyższy dokument; włączyć Preserve log; świadomie wysłać formularz do ukrytego iframe (historycznie: głównej karty); zarejestrować POST; sprawdzić zapis w aplikacji nowym GET. Powtórzenie z AFTER i jego własną sesją pozostaje na końcową próbę generalną.
4. **Dane:** formularz powyżej; brak tokenu i brak nagłówka Authorization, uwierzytelnienie wyłącznie automatycznym cookie.
5. **BEFORE:** oczekiwane 201 i notatka właściciela A.
6. **AFTER:** brak zapisu; status i przyczyna wymagają obserwacji. Możliwy jest błąd formatu lub Antiforgery; nie zakładamy izolowanej blokady CSRF.
7. **Potwierdzenie:** origin strony i nagłówek Origin, rzeczywisty typ treści, brak tokenu, obecność cookie bez wartości oraz nowa notatka w GET. Same 201 lub komunikat strony nie wystarczą.
8. **Dowody:** nagranie pochodzenia formularza, Network, odpowiedź i lista przed/po. To nie jest izolowany test Antiforgery, bo AFTER nie ma adaptera formularza.
9. **Przywrócenie:** usunąć notatkę, zamknąć stronę demonstracyjną, po serii wylogować konto i zatrzymać serwer formularza.

### T-CSRF-B02 — formularz bez uwierzytelnienia (pomocniczy)

1. **Cel:** potwierdzić zachowanie `[Authorize]` mimo zmian tworzenia.
2. **Warunki:** topologia B01, ale wylogowany/nowy profil bez cookie Auth; brak nagłówka Authorization.
3. **Kroki:** wysłać formularz; zapisać odpowiedź; następnie zalogować konto kontrolne i sprawdzić brak notatki tej próby.
4. **Dane:** `SN-T-CSRF-B02-R01`, zwykła treść.
5. **BEFORE:** oczekiwane 401, brak zapisu.
6. **AFTER:** najważniejszym kryterium jest brak zapisu bez uwierzytelnienia. Status 401 pozostaje hipotezą dla właściwej ścieżki; możliwe jest też wcześniejsze odrzucenie przez routing lub obsługę formatu. Faktyczny kod i przyczynę zarejestrować dopiero po testach.
7. **Potwierdzenie:** brak uwierzytelnienia w żądaniu i przede wszystkim brak zapisu po próbie, niezależnie od kodu odmowy. Odrzucenie przez routing lub format nie jest izolowanym dowodem działania autoryzacji; rzeczywisty kod i przyczynę ustalić w pomiarze.
8. **Dowody:** nagłówki bez sekretów, odpowiedź, kontrola listy.
9. **Przywrócenie:** jeżeli powstał nieoczekiwany zapis, zachować dowód i usunąć go kontrolowanym API właściciela; wyczyścić sesję próby.

### T-CSRF-B03 — kontrola cross-site (pomocniczy)

1. **Cel:** odróżnić ochronę SameSite od Antiforgery i od braku obsługi formatu.
2. **Warunki:** konto zalogowane na `https://localhost:7028`; formularz serwowany z `https://127.0.0.1:7443`, certyfikat obejmuje IP; najwyższy dokument, brak przekierowań i dodatkowych tokenów.
3. **Kroki:** wysłać formularz do localhost:7028; sprawdzić w DevTools wykluczenie cookie Auth z powodu SameSite; sprawdzić dane po próbie. Wykonać dla obu wariantów.
4. **Dane:** `SN-T-CSRF-B03-R01`, zwykła treść.
5. **BEFORE:** brak zapisu, oczekiwany brak cookie przy Strict i brak uwierzytelnienia.
6. **AFTER:** brak zapisu; może działać kilka barier, dlatego nie przypisujemy wyniku jednej bez diagnostyki.
7. **Potwierdzenie:** powód niewysłania cookie, nie tylko status. Błąd certyfikatu oznacza niewykonaną próbę, nie skuteczność SameSite.
8. **Dowody:** adresy, diagnostyka cookie, POST i lista po próbie.
9. **Przywrócenie:** usunąć ewentualne dane tej próby, zamknąć stronę IP i sesję testową.

### T-CORS-01 — preflight JSON (pomocniczy)

1. **Cel:** sprawdzić, czy cross-origin fetch JSON jest blokowany niezależnie od adaptera formularza.
2. **Warunki:** strona localhost:7443, aplikacja localhost:7028, konto A, brak cache preflight; żadnych zmian CORS.
3. **Kroki:** ze strony demonstracyjnej wywołać fetch POST do aplikacji z `credentials: 'include'`, typem `application/json`, bez tokenu; obserwować OPTIONS i ewentualny POST oraz listę notatek. Nie używać `no-cors` jako obejścia JSON.
4. **Dane:** JSON z tytułem `SN-T-CORS-01-R01` i zwykłą treścią.
5. **BEFORE:** oczekiwana odmowa preflight, brak właściwego POST i zapisu.
6. **AFTER:** tak samo. Jeśli POST dotarł, trzeba wyjaśnić rzeczywistą konfigurację, nie uznawać hipotezy za potwierdzoną.
7. **Potwierdzenie:** Network, brak zgody CORS i brak POST. Sam błąd odczytu odpowiedzi nie dowodzi, że żądania nie wysłano.
8. **Dowody:** OPTIONS, nagłówki odpowiedzi, konsola, kontrola listy.
9. **Przywrócenie:** usunąć ewentualny zapis, zakończyć stronę demonstracyjną; przed kolejną próbą usunąć cache lub użyć nowego profilu.

## Regresja

### T-REG-01 — zwykłe operacje na własnej notatce (regresyjny)

1. **Cel:** potwierdzić zachowanie podstawowej funkcjonalności.
2. **Warunki:** konto A, poprawne tokeny, zwykła treść, oba warianty badane osobno.
3. **Kroki:** przez UI utworzyć notatkę; odczytać; zmienić tytuł i treść; przeładować; usunąć; ponownie odczytać listę.
4. **Dane:** `SN-T-REG-01-R01`, treść `Tekst kontrolny`, następnie `Tekst po edycji`.
5. **BEFORE:** oczekiwane poprawne CRUD przy ważnych tokenach.
6. **AFTER:** identyczna funkcjonalność.
7. **Potwierdzenie:** kolejno 201, poprawny odczyt, 200 aktualizacji, 204 usunięcia i brak na liście.
8. **Dowody:** zapis etapów i odpowiedzi API bez sekretów.
9. **Przywrócenie:** usunięcie jest częścią testu; w razie przerwania usunąć zapamiętane ID.

### T-REG-02 — PUT/DELETE bez tokenu (regresyjny)

1. **Cel:** sprawdzić, czy osłabienie nie objęło pozostałych operacji.
2. **Warunki:** własna notatka A utworzona poprawnie, ważne uwierzytelnienie i cookie Antiforgery.
3. **Kroki:** wysłać poprawny PUT JSON bez tokenu; porównać treść; wysłać DELETE bez tokenu; potwierdzić istnienie; powtórzyć w drugim wariancie.
4. **Dane:** `SN-T-REG-02-R01`, próba zmiany treści na `Nie powinna zostać zapisana`.
5. **BEFORE:** oczekiwane 400 obu żądań i brak zmian.
6. **AFTER:** tak samo.
7. **Potwierdzenie:** poprawny format i sesja, brak tokenu, niezmieniony GET po obu próbach.
8. **Dowody:** PUT/DELETE, odpowiedzi i GET.
9. **Przywrócenie:** usunąć notatkę z poprawnym tokenem; jeśli treść zmieniła się nieoczekiwanie, najpierw zachować dowód.

### T-REG-03 — cudze notatki (regresyjny)

1. **Cel:** potwierdzić brak zmiany autoryzacji właściciela.
2. **Warunki:** konta A/B i osobne sesje; A tworzy zwykłą notatkę; B zna jej ID i ma swój poprawny token.
3. **Kroki:** jako B wykonać GET, poprawny PUT i DELETE na ID notatki A; następnie jako A odczytać oryginał. Token przy PUT/DELETE musi być poprawny, aby nie maskować autoryzacji.
4. **Dane:** `SN-T-REG-03-R01`, próba edycji `Treść użytkownika B`.
5. **BEFORE:** oczekiwane 404 dla operacji B, oryginał A istnieje i jest niezmieniony.
6. **AFTER:** tak samo.
7. **Potwierdzenie:** kontrola obu tożsamości bez ujawniania JWT i odczyt A po próbach.
8. **Dowody:** żądania B, statusy i końcowy GET A.
9. **Przywrócenie:** A usuwa własną notatkę; zakończyć obie sesje.

### T-REG-04 — cookies i zakres nagłówków (główny, regresyjny)

1. **Cel:** potwierdzić, że wyjątek CSP jest lokalny, a pozostałe bariery zachowane.
2. **Warunki:** oba warianty uruchamiane kolejno przez HTTPS; rejestrowane środowisko; świeże logowanie i pobranie tokenu.
3. **Kroki:** obejrzeć Set-Cookie/Storage bez wartości; pobrać `/notes.html`, `/`, `/login.html`, `/register.html` i `/api/notes`; porównać CSP i nagłówki; osobno odnotować HSTS stosownie do środowiska.
4. **Dane:** konto A, poprawne logowanie, bez payloadu.
5. **BEFORE:** Auth i Csrf mają HttpOnly, Secure, Strict; wyjątek `script-src-attr` tylko na notes.html; pozostałe dyrektywy i nagłówki zachowane, API `no-store`.
6. **AFTER:** `script-src-attr 'none'` bez wyjątku, te same flagi cookies i pozostałe nagłówki. W Development brak HSTS nie oznacza regresji; kod włącza go poza Development.
7. **Potwierdzenie:** porównanie odpowiedzi dokumentów, nie tylko pliku JS; flagi cookies i zakres Path. Sprawdzić nosniff, DENY, no-referrer i Permissions-Policy.
8. **Dowody:** zanonimizowane flagi oraz nagłówki każdego badanego zasobu; wersja i środowisko.
9. **Przywrócenie:** wylogować się i zamknąć profile; test nie powinien tworzyć notatek.

## Postępowanie z niezgodnością

Nie zmieniać oczekiwań po fakcie tylko po to, aby uzyskać zgodność. Zapisać obserwację, przyczynę, brakujące dane i decyzję o powtórzeniu. Awaria serwera, certyfikatu lub bazy nie jest dowodem skuteczności zabezpieczenia. Wynik niejednoznaczny wymaga wyjaśnienia, a nie automatycznego PASS. Uzupełniać [results.md](results.md) wyłącznie na podstawie rzeczywistych prób.
