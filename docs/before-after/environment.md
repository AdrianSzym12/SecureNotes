# Środowisko laboratoryjne

Stan dokumentu: `VERIFIED` dla deklaracji kodu; działanie referencyjnego AFTER potwierdzone przez użytkownika w etapie 03B; izolowane laboratorium BEFORE/AFTER: `PLANNED`; wszystkie 15 testów demonstracyjnych: `NOT RUN`. Baza: `21be7bb6aadd69c729250eb5d5248cabb114de04`.

## Potwierdzenie środowiska referencyjnego AFTER — 03B

Użytkownik przekazał potwierdzenie uruchomienia SecureNotes z brancha `develop` w Visual Studio 2022 oraz informację o screenshotach. Zgodnie z tym potwierdzeniem:

- aplikacja uruchamia się, łączy z MariaDB w XAMPP i poprawnie zapisuje notatki;
- Test Explorer pokazuje **85 zaliczonych testów automatycznych, 0 nieudanych, 0 pominiętych (85/85)** dla wersji referencyjnej AFTER;
- phpMyAdmin potwierdza bazy `securenotes_db` i `securenotes_test_db` oraz tabele widoczne jako `users`, `notes`, `__efmigrationshistory`.

Źródłem jest relacja użytkownika w Prompt 03B, nie wykonanie testów przez Codexa ani niezależne sprawdzenie bazy w tym etapie. Nie ustalono tutaj dokładnej daty uruchomienia i SHA artefaktu użytego do tego przebiegu; bazowy SHA dokumentacji nie zastępuje takiej metryki. Screenshoty nie zostały w tym etapie fizycznie zapisane i zweryfikowane w katalogach dowodowych; nie przypisano im zarchiwizowanych nazw ani ID. Miejsce na późniejszą metrykę: data przebiegu, pełny SHA, ścieżki zweryfikowanych plików i ID dowodów — **DO UZUPEŁNIENIA PO ARCHIWIZACJI**.

To potwierdzenie gotowości istniejącej referencji i jej testów automatycznych. Nie oznacza wykonania żadnego z 15 scenariuszy demonstracyjnych XSS/CSRF i regresyjnych tej serii; wszystkie pozostają `NOT RUN`. Nie potwierdza też jeszcze działania nowych baz laboratoryjnych.

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

Wartości poufne powinny pozostać w lokalnej konfiguracji poza repozytorium. Nie należy publikować zrzutów ustawień ani wyników poleceń wypisujących sekrety. Oba worktree mają ten sam `UserSecretsId` projektu API, zatem nie zapewniają automatycznie rozdzielenia konfiguracji. Preferowany plan przewiduje dwie nowe bazy i jawne, procesowe nadpisanie `Api__Database`: `securenotes_before_lab` dla BEFORE i `securenotes_after_lab` dla AFTER. Nie zmieniamy plików ani zapisanej konfiguracji referencyjnego AFTER. Nadpisanie dotyczy wyłącznie przyszłej, osobno zatwierdzonej sesji uruchomieniowej.

`PersistenceService.AddPersistence` ustawia `SslMode=Preferred`; sam zapis nie dowodzi szyfrowania połączenia z bazą. Zakres eksperymentu pozostaje lokalny.

## Baza i migracje

`database/setup.sql` przygotowuje `securenotes_db` i `securenotes_test_db`, obie z `utf8mb4`, oraz lokalnego użytkownika `securenotes_migrator`. Uprawnienia obejmują operacje na danych i schemacie obu baz. Skrypt nie tworzy tabel aplikacji. Migracja `20261008210341_InitialCreate` tworzy `Users`, `Notes`, indeksy i relację właściciela.

**Uruchomienie Development modyfikuje bazę:** `SecureNotes.API/Program.cs:48–55` wykonuje `Database.MigrateAsync()`. Przed przyszłym startem trzeba potwierdzić docelową bazę, jej laboratoryjny charakter i sposób odtworzenia. Nie należy uruchamiać aplikacji tylko w celu sprawdzenia, czy się kompiluje. Testowa `DatabaseFixture.InitializeAsync` również wykonuje migracje i dopuszcza wyłącznie nazwę `securenotes_test_db`.

Istniejące `securenotes_db` i `securenotes_test_db` należy zachować bez zmian podczas przygotowania laboratorium: nie wykonywać na nich resetów, czyszczenia, nowych migracji ani ponownych testów w ramach tej procedury. Preferowane są nowe `securenotes_before_lab` i `securenotes_after_lab` na tym samym serwerze MariaDB. Nie należy uruchamiać obecnego `database/setup.sql` bez zmian jako procedury ich przygotowania — wskazuje istniejące bazy. Potrzebna będzie osobno zatwierdzona procedura administracyjna ograniczona do nowych nazw i odpowiednich uprawnień; nie wolno zakładać, że obecne uprawnienia je obejmują.

W etapie 03B Codex nie tworzył baz, nie wykonywał migracji, SQL, testów, kont ani czyszczenia danych. Relacja użytkownika dotyczy istniejącego środowiska AFTER. Fixture testów infrastrukturalnych wymaga `securenotes_test_db`, dlatego nie należy uruchamiać go w ramach przygotowania nowych laboratoriów ani zakładać, że samo `Api__Database` przeniesie ten zestaw na nową nazwę.

## Plan uruchomienia

1. Potwierdzić branch i SHA badanego katalogu oraz zapisać je w protokole próby.
2. Po osobnym zatwierdzeniu przygotować nowe bazy laboratoryjne i uprawnienia, bez ingerencji w `securenotes_db` i `securenotes_test_db`; potwierdzić właściwy cel przed jakąkolwiek migracją.
3. Ustawić lokalnie wymaganą konfigurację bez zapisywania sekretów w dokumentacji.
4. Potwierdzić zaufanie certyfikatowi deweloperskiemu i wolne porty.
5. W przyszłym, zatwierdzonym etapie uruchomić wybrany wariant z właściwym `Api__Database`. Dla BEFORE można użyć profilu HTTPS; dla nietykalnego katalogu AFTER dobrać zweryfikowany artefakt i sposób startu bez zapisu w tym katalogu. Zwykłe budowanie lub `dotnet run` może zmieniać bin/obj i nie jest automatycznie dozwolone w referencji.
6. Otworzyć `https://localhost:7028/`; zarejestrować rzeczywisty adres i środowisko.
7. Utworzyć fikcyjne konta oraz przejść zwykłe logowanie i utworzenie notatki z poprawnym tokenem.

To plan późniejszego uruchomienia na nowych bazach, nie opis już przygotowanego laboratorium. Potwierdzenie użytkownika powyżej dotyczy istniejącego AFTER. Profil `https` deklaruje również `http://localhost:5070`; do demonstracji używamy wyłącznie HTTPS. Swagger jest włączany w Development, lecz README opisuje ograniczenie jego UI przez CSP. Nie osłabiamy CSP dla Swaggera. Istniejący JSON OpenAPI może służyć pomocniczo po uruchomieniu.

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
