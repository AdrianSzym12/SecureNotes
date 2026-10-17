# Lokalne laboratorium SecureNotes BEFORE

Użytkownik potwierdził w 04D rzeczywiste działanie laboratorium BEFORE na jednym Windows/PowerShell/MariaDB-XAMPP, w tym ponowny start po uproszczeniu skryptu. WhatIf bez zmian SQL i pobrania nowego hasła: PASS; setup konta i siedmiu praw do jednej bazy, bez DROP/GRANT OPTION i praw globalnych poza USAGE: PASS. Verify przed migracją: PASS dla .NET 8, dostępu, parametrów i grantów, oczekiwane NOT READY tabel. Pierwszy start z migracją InitialCreate, logowanie, zapis/odczyt i trwałość po restarcie: PASS. Uproszczony start: PASS — jeden maskowany monit, poprawny cel, ostrzeżenie, START BEFORE, HTTPS localhost:7028, Development i komunikat EF o aktualnej bazie. Są to potwierdzenia użytkownika, nie próby wykonane przez Codexa. Nie potwierdzono jeszcze drugiego komputera ani osobnej próby opcji -VerifyDatabase po uproszczeniu. Wszystkie 15 scenariuszy projektu nadal ma NOT RUN. Skrypty nie wdrażają podatności; BEFORE jest przeznaczone do kontrolowanej rekonstrukcji. Wariant docelowo podatny wolno uruchamiać wyłącznie lokalnie, na fikcyjnych danych.

## Aktualny sposób uruchamiania — 07A.2b

Samodzielna instrukcja dla nowego komputera, w tym GitHub, MariaDB, JWT/User Secrets, HTTPS i oddzielny AFTER: [LAB-SETUP.md](../../docs/before-after/LAB-SETUP.md). Użytkownik potwierdził pierwszy start BEFORE z monitem hasła, kolejny bez monitu, HTTPS 7028, bazę i aktualne migracje oraz start demo skryptem na 7443 i dostępność obu stron. To wyniki użytkownika na istniejącym stanowisku, nie odtworzenie drugiego komputera przez Codexa. Wyniki formalnych scenariuszy pozostają 6 PARTIAL — BEFORE / 9 NOT RUN. Powyższy opis 04D jest historyczny; bieżące launchery opisano poniżej.

Codzienna obsługa: `.\scripts\lab\start-before.ps1` oraz, tylko do CSRF w drugim terminalu, `.\scripts\lab\start-csrf-demo.ps1`. Nie uruchamiają się wzajemnie. Setup i verify zachowują rolę konfiguracji początkowej oraz opcjonalnej diagnostyki.

## Co jest rozdzielone

| Etap | Odpowiedzialność |
|---|---|
| Baza i konto MariaDB | `setup-before.ps1`: jedna baza i jedno konto, bez tabel aplikacji |
| Struktura tabel | Istniejące `Database.MigrateAsync()` w aplikacji, w Development |
| Konto użytkownika SecureNotes | Rejestracja przez interfejs aplikacji, po migracji |

AFTER pozostaje na istniejącej `securenotes_db` bez zmian. `securenotes_test_db` służy istniejącym testom automatycznym; żaden skrypt ich nie uruchamia. Jedyny cel administracyjny automatyzacji to **`securenotes_before_lab` i `sn_before_lab@127.0.0.1`**. Nie tworzymy dodatkowego AFTER, nie resetujemy baz, nie zmieniamy praw istniejących kont. Bazowy kod: `21be7bb6aadd69c729250eb5d5248cabb114de04`.

## Wymagane oprogramowanie

Windows, PowerShell 5.1 lub nowszy, Git, .NET 8 SDK i ASP.NET Core Runtime 8, działająca lokalna MariaDB oraz natywny klient `mysql.exe` lub `mariadb.exe`, przeglądarka i zaufany certyfikat HTTPS localhost. Nie wymagamy XAMPP ani konkretnej ścieżki instalacji. Skrypty niczego nie instalują. Mają być uruchamiane z interaktywnej konsoli, bez transkrypcji, debug trace i rejestrowania sekretów.

Na innym komputerze trzeba dostarczyć też wymaganą konfigurację JWT: Issuer, Audience, dodatnie ExpirationMinutes i SecretKey co najmniej 32-bajtowy. Skrypty nie generują klucza, nie odczytują User Secrets ani nie przenoszą sekretów z innego komputera. Sprawny przykład AFTER na jednym komputerze nie gwarantuje ich obecności na drugim. Konfigurację lokalną należy przygotować bez ujawniania wartości w repozytorium, logu lub argumentach poleceń.

## 1. Przygotuj serwer i klienta

