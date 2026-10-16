# Środowisko laboratoryjne

Stan dokumentu: `VERIFIED` dla deklaracji kodu i utworzenia pustej bazy BEFORE w 04A; działanie AFTER potwierdzone przez użytkownika w 03B; konfiguracja i uruchomienie BEFORE potwierdzone przez użytkownika w 04D, z wynikami poniżej. Wszystkie 15 testów demonstracyjnych: `NOT RUN`; cztery zmiany implementacyjne: `PLANNED`. Bazowy commit: `21be7bb6aadd69c729250eb5d5248cabb114de04`.

## Potwierdzenie środowiska referencyjnego AFTER — 03B

Użytkownik przekazał potwierdzenie uruchomienia SecureNotes z brancha `develop` w Visual Studio 2022 oraz informację o screenshotach. Zgodnie z tym potwierdzeniem:

- aplikacja uruchamia się, łączy z MariaDB w XAMPP i poprawnie zapisuje notatki;
- Test Explorer pokazuje **85 zaliczonych testów automatycznych, 0 nieudanych, 0 pominiętych (85/85)** dla wersji referencyjnej AFTER;
- phpMyAdmin potwierdza bazy `securenotes_db` i `securenotes_test_db` oraz tabele widoczne jako `users`, `notes`, `__efmigrationshistory`.

Źródłem jest relacja użytkownika w Prompt 03B, nie wykonanie testów przez Codexa ani niezależne sprawdzenie bazy w tym etapie. Nie ustalono tutaj dokładnej daty uruchomienia i SHA artefaktu użytego do tego przebiegu; bazowy SHA dokumentacji nie zastępuje takiej metryki. Screenshoty nie zostały w tym etapie fizycznie zapisane i zweryfikowane w katalogach dowodowych; nie przypisano im zarchiwizowanych nazw ani ID. Miejsce na późniejszą metrykę: data przebiegu, pełny SHA, ścieżki zweryfikowanych plików i ID dowodów — **DO UZUPEŁNIENIA PO ARCHIWIZACJI**.

To potwierdzenie gotowości istniejącej referencji i jej testów automatycznych. Nie oznacza wykonania żadnego z 15 scenariuszy demonstracyjnych XSS/CSRF i regresyjnych tej serii; wszystkie pozostają `NOT RUN`. Nie potwierdza uruchomienia BEFORE na nowej bazie.

## Uproszczony model i przygotowanie BEFORE — 04A

AFTER pozostaje gotową aplikacją z istniejącą `securenotes_db`. Nie przygotowujemy dodatkowej bazy ani środowiska AFTER. BEFORE jest przeznaczone do kontrolowanej rekonstrukcji podatności, jeszcze niewdrożonych, i otrzymuje wyłącznie nową `securenotes_before_lab`. Istniejąca `securenotes_test_db` zachowuje przeznaczenie do testów automatycznych.

W 04A Codex połączył się klientem XAMPP przez TCP do `127.0.0.1:3306`, z pominięciem plików domyślnych i bez odczytu sekretów. Serwer potwierdził `root@localhost` i MariaDB 10.4.32; lokalny listener należał do procesu XAMPP. Odczyt information_schema wykazał brak bazy BEFORE. Wykonano jedyną operację zmieniającą SQL:

```sql
CREATE DATABASE `securenotes_before_lab`
CHARACTER SET utf8mb4
COLLATE utf8mb4_general_ci;
```

Odczyt po utworzeniu potwierdził istnienie bazy, `utf8mb4`, `utf8mb4_general_ci` oraz **0 tabel**. Obie oryginalne bazy nadal istnieją; przed i po operacji ich wykaz zawierał `notes`, `users`, `__efmigrationshistory`. Poza CREATE wykonywano tylko zapytania odczytowe do identyfikacji serwera i metadanych. Nie wykonywano DML, zmian kont, haseł, GRANT ani migracji; nie odczytywano rekordów użytkowników/notatek. Zgodność wykazu tabel nie jest pomiarem niezmienności wszystkich danych przy ewentualnej równoległej pracy aplikacji — brak ingerencji wynika z zakresu wykonanych poleceń.

## Technologie i narzędzia

SecureNotes wykorzystuje C#, ASP.NET Core 8, HTML/CSS i moduły JavaScript. Wszystkie projekty mają `TargetFramework=net8.0`. Dostęp do MariaDB zapewniają EF Core i Pomelo. W kodzie zadeklarowano MariaDB `10.4.32`; nie potwierdzono wersji uruchomionego serwera.

