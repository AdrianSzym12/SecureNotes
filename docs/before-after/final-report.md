# SecureNotes — raport porównawczy BEFORE/AFTER

**Robocza wersja merytoryczna, 16.10.2026.** Przedmiot: Bezpieczeństwo aplikacji internetowych. Temat: kontrolowana rekonstrukcja Stored XSS i CSRF oraz analiza istniejących zabezpieczeń przeglądarki i API.

Bazowy commit: `21be7bb6aadd69c729250eb5d5248cabb114de04`. Ustalenia statyczne: `VERIFIED`; zmiany: `PLANNED`; wszystkie nowe testy: `NOT RUN`. Ten dokument nie jest raportem z zakończonego eksperymentu. **DO UZUPEŁNIENIA PO TESTACH:** wyniki empiryczne, dowody i ostateczna ocena porównawcza.

## 1. Wprowadzenie

Aplikacje przechowujące dane użytkownika muszą rozdzielać dane od instrukcji wykonywanych w przeglądarce. Ta sama treść może być bezpieczna jako tekst, a niebezpieczna po interpretacji jako HTML. Niezależnym problemem jest ustalenie, czy uwierzytelnione żądanie rzeczywiście wynika z intencji zalogowanego użytkownika. Cookie potwierdzające sesję może zostać wysłane automatycznie przez przeglądarkę.

SecureNotes pozwala tworzyć prywatne notatki, odczytywać je, edytować i usuwać. Istniejąca aplikacja zawiera bezpieczne renderowanie, CSP, Antiforgery, JWT w chronionym cookie oraz sprawdzanie właściciela danych. Stanowi zatem podstawę do kontrolowanego eksperymentu polegającego na odtworzeniu wybranych błędów, bez zmiany modelu danych i uprawnień.

BEFORE nie jest wcześniejszym etapem historycznego rozwoju aplikacji. To planowana rekonstrukcja na bazie wersji zabezpieczonej. AFTER pozostaje niezmienioną referencją. Wnioski powinny dotyczyć roli mechanizmów w badanych ścieżkach, a nie sugerować, że zespół odkrył działającą podatność w aktualnym kodzie lub chronologicznie wdrożył zabezpieczenia dopiero po ataku.

## 2. Cel, zakres i hipotezy

Celem jest wykazanie różnicy między przechowaniem treści, interpretacją HTML i wykonaniem JavaScript oraz między uwierzytelnieniem a kontrolą intencji operacji. Projekt obejmuje jedną treść notatki i jedną operację tworzenia. Nie obejmuje serwisów publicznych, cudzych kont, danych rzeczywistych, przejęcia infrastruktury ani szerokiego aktywnego skanowania.

Hipoteza XSS zakłada, że zmiana `textContent` na `innerHTML` spowoduje interpretację payloadu jako HTML, ale wykonanie handlera będzie nadal zależeć od CSP. Hipoteza CSRF A zakłada, że usunięcie walidacji z tworzenia JSON doprowadzi do akceptacji poprawnych żądań biznesowych również z błędnym lub brakującym tokenem. Hipoteza CSRF B zakłada możliwość utworzenia notatki przez formularz na drugim originie, jeżeli w BEFORE zostanie dodana jego obsługa bez tokenu, a cookie zostanie wysłane w kontekście same-site HTTPS.

Karta projektu P7 wskazuje własne lokalne konto, jedną ścieżkę XSS, formularz zmieniający dane i 3–6 kluczowych testów. Proponowany w karcie Node.js/Express różni się od rzeczywistego ASP.NET Core. Ta zmiana stosu wymaga potwierdzenia akceptacji; raport opisuje kod istniejący, nie niezrealizowaną propozycję.

## 3. Środowisko i odtwarzalność

Wersja referencyjna znajduje się w `H:\Roboczy\BAI\SecureNotes-dev` na `develop`, a demonstracyjna w `H:\Roboczy\BAI\SecureNotes-BEFORE` na `demo/security-before`. Przed utworzeniem niniejszej dokumentacji oba branche miały wspólny bazowy SHA i czysty status, a różnica zatwierdzonego kodu była pusta. Administracyjne powiązania worktree zostały wcześniej naprawione; obecny etap ich nie zmienia.

Projekty deklarują .NET 8, ASP.NET Core, EF Core i Pomelo. Frontend to HTML/CSS i JavaScript. `PersistenceService` wskazuje MariaDB 10.4.32 jako wersję dla dostawcy; nie jest to potwierdzenie wersji zainstalowanego serwera. Skrypt bazy przygotowuje `securenotes_db`, `securenotes_test_db` oraz użytkownika migracyjnego. Tabele tworzy migracja EF Core.