Uruchom istniejącą lokalną MariaDB wybraną metodą administracyjną. Ustal port (domyślnie 3306) i bezpieczny dostęp administratora. Nie wystawiaj serwera na sieć publiczną. Klient może być w PATH; inaczej podaj `-MySqlPath` do faktycznie istniejącego pliku. Wszystkie połączenia skryptów wymuszają TCP i 127.0.0.1. `--no-defaults` pomija niejawne pliki klienta. Nie przekazuj hasła w argumentach.

Z katalogu głównego repozytorium ustaw tylko niepoufną ścieżkę, np.:

```powershell
$mysqlClient = 'C:\MariaDB\bin\mysql.exe' # przyklad; zastap swoja lokalizacja
```

Ścieżka jest przykładem, nie wymogiem. Preferowany port całego laboratorium to 3306. Setup i verify mają parametr Port do odczytu/przygotowania innej lokalnej instancji, ale start świadomie używa 3306: nie przygotowuj konta na innym porcie i nie uruchamiaj następnie aplikacji na 3306.

## 2. Sprawdź plan i przygotuj bazę/konto

```powershell
.\scripts\lab\setup-before.ps1 -MySqlPath $mysqlClient -WhatIf
```

WhatIf **łączy się odczytowo z serwerem** i wymaga hasła administratora, lecz nie wykonuje CREATE ani GRANT i nie pyta o nowe hasło konta. Hasło administratora wpisuje się w natywnym monicie klienta MariaDB; dla poprawnie skonfigurowanego konta bez hasła naciska się Enter. Nie zakładamy, że konto root zawsze ma puste hasło. Inny lokalny administrator: parametr `-AdminUser`.

Przeczytaj wynik, potem dopiero:

```powershell
.\scripts\lab\setup-before.ps1 -MySqlPath $mysqlClient
```

Rzeczywiste CREATE DATABASE i CREATE USER/GRANT wymagają ShouldProcess oraz dodatkowego wpisania odpowiednio `UTWORZ BAZE` i `UTWORZ KONTO`. Nowe hasło `sn_before_lab` jest pobierane dwa razy przez maskowany prompt. Nie jest generowane ani zapisywane przez skrypt. Każde wywołanie klienta może ponownie poprosić o **hasło administratora**, inne niż nowe hasło konta. Nie wpisuj hasła zamiast tekstu potwierdzenia.

Setup sprawdza istnienie dokładnej bazy i konta. Istniejącej bazy nie czyści; niezgodne kodowanie/kolacja powodują zatrzymanie bez ALTER. Istniejące konto musi mieć dokładnie siedem praw na escapowanej nazwie bazy: SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, INDEX. Grant używa zakresu `` `securenotes\_before\_lab`.* ``; podkreślenia nie mogą działać jak wildcard. Globalny USAGE oznacza istnienie konta i nie jest uprawnieniem administracyjnym. Każdy dodatkowy grant, rola, DROP, GRANT OPTION lub nieoczekiwany zakres powoduje zatrzymanie. Konta o tej nazwie dla innych hostów także wymagają ręcznej oceny. Skrypt nie naprawia praw i nie resetuje hasła.

### Przekazanie hasła i ograniczenia

SQL jest przesyłany klientowi przez standardowe wejście, nie przez argument procesu ani plik tymczasowy. Nowe hasło jest w pamięci przekształcane do formatu mysql_native_password (podwójny SHA-1 wymagany przez ten mechanizm MariaDB), a CREATE USER zawiera hash zamiast jawnego hasła. To protokół uwierzytelnienia MariaDB, nie wzorzec hashowania haseł aplikacji. Hash także jest poufny. Skrypt nie wyświetla surowego SHOW GRANTS, które może go zawierać, ani natywnego błędu mogącego powtórzyć SQL.

Nie gwarantujemy niewidoczności w pamięci procesu, pamięci serwera, instrumentacji systemowej ani logach/audycie MariaDB. Bufory bajtowe są czyszczone, lecz nie można zagwarantować wymazania wszystkich kopii stringów zarządzanych. Administrator przed zgodą powinien ocenić politykę logowania serwera. Skrypt jej nie zmienia. Połączenie jest wyłącznie lokalne, bez deklaracji gwarantowanego szyfrowania transportu SQL.

Natywny interaktywny prompt klienta wraz z SQL na stdin wymaga rzeczywistej konsoli Windows i musi zostać zweryfikowany na docelowym kliencie. Skrypt nie jest przeznaczony do CI ani hosta bez konsoli. Jeśli prompt nie działa, klient/plugin nie obsługuje tej metody lub nie można zaakceptować logowania hashów, **nie obchodź uwierzytelniania**: przygotuj konto ręcznie w phpMyAdmin, wpisując hasło wyłącznie lokalnie. Konto host=127.0.0.1, żadnych praw globalnych, siedem wskazanych praw tylko do konkretnej bazy z chronionymi podkreśleniami; potem uruchom verify. Nie używaj SQL z hasłem w historii klienta.