| Zależność | Wersja deklarowana w projektach |
|---|---|
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.22 |
| Microsoft.EntityFrameworkCore.Tools / Design | 8.0.13 |
| Pomelo.EntityFrameworkCore.MySql | 8.0.3 |
| System.IdentityModel.Tokens.Jwt | 8.9.0 |
| Microsoft.Extensions.Identity.Core | 8.0.8 |
| Swashbuckle.AspNetCore | 6.6.2 |
| xUnit / Microsoft.NET.Test.Sdk | 2.5.3 / 17.8.0 |
| Microsoft.AspNetCore.TestHost | 8.0.11 |

Do przyszłego uruchomienia potrzebne są .NET 8 SDK, MariaDB, Git, przeglądarka z DevTools i zaufany lokalny certyfikat HTTPS. Visual Studio 2022 jest opcjonalną alternatywą dla CLI. ZAP/Burp Community może wspomóc rejestrowanie ruchu, lecz nie zastępuje rzeczywistego formularza w przeglądarce. Oddzielna strona CSRF wymaga lokalnego serwera statycznego HTTPS; konkretny serwer pozostaje do wyboru. Nie instalowano narzędzi ani nie sprawdzano ich gotowości do demonstracji.

## Konfiguracja bez sekretów

Sekcja `Api` obejmuje `Server`, `Database`, `Port`, `User`, `Password`; `Jwt` obejmuje `Issuer`, `Audience`, `ExpirationMinutes`, `SecretKey`. Kod wymaga poprawnej konfiguracji bazy, dodatniego czasu JWT i klucza co najmniej 32-bajtowego. Nie podajemy wartości haseł, kluczy, JWT ani cookies. Nie odczytywano User Secrets.

Wartości poufne powinny pozostać w lokalnej konfiguracji poza repozytorium. Nie publikować zrzutów sekretów. Oba worktree mają ten sam UserSecretsId, więc sam podział katalogów nie izoluje konfiguracji. Start BEFORE procesowo ustawia Api__Server=127.0.0.1, Api__Port=3306, Api__Database=securenotes_before_lab, Api__User=sn_before_lab i lokalnie podane Api__Password. AFTER zachowuje istniejącą securenotes_db i konfigurację. W 04A nie ustawiano zmiennych i nie uruchamiano aplikacji; później użytkownik potwierdził konto, prawa i działający start w 04D. Przy odtworzeniu na innym komputerze nadal trzeba przygotować dostęp i konfigurację JWT; nie używać root jako konta aplikacji.

`PersistenceService.AddPersistence` ustawia `SslMode=Preferred`; sam zapis nie dowodzi szyfrowania połączenia z bazą. Zakres eksperymentu pozostaje lokalny.

## Baza i migracje

`database/setup.sql` przygotowuje `securenotes_db` i `securenotes_test_db`, obie z `utf8mb4`, oraz lokalnego użytkownika `securenotes_migrator`. Uprawnienia obejmują operacje na danych i schemacie obu baz. Skrypt nie tworzy tabel aplikacji. Migracja `20261008210341_InitialCreate` tworzy `Users`, `Notes`, indeksy i relację właściciela.

**Uruchomienie Development modyfikuje bazę:** `SecureNotes.API/Program.cs:48–55` wykonuje `Database.MigrateAsync()`. Przed przyszłym startem trzeba potwierdzić docelową bazę, jej laboratoryjny charakter i sposób odtworzenia. Nie należy uruchamiać aplikacji tylko w celu sprawdzenia, czy się kompiluje. Testowa `DatabaseFixture.InitializeAsync` również wykonuje migracje i dopuszcza wyłącznie nazwę `securenotes_test_db`.

Istniejące `securenotes_db` i `securenotes_test_db` należy zachować bez zmian podczas przygotowania BEFORE: nie wykonywać na nich resetów, czyszczenia, nowych migracji ani ponownych testów w ramach tej procedury. Jedyna nowa baza to utworzona `securenotes_before_lab` na tym samym serwerze MariaDB. Nie należy uruchamiać obecnego `database/setup.sql` jako procedury jej przygotowania — wskazuje istniejące bazy. Nie wolno zakładać, że obecne konto aplikacyjne ma dostęp do nowej bazy; nadanie uprawnień nie było częścią 04A.

W etapie 03B Codex nie tworzył baz, nie wykonywał migracji, SQL, testów, kont ani czyszczenia danych. Relacja użytkownika dotyczy istniejącego środowiska AFTER. W 04A utworzono wyłącznie pustą bazę BEFORE i sprawdzono metadane. Fixture testów infrastrukturalnych wymaga `securenotes_test_db`, dlatego nie należy uruchamiać go w ramach przygotowania BEFORE ani zakładać, że samo `Api__Database` przeniesie ten zestaw na nową nazwę.

