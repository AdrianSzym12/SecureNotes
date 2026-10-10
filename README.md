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
└── SecureNotes.Infrastructure.Tests/
    └── Testy infrastruktury i integracji
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
- `Permissions-Policy`.

Mechanizm pozostaje aktywny w aplikacji.

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
- przeglądarka internetowa obsługująca HTTPS i JavaScript.

## 7. Uruchamianie aplikacji

1. Sklonuj repozytorium i otwórz rozwiązanie SecureNotes w Visual Studio 2022.
2. Uruchom MariaDB.
3. Skonfiguruj połączenie z bazą danych oraz wymagane ustawienia JWT.
4. Dane wrażliwe, w szczególności hasła do bazy i klucz JWT, przechowuj poza repozytorium, np. w Visual Studio User Secrets.
5. Ustaw `SecureNotes.API` jako projekt startowy.
6. Uruchom aplikację przy użyciu HTTPS, naciskając F5.

Domyślnie otwierana jest strona główna:

`https://localhost:7028/`

Zachowanie to wynika z konfiguracji pliku `Properties/launchSettings.json`, w którym ustawiono:

```json
"launchUrl": ""
```

Port `7028` jest adresem wykorzystywanym w lokalnym środowisku projektowym. W innej konfiguracji może mieć inną wartość.

## 8. Swagger UI

Swagger/OpenAPI jest skonfigurowany dla środowiska `Development`.

Adres:

`https://localhost:7028/swagger/index.html`

**Znane ograniczenie:** aktualna restrykcyjna polityka CSP powoduje wyświetlanie pustej strony interfejsu Swagger UI.

Podczas diagnostyki stwierdzono, że problem ustępuje po tymczasowym wyłączeniu `SecurityHeadersMiddleware`. Nie jest to jednak rozwiązanie docelowe, ponieważ middleware odpowiada za ustawianie nagłówków bezpieczeństwa również dla pozostałych stron aplikacji.

W wersji przechowywanej w repozytorium `SecurityHeadersMiddleware` pozostaje aktywny.

Planowane rozwiązanie zakłada dostosowanie CSP wyłącznie do potrzeb Swagger UI w środowisku deweloperskim, bez osłabiania zabezpieczeń właściwego interfejsu SecureNotes.

## 9. Testowanie

W projekcie zaimplementowano testy jednostkowe i integracyjne obejmujące między innymi:

- walidację danych domenowych;
- rejestrację i logowanie;
- działanie serwisów aplikacyjnych;
- tworzenie i odczyt notatek;
- aktualizację i usuwanie notatek;
- sprawdzanie właściciela notatki;
- poprawność działania repozytoriów.

**Ostatni potwierdzony wynik: 82/82 zaliczonych testów automatycznych.**

Do uruchomienia testów integracyjnych wymagane jest odpowiednio przygotowane środowisko bazodanowe, w tym testowa baza MariaDB `securenotes_test_db`.

Testy można wykonać w Visual Studio za pomocą:

`Test → Test Explorer → Run All Tests`

W ramach ręcznej weryfikacji sprawdzano również kontrolę dostępu, mechanizmy CSRF oraz zachowanie aplikacji w przypadku prób dostępu do notatek innego użytkownika.

Końcowy audyt z wykorzystaniem OWASP ZAP oraz przygotowanie zbiorczego raportu bezpieczeństwa stanowią osobny etap projektu.

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
