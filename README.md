# SecureNotes

**SecureNotes** to aplikacja internetowa służąca do bezpiecznego tworzenia i przechowywania prywatnych notatek. Projekt został przygotowany w technologii ASP.NET Core (.NET 8) z wykorzystaniem architektury Onion Architecture.

Głównym celem projektu jest praktyczne zastosowanie mechanizmów bezpieczeństwa aplikacji internetowych, ze szczególnym uwzględnieniem uwierzytelniania, autoryzacji, ochrony sesji oraz zabezpieczeń przed podatnościami XSS, CSRF i IDOR.

## 1. Funkcjonalności

Aplikacja umożliwia:

- rejestrację nowego użytkownika;
- logowanie i wylogowanie;
- zarządzanie prywatną sesją użytkownika;
- tworzenie nowych notatek;
- wyświetlanie listy własnych notatek;
- odczytywanie pojedynczej notatki;
- edytowanie istniejących notatek;
- usuwanie notatek;
- dostęp wyłącznie do danych należących do aktualnie zalogowanego użytkownika.

Interfejs aplikacji jest responsywny i został przygotowany z wykorzystaniem HTML, CSS oraz JavaScript.

## 2. Technologie

| Obszar | Technologia |
|---|---|
| Backend | C#, ASP.NET Core Web API, .NET 8 |
| Architektura | Onion Architecture |
| Baza danych | MariaDB |
| ORM | Entity Framework Core 8, Pomelo |
| Frontend | HTML5, CSS3, JavaScript |
| Uwierzytelnianie | JWT |
| Przechowywanie sesji | Cookies HttpOnly |
| Testy | xUnit, testy jednostkowe i integracyjne |
| Dokumentacja API | Swagger / OpenAPI |
| Środowisko programistyczne | Visual Studio 2022 |
| Kontrola wersji | Git, GitHub |

## 3. Architektura projektu

Projekt wykorzystuje Onion Architecture, której celem jest oddzielenie logiki domenowej od szczegółów implementacyjnych i infrastruktury.

Struktura rozwiązania:

```text
SecureNotes/
│
├── SecureNotes.Domain/
│   └── Encje domenowe i reguły biznesowe
│
├── SecureNotes.Application/
│   └── Serwisy aplikacyjne, interfejsy i DTO
│
├── SecureNotes.Infrastructure/
│   └── Entity Framework Core, repozytoria i baza danych
│
├── SecureNotes.API/
│   └── Kontrolery, konfiguracja API i interfejs WWW
│
├── SecureNotes.Domain.Tests/
│   └── Testy logiki domenowej
│
├── SecureNotes.Infrastructure.Tests/
│   └── Testy infrastruktury i integracji
│
└── database/
    └── setup.sql
```

Takie podejście pozwala ograniczyć zależność najważniejszej logiki aplikacji od bazy danych, interfejsu użytkownika i frameworków.

## 4. Bezpieczeństwo aplikacji

Bezpieczeństwo jest głównym założeniem projektu SecureNotes.

### 4.1. Uwierzytelnianie i ochrona sesji

Aplikacja wykorzystuje tokeny JWT podpisywane algorytmem HMAC-SHA256.

Token uwierzytelniający jest przechowywany w ciasteczku `SecureNotes.Auth` z następującymi atrybutami:

- `HttpOnly` – uniemożliwia bezpośredni odczyt ciasteczka przez JavaScript;
- `Secure` – ogranicza przesyłanie ciasteczka do połączeń HTTPS;
- `SameSite=Strict` – ogranicza przesyłanie ciasteczka w kontekście innych witryn.

Hasła użytkowników nie są przechowywane w postaci jawnej. Do ich haszowania wykorzystywany jest mechanizm `PasswordHasher` platformy ASP.NET Core.

### 4.2. Ochrona przed CSRF

Aplikacja wykorzystuje mechanizm ASP.NET Core Antiforgery.

Żądania zmieniające stan aplikacji, takie jak rejestracja, logowanie, wylogowanie, tworzenie, edycja i usuwanie notatek, wymagają poprawnego tokenu CSRF.

Token jest przekazywany w nagłówku:

`X-CSRF-TOKEN`

### 4.3. Ochrona przed IDOR

Każda notatka jest przypisana do konkretnego użytkownika poprzez identyfikator `UserId`.