## Plan uruchomienia

### Automatyzacja — wyniki konfiguracji potwierdzone przez użytkownika w 04D

W [scripts/lab/README.md](../../scripts/lab/README.md) opisano setup-before.ps1 (baza/konto i ograniczone granty, WhatIf i potwierdzenia), verify-before.ps1 (odczytowy CURRENT_USER, DATABASE, parametry, tabele, historia i prawa) oraz start-before.ps1 (kontrole lokalne i procesowy wybór BEFORE). Użytkownik rzeczywiście przetestował automatyzację na jednym środowisku Windows, PowerShell i MariaDB/XAMPP. Poniższe wyniki pochodzą z jego potwierdzenia w Prompt 04D, nie z wykonania skryptów przez Codexa. Dawny brak konta w 04C i pusta baza w 04A opisują stan historyczny, zastąpiony kolejnymi działaniami użytkownika.

| Kontrola konfiguracji | Obserwacje potwierdzone przez użytkownika | Wynik |
|---|---|---|
| setup-before.ps1 -WhatIf | Połączenie działa, istniejąca baza wykryta, brak modyfikacji SQL; zaplanowano konto bez pobierania jego nowego hasła | PASS |
| setup-before.ps1 | Utworzono sn_before_lab@127.0.0.1; siedem praw tylko do securenotes_before_lab; bez DROP i GRANT OPTION, bez praw globalnych poza technicznym USAGE; kontrola SHOW GRANTS poprawna; istniejących danych nie usunięto | PASS |
| verify-before.ps1 przed migracją | .NET 8, połączenie/konto, kodowanie/kolacja i SHOW GRANTS: PASS; tabele/migracje: NOT READY zgodnie z oczekiwaniem pustej bazy | Zgodne ze stanem przed migracją |
| Pierwsze uruchomienie BEFORE | Development, automatyczne migracje EF Core; notes, users, __efmigrationshistory; potwierdzona 20261008210341_InitialCreate; logowanie i zapis/odczyt notatek; dane dostępne po restarcie | PASS |
| Uproszczony start-before.ps1 po zmianie | Dokładnie jeden maskowany monit; cel 127.0.0.1:3306 / securenotes_before_lab / sn_before_lab; ostrzeżenie, START BEFORE, pomyślny start, HTTPS localhost:7028 i Development; komunikat EF Core „No migrations were applied. The database is already up to date.” | PASS |

Nie zgłoszono osobnego wyniku uruchomienia startu z -VerifyDatabase po uproszczeniu ani odtworzenia na drugim komputerze. Nie przypisujemy im PASS. Nie potwierdzamy dodatkowych obserwacji, np. niezależnego pomiaru przywrócenia środowiska procesu. Daty prób, SHA faktycznie użytych plików i archiwizacja materiałów pozostają do uzupełnienia; nie tworzymy fikcyjnych nazw screenshotów ani ID dowodów. W katalogach evidence nie dodano plików obrazów. Te wyniki konfiguracji nie zmieniają 15 statusów NOT RUN ani czterech CHG PLANNED.

Kolejność odtworzenia na innym komputerze: setup z WhatIf → zatwierdzony setup → verify → zatwierdzony start → kontrola historii/tabel. To instrukcja, nie wynik drugiego wdrożenia. Podawać rzeczywistą ścieżkę klienta parametrem MySqlPath; brak założenia XAMPP lub dysku projektu. WhatIf wykonuje uwierzytelnione odczyty, ale nie CREATE/GRANT. Setup nie resetuje kont, nie czyści danych i nie poprawia niezgodnych praw. Zwykły restart to start-before.ps1 z jednym maskowanym monitem. -VerifyDatabase opcjonalnie wykonuje pełny verify przed startem i wymaga dodatkowych uwierzytelnień; bez przełącznika kontrole przedstartowe są lokalne, bez SQL. Jeśli klient na innym komputerze nie obsługuje natywnego promptu, obowiązuje opisany wariant ręczny phpMyAdmin, bez hasła w argumentach.

Automatyczne tabele nadal tworzy wyłącznie istniejące Database.MigrateAsync w Development. Nie dodano dotnet ef database update ani drugiego mechanizmu migracji. Start wymaga potwierdzenia przy każdym wywołaniu i przywraca pięć poprzednich zmiennych procesu w finally. Hasło nowego konta jest maskowane i przekazywane w SQL jako hash mysql_native_password przez stdin; hash również jest poufny. Hasło procesu aplikacji jest tekstowe w jego środowisku i dziedziczone przez procesy potomne. Pamięć i logi/audyt MariaDB nie zapewniają całkowitej niewidoczności; szczegóły i ograniczenia są w instrukcji. Nie modyfikować wspólnego UserSecretsId. Visual Studio otwarte niezależnie nie dziedziczy ustawień z dowolnej sesji PowerShell; preferowany start laboratoryjny to skrypt. Wszystkie 15 scenariuszy pozostaje NOT RUN, a cztery zmiany podatności PLANNED.