Profile uruchomienia deklarują HTTPS na localhost:7028 oraz HTTP na localhost:5070. Badania mają używać HTTPS i zaufanego certyfikatu. Druga strona na localhost:7443 zapewni inny origin, zachowując ten sam site. Dane i sesje wariantów muszą być rozdzielone; rekomendowane jest uruchamianie kolejno na tym samym adresie aplikacji i korzystanie z osobnych profili przeglądarki.

Start Development automatycznie uruchamia migracje. Z tego powodu nawet uruchomienie w celu diagnostycznym wymaga uprzedniego potwierdzenia docelowej bazy. Oba worktree mają wspólny identyfikator User Secrets API, więc sam podział katalogów nie gwarantuje izolacji danych. Procedurę nadpisania konfiguracji procesu lub odtwarzania bazy trzeba ustalić przed próbami. Nie odczytywano wartości sekretów ani nie uruchamiano środowiska w tym etapie.

Szczegóły narzędzi, danych i kolejności startu znajdują się w [environment.md](environment.md). **DO UZUPEŁNIENIA PO TESTACH:** rzeczywiste wersje narzędzi, sposób izolacji, potwierdzone porty, certyfikaty i protokół uruchomienia.

## 4. Architektura i mechanizmy istniejące

Architektura rozdziela Domain, Application, Infrastructure i API. Domain zawiera encje i reguły danych. Application realizuje przypadki użycia poprzez serwisy i interfejsy. Infrastructure implementuje repozytoria, Unit of Work, EF Core, hashowanie haseł i JWT. API łączy zależności i udostępnia kontrolery oraz statyczny interfejs. Diagram przedstawiono w [architecture.md](architecture.md).

Przy zapisie `notes.js` przekazuje dane do `api.js`, który pobiera token Antiforgery i wysyła JSON. Kontroler odczytuje właściciela z claims, a serwis tworzy encję i zatwierdza zapis. Przy odczycie repozytorium filtruje dane po właścicielu, API zwraca DTO, a frontend buduje DOM. Treść zachowana w bazie nie zyskuje przez sam zapis statusu danych zaufanych.

Hasła obsługuje ASP.NET Identity `PasswordHasher<object>`. Rejestracja dopuszcza hasła długości 12–128 znaków. JWT jest podpisywany HMAC-SHA256 i walidowany pod kątem podpisu, algorytmu, wystawcy, odbiorcy i terminu ważności. Klucz musi mieć co najmniej 32 bajty, a tolerancja czasu wynosi zero. JWT trafia do cookie `SecureNotes.Auth`, nie do localStorage w analizowanym przepływie.

Cookie ma HttpOnly, Secure i SameSite=Strict. Wylogowanie usuwa cookies, lecz nie wprowadza serwerowej listy unieważnienia już wydanych JWT. Wszystkie akcje notatek wymagają uwierzytelnienia, a dostęp do pojedynczej notatki jest filtrowany po jej ID i ID właściciela. Te elementy nie są przedmiotem planowanego osłabienia.

## 5. Model zagrożeń i ryzyko

Chronimy poufność i integralność notatek, możliwość działania w sesji oraz dane uwierzytelniające. Zespół laboratoryjny symuluje zarówno użytkownika zapisującego treść, jak i operatora drugiej strony. Nie zakładamy istnienia mechanizmu udostępniania notatek innym kontom. Potencjalny wpływ wykonania JavaScript w originie aplikacji jest szerszy niż sam alert, lecz nie oznacza to wykazania drogi dostarczenia payloadu ofierze.

Ryzyko oceniamy jakościowo. XSS ma potencjalnie wysoki wpływ na kontekst strony, ale ekspozycja w tym modelu jest ograniczona prywatnym zapisem. Formularzowe CSRF ma w doświadczeniu skutek w integralności: tworzy niechcianą notatkę. Nie demonstruje odczytu odpowiedzi przez stronę atakującą ani kradzieży danych. Wymaga aktywnej sesji i warunków wysłania cookie.

Priorytetem metodycznym jest rozdzielenie mechanizmów. Błąd przypisania wyniku może prowadzić do nieuprawnionego wniosku o skuteczności Antiforgery, CSP lub SameSite. Szczegółowe aktywa, bariery i ograniczenia zawiera [threat-model.md](threat-model.md). Nie obliczano CVSS; nie przyjęto udokumentowanej metodologii oceny liczbowej.

## 6. Stored XSS — projekt doświadczenia