Operacje odczytu, edycji i usuwania sprawdzają właściciela notatki po stronie serwera. Użytkownik nie może uzyskać dostępu do cudzej notatki wyłącznie poprzez zmianę jej identyfikatora w adresie żądania.

W przypadku próby dostępu do notatki, która nie istnieje lub nie należy do aktualnego użytkownika, API zwraca odpowiedź `404 Not Found`.

### 4.4. Ochrona przed XSS

Treść notatek jest wyświetlana w interfejsie użytkownika przy użyciu bezpiecznych mechanizmów DOM, w szczególności `textContent`, zamiast interpretowania danych użytkownika jako kodu HTML.

Aplikacja wykorzystuje również restrykcyjną politykę Content Security Policy (CSP), która ogranicza wykonywanie nieautoryzowanych skryptów.

### 4.5. Nagłówki bezpieczeństwa

W projekcie zastosowano własny `SecurityHeadersMiddleware`, który ustawia między innymi:

- `Content-Security-Policy`;
- `X-Content-Type-Options: nosniff`;
- `X-Frame-Options: DENY`;
- `Referrer-Policy: no-referrer`;
- `Permissions-Policy`;
- `Cache-Control: no-store` dla odpowiedzi API.

Dodatkowo skonfigurowano HTTP Strict Transport Security (HSTS), aktywne poza środowiskiem Development, z polityką `max-age=2592000` (30 dni).

Mechanizmy bezpieczeństwa pozostają aktywne w aplikacji, a restrykcyjna polityka CSP nie została osłabiona.

## 5. Endpointy API

### Uwierzytelnianie

| Metoda | Endpoint | Opis |
|---|---|---|
| POST | `/api/auth/register` | Rejestracja użytkownika |
| POST | `/api/auth/login` | Logowanie |
| POST | `/api/auth/logout` | Wylogowanie |
| GET | `/api/auth/me` | Sprawdzenie aktualnej sesji |
| GET | `/api/auth/csrf-token` | Pobranie tokenu CSRF |

### Notatki

Wszystkie poniższe endpointy wymagają uwierzytelnienia.

| Metoda | Endpoint | Opis |
|---|---|---|
| GET | `/api/notes` | Pobranie własnych notatek |
| GET | `/api/notes/{id}` | Pobranie wybranej notatki |
| POST | `/api/notes` | Utworzenie notatki |
| PUT | `/api/notes/{id}` | Edycja notatki |
| DELETE | `/api/notes/{id}` | Usunięcie notatki |

Żądania POST, PUT i DELETE wymagają również poprawnego tokenu CSRF.

## 6. Wymagania

Do lokalnego uruchomienia projektu potrzebne są:

- Visual Studio 2022 z obsługą ASP.NET Core;
- .NET 8 SDK;
- MariaDB;
- XAMPP lub inna lokalna instalacja MariaDB;
- dostęp do phpMyAdmin lub innego klienta MariaDB;
- przeglądarka internetowa obsługująca HTTPS i JavaScript.

## 7. Uruchamianie aplikacji

### 7.1. Klonowanie repozytorium

1. Otwórz repozytorium SecureNotes na GitHubie.
2. Kliknij zielony przycisk **Code**.
3. W zakładce **HTTPS** skopiuj adres repozytorium.
4. Uruchom **Visual Studio 2022**.
5. Na ekranie startowym wybierz **Clone a repository (Klonuj repozytorium)**.
6. W polu **Repository location** wklej skopiowany adres GitHub.
7. W polu **Path** wybierz folder, w którym projekt ma zostać zapisany.
8. Kliknij **Clone (Klonuj)** i poczekaj na pobranie projektu.
9. Otwórz rozwiązanie SecureNotes w Visual Studio.
10. W menu **Git → Manage Branches** znajdź gałąź `develop`, kliknij ją prawym przyciskiem myszy i wybierz **Checkout**. Jeżeli gałąź jest widoczna tylko w sekcji zdalnej, wybierz `origin/develop` i utwórz z niej lokalną gałąź.

### 7.2. Przygotowanie baz danych

Przed pierwszym uruchomieniem aplikacji należy przygotować lokalne bazy MariaDB.

W repozytorium znajduje się skrypt:

`database/setup.sql`