CREATE USER i GRANT nie są jedną transakcją. Po awarii między nimi może pozostać konto bez pełnych praw. Następne uruchomienie ma wtedy **zatrzymać się**, nie naprawiać ani resetować automatycznie. Administrator musi wyjaśnić i osobno zatwierdzić dalsze działanie. Ponowne uruchomienie na zgodnej konfiguracji wykonuje wyłącznie odczyty i zachowuje dane.

## 3. Zweryfikuj dostęp konta aplikacji

```powershell
.\scripts\lab\verify-before.ps1 -MySqlPath $mysqlClient
```

Podawaj interaktywnie hasło **sn_before_lab**, nigdy administratora. Skrypt sprawdza CURRENT_USER, DATABASE, wersję, kodowanie, kolację, dokładne granty, dostępne tabele i historię migracji. Wyświetla tabelę PASS / FAIL / NOT READY. Pusta baza przed pierwszą migracją daje **NOT READY dla tabel**, co jest poprawnym etapem przygotowania, nie błędem dostępu. Błędy połączenia, tożsamości i praw kończą skrypt błędem; istniejący częściowy schemat wymaga diagnozy, nie resetu. Po migracji oczekiwane są Users/Notes (na Windows mogą być małymi literami) i wpis InitialCreate w historii. Kontrola nie udowadnia kompletności wszystkich kolumn ani danych.

## 4. HTTPS i pierwszy start

Przed startem sprawdź certyfikat poleceniem odczytowym `dotnet dev-certs https --check --trust`. Jeżeli brak zaufanego certyfikatu, administrator/użytkownik musi osobno przygotować go standardowymi narzędziami .NET; skrypty nie tworzą i nie ufają certyfikatom automatycznie. Porty 7028 i 5070 muszą być wolne. Nie zastępuj HTTPS przez globalne wyłączenie Secure w cookies.

```powershell
.\scripts\lab\start-before.ps1
```

Standardowy start pyta o istniejące hasło sn_before_lab **tylko przy pierwszym lokalnym zapisie DPAPI**; kolejne starty odczytują je automatycznie. Nie wymaga `START BEFORE` i nie uruchamia verify-before.ps1 ani klienta MariaDB. Nadal sprawdza projekt, branch demo/security-before, SDK i ASP.NET Core Runtime 8, profil HTTPS/Development i brak sprzecznych ustawień. Wyświetla sztywny cel 127.0.0.1:3306 / securenotes_before_lab / sn_before_lab oraz ostrzeżenie o migracjach.

Zapis `%LOCALAPPDATA%\SecureNotesLab\before-db-password.dpapi` jest poza repozytorium i chroniony Windows DPAPI dla konta/komputera. `start-before.ps1 -ResetStoredPassword` pozwala ponownie podać hasło i zastąpić lokalny zapis; nie zmienia konta/hasła MariaDB. Błędny/uszkodzony/niedostępny zapis powoduje zatrzymanie bez automatycznego nadpisania; puste hasło nie zastępuje poprawnego. Zapis z innego komputera nie jest mechanizmem przenoszenia poświadczeń. DPAPI nie usuwa potrzeby krótkotrwałego jawnego hasła w pamięci i środowisku procesu aplikacji.

Opcjonalna pełna diagnostyka przed startem:

```powershell
.\scripts\lab\start-before.ps1 -VerifyDatabase
# Jezeli klient nie jest w PATH:
.\scripts\lab\start-before.ps1 -VerifyDatabase -MySqlPath $mysqlClient
```

Tylko `-VerifyDatabase` uruchamia istniejący verify-before.ps1, sprawdzający faktyczne konto, bazę, granty i schemat przed migracją. Ten tryb ma dodatkowe natywne monity uwierzytelniania; verify nie korzysta z DPAPI. Hasło do samej aplikacji pochodzi z lokalnego zapisu lub pierwszego maskowanego monitu. Parametr MySqlPath dotyczy tylko diagnostyki. Verify pozostaje niezależnym narzędziem. Bez przełącznika start potwierdza konfigurację procesu, ale nie sprawdza rzeczywistego połączenia SQL przed aplikacją. Pełną kontrolę wykonać po zmianie środowiska lub w razie problemów, nie przy każdym restarcie.

