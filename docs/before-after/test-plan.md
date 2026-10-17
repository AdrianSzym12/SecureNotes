# Plan testów BEFORE/AFTER

Procedury: PLANNED do dokończenia. Stan 06F, 17.10.2026: **T-XSS-02/04, T-CSRF-A01/A02/A03 i T-CSRF-B01 — PARTIAL — BEFORE; pozostałe 9 scenariuszy NOT RUN**. Wszystkie cztery zmiany wdrożono i częściowo zweryfikowano przez użytkownika w opisanym zakresie BEFORE. AFTER nie badano; porównania wykonamy dopiero podczas końcowej próby generalnej. Bazowy SHA: `21be7bb6aadd69c729250eb5d5248cabb114de04`. Faktyczne wyniki i ograniczenia: [results.md](results.md).

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