Skrypt tworzy dwie bazy danych:

| Baza danych | Przeznaczenie |
|---|---|
| `securenotes_db` | Główna baza aplikacji |
| `securenotes_test_db` | Oddzielna baza testów automatycznych |

Tworzy również lokalnego użytkownika MariaDB `securenotes_migrator` i nadaje mu uprawnienia ograniczone do obu wskazanych baz.

**Instrukcja wykonania:**

1. Uruchom MariaDB, np. za pomocą XAMPP.
2. Otwórz phpMyAdmin i zaloguj się jako administrator MariaDB.
3. Otwórz plik `database/setup.sql`.
4. Skopiuj jego zawartość do zakładki **SQL** w phpMyAdmin.
5. Przed wykonaniem zastąp `UZUPELNIJ_WLASNE_HASLO` własnym hasłem użytkownika `securenotes_migrator`.
6. Kliknij **Wykonaj / Go**.
7. Sprawdź, czy obie bazy zostały utworzone, a zapytanie `SHOW GRANTS` pokazuje odpowiednie uprawnienia.

Hasło należy zmienić wyłącznie w lokalnej kopii wykonywanego skryptu. Nie zapisuj go w pliku `setup.sql` przeznaczonym do repozytorium.

Skrypt wykorzystuje `CREATE DATABASE IF NOT EXISTS` i nie usuwa istniejących tabel ani danych. Jeżeli użytkownik `securenotes_migrator` już istnieje, polecenie `CREATE USER IF NOT EXISTS` nie zmieni jego dotychczasowego hasła — w takim przypadku należy użyć hasła już przypisanego do tego konta lub zmienić je osobno jako lokalny administrator.

Skrypt przygotowuje bazy i uprawnienia. Strukturę tabel aplikacji tworzą migracje Entity Framework Core.

### 7.3. Konfiguracja aplikacji i User Secrets

Po przygotowaniu baz danych należy skonfigurować lokalne dane dostępowe do MariaDB oraz wymagane ustawienia JWT.

**User Secrets należy skonfigurować osobno w dwóch projektach:**

#### SecureNotes.API

1. W Visual Studio kliknij prawym przyciskiem myszy projekt `SecureNotes.API`.
2. Wybierz **Manage User Secrets (Zarządzaj kluczami tajnymi użytkownika)**.
3. W otwartym pliku `secrets.json` ustaw hasło użytkownika MariaDB:

```json
{
  "Api": {
    "Password": "TUTAJ_WLASNE_LOKALNE_HASLO"
  }
}
```

Hasło musi być zgodne z hasłem ustawionym dla użytkownika `securenotes_migrator` podczas wykonywania `database/setup.sql`.

W tym samym projekcie należy również skonfigurować wymagane sekrety JWT zgodnie z konfiguracją aplikacji. Nie należy usuwać istniejących poprawnych ustawień z pliku `secrets.json`.

#### SecureNotes.Infrastructure.Tests

1. Kliknij prawym przyciskiem myszy projekt `SecureNotes.Infrastructure.Tests`.
2. Wybierz **Manage User Secrets (Zarządzaj kluczami tajnymi użytkownika)**.
3. W jego osobnym pliku `secrets.json` ustaw:

```json
{
  "Api": {
    "Password": "TUTAJ_WLASNE_LOKALNE_HASLO"
  }
}
```

Testy infrastruktury korzystają z bazy `securenotes_test_db`, wskazanej w `appsettings.Test.json`. Hasło powinno być zgodne z danymi użytkownika `securenotes_migrator`.

**Ważne:** oba projekty mają oddzielne konfiguracje User Secrets. Ustawienie hasła wyłącznie w `SecureNotes.API` nie oznacza, że będzie ono automatycznie dostępne dla `SecureNotes.Infrastructure.Tests` — i odwrotnie.

W obu przykładach `TUTAJ_WLASNE_LOKALNE_HASLO` należy zastąpić hasłem wybranym podczas lokalnej konfiguracji MariaDB.

Nie należy umieszczać rzeczywistych haseł ani klucza JWT w plikach przeznaczonych do repozytorium GitHub.

### 7.4. Uruchomienie aplikacji