Procesowe Api__Server=127.0.0.1, Api__Port=3306, Api__Database=securenotes_before_lab, Api__User=sn_before_lab i Api__Password nadpisują wspólny UserSecretsId bez jego edycji. Hasło w środowisku procesu jest tekstowe w pamięci i jest dziedziczone przez dotnet oraz jego procesy potomne; nie uruchamiaj w tym kontekście AFTER, nie wypisuj środowiska. Finally przywraca poprzednie pięć wartości po zakończeniu/błędzie. Przy wymuszonym zabiciu hosta nie ma gwarancji wykonania finally; użyj dedykowanej sesji i zamknij ją po pracy.

W środku wykonywane jest `dotnet run --project <projekt ustalony wzgledem skryptu> --launch-profile https`. Polecenie może budować/przywracać zależności i zapisywać bin/obj **BEFORE**; skrypt nie instaluje narzędzi i nie dotyka AFTER. SDK 8 jest wymagane jako dostępne, ale przy kilku SDK domyślny wybór CLI może być nowszy. Profil ustawia Development niezależnie od IDE. Sprzeczne DOTNET_ENVIRONMENT lub nadpisania Api w profilu powodują zatrzymanie.

**Database.MigrateAsync() uruchomi obecne migracje i utworzy tabele.** Nie wywołujemy dotnet ef database update, nie tworzymy tabel ręcznie. EF nie zastępuje setupu konta i dostępu. Przy późniejszych migracjach nowe wymagania uprawnień trzeba ocenić osobno; DROP nie jest dostępny i skrypt nie realizuje rollbacku.

## 5. Przeglądarka, ponowienie i IDE

Po udanym starcie otwórz `https://localhost:7028/`, zarejestruj fikcyjne konto użytkownika aplikacji, zaloguj się, dodaj zwykłą notatkę. Jest to konto aplikacji, nie MariaDB. Nie wykonuj jeszcze scenariuszy XSS/CSRF bez ich osobnego zlecenia. Po zakończeniu zatrzymaj dotnet przez Ctrl+C i ponów verify: powinien pokazać historię migracji i tabele. Nie uruchamiaj całego zestawu testów infrastrukturalnych, który celuje w securenotes_test_db.

Preferowana metoda terminala i VS Code to start-before.ps1. Samo dotnet run bez przygotowania procesu może odziedziczyć bazę AFTER ze wspólnych sekretów; nie jest bezpiecznym skrótem. Visual Studio 2022 już otwarte wcześniej **nie odziedziczy** zmiennych z dowolnej sesji PowerShell. Nie przepisuj hasła do launchSettings ani wspólnych User Secrets, aby uruchomić F5. Automatyczne migracje zależą od Development, nie od Visual Studio.

Start wykonuje kontrole bez potwierdzenia tekstowego; setup nadal ma własne potwierdzenia i nie resetuje istniejących danych. Używaj osobnego profilu BEFORE, bo cookies localhost nie są izolowane portem. Po pracy wyloguj konto. Przygotowany serwer demonstracyjny na 7443 uruchamia osobny `start-csrf-demo.ps1`; [instrukcja demo](csrf-demo/README.md). Nie ma dodatkowego wspólnego launchera ani skryptu zapisu hasła.

## Błędy i granice

- Brak klienta: podaj faktyczną scieżkę MySqlPath. Brak serwera: sprawdź lokalną usługę/port, bez zmiany hosta na zdalny.
- Błąd dostępu: sprawdź, czy prompt dotyczy administratora (setup), czy sn_before_lab (verify/start). Nie wpisuj hasła w parametrze. Nie publikuj surowych logów.
- Nieoczekiwane granty, kolacja lub częściowy schemat: zatrzymaj się; nie ma automatycznej naprawy ani resetu.
- JWT lub baza przy starcie: sprawdź kategorię błędu lokalnie, nie wypisuj sekretów; nie osłabiaj walidacji JWT i nie uruchamiaj AFTER do diagnozy z tej sesji.
- Brak/niezaufany HTTPS lub zajęte porty: rozwiąż lokalnie przed kolejnym startem, nie zmieniaj zabezpieczeń aplikacji.

Poświadczenia muszą być dostarczane lokalnie. Użytkownik potwierdził także ponowne uruchomienie bieżącego uproszczonego startu; nie jest to dowód wykonania scenariuszy XSS/CSRF. Na innym komputerze klient i cała sekwencja nadal wymagają sprawdzenia, w tym lokalny certyfikat, dostępna konfiguracja JWT, granty i natywny prompt. Nie twierdzimy, że wynik jednego środowiska dowodzi pełnej przenośności. Archiwizacja rzeczywistych screenshotów, dat i metryk jest otwarta; nie nadano im fikcyjnych nazw/ID. [Dokumentacja środowiska](../../docs/before-after/environment.md) i [plan 15 testów](../../docs/before-after/test-plan.md) pozostają źródłem zakresu eksperymentu.