W aktualnym kodzie `createNoteElement` przypisuje treść przez `textContent`. Dane stają się tekstem, nawet jeżeli zawierają nawiasy i atrybuty HTML. Domena ogranicza długość treści, ale nie usuwa znaczników; przy obecnym sposobie wyświetlania nie jest to samo w sobie błąd. CSP dodatkowo zawiera `script-src 'self'` i `script-src-attr 'none'`.

CHG-XSS-01 zmieni wyłącznie ujście treści na `innerHTML`. CHG-XSS-02 dopuści atrybuty zdarzeń wyłącznie na dokumencie `/notes.html`, pozostawiając pozostałą politykę. Wyjątek nadal jest realnym osłabieniem strony, nie technicznym zezwoleniem dla tylko jednego payloadu; jego stosowanie ogranicza się do lokalnego BEFORE. Tytuł, edytor, model i autoryzacja pozostaną bez zmian.

Nieszkodliwy payload zawiera obraz spod nieistniejącego lokalnego adresu i handler `onerror`, który pokazuje alert oraz ustawia `data-sn-xss`. T-XSS-01 sprawdzi zachowanie danych po zapisie, T-XSS-02 interpretację i wykonanie, T-XSS-03 ponowne wykonanie po pobraniu, a T-XSS-04 działanie oryginalnej CSP na pośrednim etapie z samym niebezpiecznym rendererem.

Element `img` w DOM potwierdza HTML, nie JavaScript. Alert i znacznik ustawiony przez handler są dowodem wykonania dopiero po powiązaniu z treścią pobraną z API. Ponowne załadowanie pozwala odróżnić Stored XSS od jednorazowej zmiany DOM. W AFTER przewidujemy dosłowny tekst payloadu i brak wykonania; w pośrednim BEFORE HTML bez handlera; w docelowym BEFORE wykonanie.

**DO UZUPEŁNIENIA PO TESTACH:** odpowiedzi POST/GET, ID notatki, DOM, rzeczywisty nagłówek CSP, alert, nagranie ponownego pobrania i wyjaśnienie odstępstw. Na tym etapie nie zaobserwowano wykonania payloadu.

## 7. CSRF — dwa niezależne doświadczenia

### 7.1. Walidacja Antiforgery dla JSON

`POST /api/notes` przyjmuje istniejący DTO przez `[FromBody]` i ma `[ValidateAntiForgeryToken]`. Token żądania jest przekazywany w `X-CSRF-TOKEN`, a cookie `SecureNotes.Csrf` stanowi osobny składnik walidacji. Frontend pobiera token przez `GetAndStoreTokens`. Dla prób trzeba uzyskać token właściwy dla aktualnej tożsamości po logowaniu.

CHG-CSRF-01 usunie walidację wyłącznie z tworzenia. T-CSRF-A01–03 porównają poprawny, błędny i brakujący token przy niezmienionym formacie, body i ważnym uwierzytelnieniu. Tożsamość i cookie Antiforgery pozostają poprawne także w próbie brakującego nagłówka. Oczekujemy 201 w obu wersjach z tokenem poprawnym oraz 201 w BEFORE i 400 w AFTER dla pozostałych dwóch wariantów. Każdy wynik wymaga potwierdzenia stanu danych i przyczyny odrzucenia.

Doświadczenie A można wykonać przez bezpośredni fetch z originu aplikacji. Nie jest ono samodzielnym atakiem z zewnętrznej strony. Izoluje rolę walidacji tokenu, unikając dodatkowych zmiennych formatu i preflight. Wartości prawidłowych tokenów i cookies nie muszą być identyczne między sesjami; identyczna ma być struktura żądań i treść biznesowa.

### 7.2. Lokalny formularz z innego originu

Zwykły formularz wysyła dane URL-encoded, nie JSON. Cross-origin fetch z `application/json` wymaga preflight, a kod nie konfiguruje CORS. Dlatego CHG-CSRF-02 doda w BEFORE mały adapter formularzowy pod tym samym adresem, rozdzielony od akcji JSON przez typ treści. To świadomie dodana laboratoryjna powierzchnia ataku, nie istniejąca podatność AFTER.

Strona `https://localhost:7443` wyśle formularz do `https://localhost:7028/api/notes`. Zmiana portu oznacza inny origin, lecz nie inny site. Cookie SameSite=Strict może więc zostać wysłane w tym kontekście bez zmiany jego flag. HttpOnly blokuje odczyt cookie przez JavaScript, nie automatyczne uwierzytelnianie żądania. Secure wymaga HTTPS. Cookies nie są odseparowane portem, dlatego serwer demonstracyjny może otrzymać cookie uwierzytelniające już przy otwarciu strony. Musi być całkowicie lokalny, kontrolowany przez zespół i nie może zapisywać ani publikować nagłówków Cookie. Ta demonstracja nie dowodzi ataku z niezależnej domeny cross-site ani obejścia SameSite=Strict.