1. Potwierdzić branch i SHA badanego katalogu oraz zapisać je w protokole próby.
2. Przy odtworzeniu przygotować bazę/konto przez setup po zatwierdzeniu, potem wykonać verify. W obecnym laboratorium konto, uprawnienia i migracje są już potwierdzone przez użytkownika; nie resetować danych ani nie zakładać, że baza nadal jest pusta. Nie ingerować w securenotes_db i securenotes_test_db.
3. Ustawić lokalnie wymaganą konfigurację bez zapisywania sekretów w dokumentacji.
4. Potwierdzić zaufanie certyfikatowi deweloperskiemu i wolne porty.
5. Uruchamiać BEFORE przez start-before.ps1: sztywne Api__*, jedno maskowane hasło aplikacji, START BEFORE i istniejący profil HTTPS. -VerifyDatabase to opcjonalna pełna diagnostyka z dodatkowymi monitami. AFTER pozostaje działającą referencją bez dodatkowej instancji/bazy; nie uruchamiać go z sesji BEFORE ani nie budować jego katalogu w ramach tej procedury.
6. Otworzyć `https://localhost:7028/`; zarejestrować rzeczywisty adres i środowisko.
7. Utworzyć fikcyjne konta oraz przejść zwykłe logowanie i utworzenie notatki z poprawnym tokenem.

Ta instrukcja służy odtworzeniu konfiguracji sprawdzonej przez użytkownika na jednym komputerze; Codex w 04D nie uruchamiał ponownie aplikacji, SQL i migracji. Profil https deklaruje również http://localhost:5070, ale do demonstracji używamy HTTPS. Swagger w Development pozostaje ograniczany przez zachowaną CSP; nie osłabiamy jej dla UI. Przenośność na drugi komputer wymaga osobnej próby.

## Rozdzielenie wariantów i danych

Rekomendowane jest uruchamianie wariantów kolejno na tym samym adresie aplikacji, z osobnymi profilami przeglądarki BEFORE/AFTER. W każdym wariancie trzeba osobno uzyskać ważną sesję i token po zalogowaniu. Nie przenosimy JWT ani cookie Antiforgery między wersjami. Cookies nie są izolowane numerem portu.

Konta fikcyjne: A — właściciel notatek; B — wyłącznie do regresji autoryzacji. Nie używamy rzeczywistych adresów ani haseł. Tytuły notatek otrzymują prefiks `SN-<ID-testu>-<próba>`. Przed próbą zapisujemy listę identyfikatorów; po próbie usuwamy tylko własne dane testowe przez prawidłowo uwierzytelnione API z poprawnym tokenem. Nie używamy resetów Git ani usuwania całej bazy jako rutynowego sprzątania.

## Topologia CSRF

| Usługa | Planowany adres | Znaczenie |
|---|---|---|
| Badany wariant SecureNotes | `https://localhost:7028` | Origin aplikacji i cookie uwierzytelniające |
| Strona formularza | `https://localhost:7443` | Inny origin, ten sam site |
| Kontrola cross-site | `https://127.0.0.1:7443` | Inny site; wymaga certyfikatu dla adresu IP |

Strona atakująca powinna być otwierana jako samodzielna strona najwyższego poziomu, nie wewnątrz iframe ani przez `file://`. Serwer demonstracyjny może otrzymać cookie uwierzytelniające już przy otwarciu strony, ponieważ cookies nie są izolowane numerem portu. Musi być całkowicie lokalny i kontrolowany przez zespół; nie może zapisywać ani publikować nagłówków Cookie. Nie mieszamy hostów ani schematów w głównym doświadczeniu B01. SecureNotes na HTTPS localhost:7028 i serwer na HTTPS localhost:7443 mają różne originy, lecz ten sam site. Demonstracja nie dowodzi ataku z niezależnej domeny cross-site ani obejścia SameSite=Strict.

Obie usługi mają słuchać wyłącznie na interfejsach lokalnych. Nie używamy tuneli, publicznego hostingu ani danych spoza laboratorium. Plan nie wymaga globalnej zmiany CORS, SameSite, HttpOnly, Secure ani JWT.
