# SecureNotes — porównanie BEFORE/AFTER

Dokumentacja projektu z przedmiotu **Bezpieczeństwo aplikacji internetowych**, stan na 16.10.2026. Temat karty projektu: P7 — Utwardzanie przeglądarki 2026 (Browser Hardening).

## Cel i zakres

Celem jest zbadanie jednej ścieżki Stored XSS w treści prywatnej notatki oraz CSRF przy tworzeniu notatki w lokalnym, autoryzowanym laboratorium. Badanie obejmuje rekonstrukcję podatności, porównanie z istniejącymi zabezpieczeniami i powtórzenie scenariuszy wobec wersji referencyjnej. Dozwolone są wyłącznie fikcyjne konta i dane oraz lokalne usługi. Cudze systemy, konta i dane pozostają poza zakresem.

**BEFORE jest kontrolowaną rekonstrukcją podatności, a nie historyczną wersją SecureNotes.** AFTER zawiera istniejące zabezpieczenia. Kolejność utworzenia wariantów nie jest chronologią wdrażania poprawek; porównanie pokazuje ich rolę eksperymentalnie.

| Wersja | Katalog | Branch | Rola |
|---|---|---|---|
| AFTER | `H:\Roboczy\BAI\SecureNotes-dev` | `develop` | Referencja tylko do odczytu |
| BEFORE | `H:\Roboczy\BAI\SecureNotes-BEFORE` | `demo/security-before` | Przyszła wersja demonstracyjna |

Bazowy commit obu wersji: `21be7bb6aadd69c729250eb5d5248cabb114de04`.

## Status i reguły interpretacji

- `VERIFIED` — potwierdzone odczytem aktualnego kodu lub kontrolą Git; nie oznacza testu działającej aplikacji.
- `PLANNED` — zaplanowana zmiana albo procedura.
- `NOT RUN` — test niewykonany. Oczekiwany wynik jest hipotezą, nie obserwacją.
- `DO WERYFIKACJI` — informacja wymagająca dodatkowego źródła lub decyzji.

**Aktualnie VERIFIED:** oba branche mają podany HEAD; przed utworzeniem dokumentacji ich status był czysty, a różnica zatwierdzonego kodu pusta. Oba warianty mają dziś te same zabezpieczenia. Cztery zmiany rekonstrukcyjne mają status `PLANNED`; wszystkie 15 nowych scenariuszy ma status `NOT RUN`. Codex w etapach dokumentacyjnych nie uruchamiał aplikacji, migracji ani testów.

**Potwierdzenie użytkownika — 03B:** referencyjne AFTER uruchomione z `develop` w Visual Studio 2022 działa z MariaDB/XAMPP i zapisuje notatki. Użytkownik zgłosił wynik testów automatycznych **85/85 (0 nieudanych, 0 pominiętych)** oraz obecność istniejących baz i tabel w phpMyAdmin. To nie są wyniki demonstracyjnych scenariuszy ani testy wykonane przez Codexa. Screenshotom nie przypisano jeszcze zarchiwizowanych plików lub ID. Szczegóły i miejsca do uzupełnienia są w [opisie środowiska](environment.md). Preferowany plan laboratorium używa nowych baz `securenotes_before_lab` i `securenotes_after_lab`, bez modyfikowania istniejących `securenotes_db` i `securenotes_test_db`.

## Scenariusze

**Stored XSS:** zapis nieszkodliwego payloadu w treści notatki, odczyt przez API i renderowanie po ponownym załadowaniu. Planowane BEFORE interpretuje treść jako HTML i dopuszcza handler zdarzenia wyłącznie na stronie notatek. AFTER używa `textContent` oraz restrykcyjnej CSP. Osobno oceniamy zapis, interpretację HTML, wykonanie JavaScript i trwałość. Prywatne notatki nie zapewniają automatycznie drogi do atakowania innych użytkowników.

**CSRF A:** trzy identyczne strukturalnie żądania JSON do `POST /api/notes`, różniące się tokenem Antiforgery: poprawny, błędny lub brakujący. To porównanie działania walidacji.

**CSRF B:** formularz bez tokenu ze strony `https://localhost:7443` do aplikacji `https://localhost:7028`. Są to różne originy, lecz ten sam site. Obsługa formularza będzie dodana wyłącznie w BEFORE jako powierzchnia laboratoryjna. Odrzucenia formularza przez AFTER nie przypisujemy wyłącznie Antiforgery: AFTER nie obsługuje tego formatu w tej akcji.

## Organizacja dokumentacji

| Dokument | Zawartość |
|---|---|
| [Środowisko](environment.md) | Wymagania, izolacja, uruchamianie, HTTPS i topologia |
| [Architektura](architecture.md) | Warstwy, diagram, przepływy i granice zaufania |
| [Model zagrożeń](threat-model.md) | Aktywa, warunki ataków, ryzyko i ograniczenia |
| [Rejestr zmian](change-register.md) | Cztery planowane zmiany i ich wpływ |
| [Plan testów](test-plan.md) | Kroki wszystkich 15 scenariuszy i przywracanie stanu |
| [Wyniki](results.md) | Wyniki oczekiwane, miejsce na obserwacje i indeks dowodów |
| [Mapowanie standardów](standards-mapping.md) | Top 10:2025, ASVS 5.0.0, WSTG 4.2 |
| [Raport końcowy — wersja robocza](final-report.md) | Merytoryczny raport z wyraźnym oddzieleniem hipotez |
| [Prezentacja](presentation.md) | Plan 11 minut oraz pokaz |
| [Dowody BEFORE](evidence/before/) / [AFTER](evidence/after/) | Obecnie puste katalogi z `.gitkeep` |

## Identyfikatory i źródła

Zmiany: `CHG-XSS-01/02`, `CHG-CSRF-01/02`. Testy: identyfikatory z [planu](test-plan.md). Dowód: `E-<ID-testu>-<BEFORE|AFTER>-R<próba>-<numer>`, np. `E-T-XSS-02-BEFORE-R01-01.png`. W indeksie zapisujemy SHA faktycznie badanego wariantu, datę, środowisko, opis i zakres anonimizacji.

Źródłami tej wersji są kod z bazowego commita, główny README repozytorium, `database/setup.sql`, bieżący audyt statyczny i lokalna karta `H:\Roboczy\BAI\Karta_projektu_BAI_Grupa_1.pdf`. Historyczne `docs/security-audit/` pozostaje niezmienione; jego wyniki nie są przenoszone do nowej serii.

Karta proponuje Node.js/Express, natomiast rzeczywista aplikacja używa ASP.NET Core 8. Zgodność zmienionego stosu z zaakceptowanym zakresem wymaga uzgodnienia. Dokumenty opisują rzeczywisty kod, nie proponowany stos. Szczegółowe dokumenty wewnętrzne pozostają poza repozytorium i nie są tworzone w tym etapie.