CORS ogranicza dostęp skryptu do odpowiedzi i niektóre typy żądań; nie jest ogólnym zakazem nawigacji formularza. Polityka `form-action` strony aplikacji nie kontroluje formularza znajdującego się na innej stronie. Pokaz nie używa iframe, więc blokowanie ramek nie jest jego warunkiem. Strona atakująca nie musi odczytać odpowiedzi, aby spowodować zapis.

T-CSRF-B01 ma wykazać POST bez tokenu i rzeczywiste utworzenie notatki. T-CSRF-B02 sprawdzi brak uwierzytelnienia. T-CSRF-B03 zmieni kontekst na cross-site i sprawdzi wykluczenie cookie przy Strict. T-CORS-01 zbada osobno preflight JSON. Odrzucenie formularza przez AFTER może wynikać z Antiforgery lub nieobsługiwanego formatu. Nie wolno przedstawiać tego porównania jako izolowanego dowodu Antiforgery.

**DO UZUPEŁNIENIA PO TESTACH:** faktyczne originy, obecność cookie bez wartości, brak tokenu, typ treści, odpowiedzi, diagnostyka preflight i SameSite, lista notatek przed/po oraz ustalona przyczyna odrzucenia AFTER.

## 8. Metodyka i dowody

Plan obejmuje 15 testów, z czego sześć głównych: T-XSS-02, T-CSRF-A01, T-CSRF-A02, T-CSRF-A03, T-CSRF-B01 i T-REG-04. Pozostałe potwierdzają trwałość, wpływ CSP, granice cookies i CORS oraz zachowanie CRUD i autoryzacji. Każda próba wymaga zapisania SHA wariantu, środowiska, danych wejściowych i numeru powtórzenia. Procedury i sprzątanie opisano w [test-plan.md](test-plan.md).

Ocenę zgodności z hipotezą oddzielamy od skuteczności zabezpieczenia. Wykonanie XSS w BEFORE jest oczekiwanym rezultatem rekonstrukcji, lecz negatywną oceną bariery bezpieczeństwa dla tej ścieżki. Kod 400 nie ma samodzielnie znaczenia bez wyjaśnienia, a kod 201 wymaga potwierdzenia zapisu. Awaria certyfikatu, bazy lub serwera nie jest wynikiem testu bezpieczeństwa.

Dowody muszą łączyć żądanie, odpowiedź i stan danych lub DOM. Publiczne materiały nie zawierają wartości haseł, cookies ani tokenów. Surowe sesje proxy i HAR wymagają odrębnej kontroli i nie powinny być publikowane. Obecne katalogi dowodów są puste; pliki `.gitkeep` nie są materiałem technicznym.

## 9. Porównanie planowanych wariantów

| Mechanizm | AFTER — stan kodu VERIFIED | BEFORE — stan PLANNED |
|---|---|---|
| Treść notatki | `textContent` | `innerHTML` |
| Handlery w CSP | Zakazane | Dopuszczone tylko na notes.html |
| Antiforgery tworzenia JSON | Wymagane | Usunięte z jednej operacji |
| Formularz tworzenia | Brak adaptera | Nowy adapter laboratoryjny bez tokenu |
| JWT, cookies i właściciel | Istniejące mechanizmy | Zachowane |
| Antiforgery PUT/DELETE i auth | Wymagane | Zachowane |
| Globalny CORS | Brak konfiguracji | Bez zmian |

Tabela porównuje plan z bazą, nie wykonaną implementację. Rejestr plików i metod znajduje się w [change-register.md](change-register.md). **DO UZUPEŁNIENIA PO TESTACH:** końcowy diff, rzeczywiste SHA zmian, obserwacje i powiązania dowodów w [results.md](results.md).

## 10. Standardy i pokrycie

Mapowanie opiera się na oficjalnych wydaniach zebranych podczas przeglądu 02A.1: OWASP Top 10:2025 wiąże XSS (CWE-79) z A05:2025 Injection, a CSRF (CWE-352) z A01:2025 Broken Access Control. W ASVS 5.0.0 bezpośrednio powiązano V3.2.2 z rendererem (CHG-XSS-01, T-XSS-02/03), V3.4.3 z CSP (CHG-XSS-02, T-XSS-04 i T-REG-04), a V3.5.1 z kontrolą pochodzenia żądań, w tym Antiforgery (CHG-CSRF-01/02, T-CSRF-A01–03 i T-CSRF-B01). V3.5.2 powiązano warunkowo z funkcjonalnością polegającą na preflight: T-CORS-01 i T-CSRF-B01 porównują JSON oraz dodaną przez CHG-CSRF-02 ścieżkę formularza, bez twierdzenia, że AFTER polega wyłącznie na CORS. V3.3.2 dotyczy właściwego SameSite (T-REG-04, T-CSRF-B03), a V3.3.4 HttpOnly dla danych niedostępnych skryptom (T-REG-04); cookies mają pozostać niezmienione przy rekonstrukcji. Testy pomocnicze wyjaśniają granice tych mechanizmów, nie dowodzą pełnej zgodności.