1. W Solution Explorer kliknij prawym przyciskiem myszy projekt `SecureNotes.API`.
2. Wybierz **Set as Startup Project**.
3. Wybierz profil uruchamiania HTTPS.
4. Uruchom aplikację za pomocą **F5** lub przycisku **Start** w Visual Studio.
5. Po uruchomieniu otwórz w przeglądarce:

`https://localhost:7028/`

Podczas pierwszego lokalnego uruchomienia może być konieczne zaufanie certyfikatowi deweloperskiemu ASP.NET Core.

Domyślnie otwierana jest strona główna aplikacji. Zachowanie to wynika z konfiguracji pliku `Properties/launchSettings.json`, w którym ustawiono:

```json
"launchUrl": ""
```

Port `7028` jest adresem wykorzystywanym w lokalnym środowisku projektowym. W innej konfiguracji numer portu może się różnić.

Po poprawnym uruchomieniu można zarejestrować konto testowe, zalogować się i rozpocząć pracę z prywatnymi notatkami.

## 8. Swagger UI

Swagger/OpenAPI jest skonfigurowany dla środowiska `Development`.

Adres interfejsu:

`https://localhost:7028/swagger/index.html`

**Znane ograniczenie:** aktualna restrykcyjna polityka CSP powoduje wyświetlanie pustej strony interfejsu Swagger UI.

Podczas diagnostyki stwierdzono, że problem ustępuje po tymczasowym wyłączeniu `SecurityHeadersMiddleware`. Nie jest to jednak rozwiązanie docelowe, ponieważ middleware odpowiada za ustawianie nagłówków bezpieczeństwa również dla pozostałych stron aplikacji.

Podjęto decyzję o zachowaniu restrykcyjnej polityki CSP bez dodawania wyjątków osłabiających zabezpieczenia aplikacji.

Dokumentacja OpenAPI w formacie JSON pozostaje dostępna pod adresem:

`https://localhost:7028/swagger/v1/swagger.json`

## 9. Testowanie

W projekcie zaimplementowano testy jednostkowe i integracyjne obejmujące między innymi:

- walidację danych domenowych;
- rejestrację i logowanie;
- działanie serwisów aplikacyjnych;
- tworzenie i odczyt notatek;
- aktualizację i usuwanie notatek;
- sprawdzanie właściciela notatki;
- poprawność działania repozytoriów;
- weryfikację konfiguracji HSTS.

**Ostatni potwierdzony wynik: 85/85 zaliczonych testów automatycznych.**

Testy integracyjne wymagają działającej MariaDB, bazy `securenotes_test_db` oraz poprawnie skonfigurowanego hasła w User Secrets projektu testowego. Bazę i uprawnienia można przygotować za pomocą `database/setup.sql`, zgodnie z sekcją 7.2.

Testy można wykonać w Visual Studio za pomocą:

`Test → Test Explorer → Run All Tests`

W ramach ręcznej weryfikacji sprawdzano również kontrolę dostępu, mechanizmy CSRF oraz zachowanie aplikacji w przypadku prób dostępu do notatek innego użytkownika.

Szczegółowa dokumentacja przeprowadzonych testów bezpieczeństwa, działań naprawczych i ich wyników znajduje się w katalogu [`docs/security-audit/`](docs/security-audit/).

## 10. Organizacja pracy z Git

Projekt rozwijany jest przy użyciu systemu kontroli wersji Git i repozytorium GitHub.

Główne gałęzie:

- `main` – główna gałąź projektu;
- `develop` – gałąź integracyjna;
- `feature/*` – implementacja nowych funkcjonalności;
- `fix/*` – poprawki błędów;
- `chore/*` – porządkowanie projektu;
- `docs/*` – zmiany dokumentacji.

Zmiany przygotowywane są na osobnych branchach, a następnie scalane z `develop` za pomocą Pull Request.

## 11. Cel edukacyjny

SecureNotes jest projektem edukacyjnym realizowanym w celu rozwijania praktycznych umiejętności z zakresu tworzenia i zabezpieczania aplikacji internetowych.

Projekt pozwala przeanalizować zagrożenia charakterystyczne dla aplikacji Web API, wdrożyć mechanizmy ograniczające ryzyko podatności oraz zweryfikować ich działanie poprzez testy automatyczne i manualne.

Aplikacja jest przeznaczona do lokalnych testów i demonstracji zastosowanych mechanizmów bezpieczeństwa.