Metodykę wiążemy z WSTG-v42-INPV-02 dla Stored XSS (T-XSS-01–04), WSTG-v42-SESS-05 dla CSRF (T-CSRF-A01–03 i T-CSRF-B01–03) oraz WSTG-v42-SESS-02 dla atrybutów cookies (T-REG-04, pomocniczo T-CSRF-B03). Pełne adresy wersjonowanych źródeł i uzasadnienie każdej relacji podaje [mapowanie standardów](standards-mapping.md). Potwierdzenie identyfikatorów nie jest potwierdzeniem wyników: wszystkie nowe testy pozostają NOT RUN.

Wybrane scenariusze nie stanowią pełnego audytu ASVS ani podstawy do zadeklarowania poziomu zgodności. Zakres obejmuje konkretną ścieżkę DOM i tworzenie notatki oraz wybrane regresje. [standards-mapping.md](standards-mapping.md) rozróżnia powiązania bezpośrednie, warunkowe i pomocnicze oraz wskazuje źródła; dowody skuteczności pozostają do uzupełnienia po testach.

## 11. Ograniczenia badania

Obecnie nie ma nowych wyników wykonawczych. Historyczny wynik 85/85 w README i wcześniejsze zrzuty audytowe nie są wynikami tej serii. Testy API HSTS budują odrębny TestServer i nie dowodzą działania całego pipeline. Testy infrastruktury korzystają z bazy i migracji, dlatego ich uruchomienie wymaga osobnej kontroli środowiska.

Prywatny model notatek ogranicza możliwość dostarczenia payloadu innemu użytkownikowi. Demonstracja CSRF wymaga same-site originu, nie dowodzi obejścia SameSite i dodaje obsługę formularza tylko w BEFORE. Jednoczesna zmiana renderera i CSP nie izoluje każdej bariery, dlatego zaplanowano etap pośredni. Nie testujemy wszystkich przeglądarek, wejść, zależności ani wdrożenia Production. Potencjalne skutki XSS są szersze niż nieszkodliwy payload, ale nie są empirycznie wykazywane.

## 12. Wnioski na obecnym etapie

Kod potwierdza istnienie wielowarstwowych zabezpieczeń w obu aktualnych wariantach. Minimalna rekonstrukcja wymaga trzech plików i czterech logicznych zmian, ponieważ sama zamiana renderera nie znosi CSP, a samo usunięcie Antiforgery nie zapewnia obsługi standardowego formularza. Rozdzielenie doświadczeń pozwala uniknąć błędnego przypisania efektu jednej barierze.

Nie ma podstaw do stwierdzenia skutecznego ataku ani potwierdzonej odporności po retestach. Następny etap powinien rozwiązać izolację środowiska, akceptację stosu i HTTPS, następnie wdrażać zmiany pojedynczo i wykonywać plan. **DO UZUPEŁNIENIA PO TESTACH:** końcowe wnioski o skuteczności, ograniczenia zaobserwowane, ryzyko rezydualne i rekomendacje oparte na wynikach.

## 13. Plan składu raportu 8–12 stron

Docelowo około 10 stron: wprowadzenie i zakres 1; środowisko 1; architektura i diagram 1; model zagrożeń 1; XSS 1,5; CSRF 2; metodyka i wyniki 1,5; standardy, ograniczenia i wnioski 1. Rozdziały niniejszego dokumentu dostarczają treści roboczej; rzeczywista paginacja zależy od szablonu, fontu, tabel i dowodów. Nie deklarujemy osiągnięcia 8–12 stron przed składem i eksportem. Należy wybierać dowody rozstrzygające, a pełne protokoły pozostawić w dokumentacji towarzyszącej.

Źródła: kod bazowego commita; główny `README.md`; `database/setup.sql`; lokalna karta projektu P7; odczytany audyt statyczny. Historyczna dokumentacja jest kontekstem, nie dowodem nowej serii. Bieżący [indeks dokumentów](README.md) prowadzi do wszystkich części i przyszłych dowodów.
