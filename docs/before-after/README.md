# SecureNotes — porównanie BEFORE/AFTER

Dokumentacja projektu z przedmiotu **Bezpieczeństwo aplikacji internetowych**, stan na 17.10.2026 (06F). Temat karty projektu: P7 — Utwardzanie przeglądarki 2026 (Browser Hardening).

Instrukcja odtworzenia po pobraniu z GitHuba: **[LAB-SETUP.md](LAB-SETUP.md)**. Obejmuje Windows/.NET/MariaDB, lokalne JWT/User Secrets i HTTPS, dwa launchery BEFORE/demo, DPAPI oraz oddzielną konfigurację AFTER. Nowe skrypty i instrukcja muszą zostać wcześniej opublikowane; nie zakładamy ich obecności na zdalnej gałęzi. Potwierdzenie działania launcherów na obecnym komputerze pochodzi od użytkownika, nie jest testem odtworzenia na nowym stanowisku ani zmianą wyników 15 scenariuszy.

## Cel i zakres

Celem jest zbadanie jednej ścieżki Stored XSS w treści prywatnej notatki oraz CSRF przy tworzeniu notatki w lokalnym, autoryzowanym laboratorium. Badanie obejmuje rekonstrukcję podatności, porównanie z istniejącymi zabezpieczeniami i powtórzenie scenariuszy wobec wersji referencyjnej. Dozwolone są wyłącznie fikcyjne konta i dane oraz lokalne usługi. Cudze systemy, konta i dane pozostają poza zakresem.

**BEFORE jest kontrolowaną rekonstrukcją podatności, a nie historyczną wersją SecureNotes.** AFTER zawiera istniejące zabezpieczenia. Kolejność utworzenia wariantów nie jest chronologią wdrażania poprawek; porównanie pokazuje ich rolę eksperymentalnie.

| Wersja | Katalog | Branch | Rola |
|---|---|---|---|
| AFTER | `H:\Roboczy\BAI\SecureNotes-dev` | `develop` | Referencja tylko do odczytu |
| BEFORE | `H:\Roboczy\BAI\SecureNotes-BEFORE` | `demo/security-before` | Wdrożona wersja demonstracyjna, częściowe wyniki użytkownika |

Bazowy commit obu wersji: `21be7bb6aadd69c729250eb5d5248cabb114de04`.

## Status i reguły interpretacji

- `VERIFIED` — potwierdzone odczytem aktualnego kodu lub kontrolą Git; nie oznacza testu działającej aplikacji.
- `PLANNED` — zaplanowana zmiana albo procedura.
- `NOT RUN` — test niewykonany. Oczekiwany wynik jest hipotezą, nie obserwacją.
- `DO WERYFIKACJI` — informacja wymagająca dodatkowego źródła lub decyzji.

**Stan 06F, 17.10.2026:** bazowy SHA pozostaje HEAD referencji develop. Wszystkie cztery zmiany rekonstrukcyjne są IMPLEMENTED / PARTIALLY VERIFIED w opisanym zakresie BEFORE na podstawie prób użytkownika. Matryca zawiera 6 PARTIAL — BEFORE i 9 NOT RUN; AFTER nie badano, a końcowe porównania pozostają na próbę generalną. Szczegóły: [wyniki](results.md), [rejestr zmian](change-register.md), [lokalna demonstracja CSRF](../../scripts/lab/csrf-demo/README.md). Codex nie wykonywał prób aplikacji, HTTP, SQL ani migracji. Poniższe wpisy 03B/04A opisują historię przygotowania.

**Potwierdzenie użytkownika — 03B:** referencyjne AFTER uruchomione z `develop` w Visual Studio 2022 działa z MariaDB/XAMPP i zapisuje notatki. Użytkownik zgłosił wynik testów automatycznych **85/85 (0 nieudanych, 0 pominiętych)** oraz obecność istniejących baz i tabel w phpMyAdmin. To nie są wyniki demonstracyjnych scenariuszy ani testy wykonane przez Codexa. Screenshotom nie przypisano jeszcze zarchiwizowanych plików lub ID. Szczegóły i miejsca do uzupełnienia są w [opisie środowiska](environment.md).

**Uproszczenie środowiska — 04A:** AFTER pozostaje gotową referencją z `securenotes_db`; nie tworzymy dla niego dodatkowej bazy ani środowiska. Dla BEFORE utworzono wyłącznie `securenotes_before_lab`, potwierdzając `utf8mb4`, `utf8mb4_general_ci` i brak tabel. Nie wykonano migracji ani nadania uprawnień. Istniejące `securenotes_db` i `securenotes_test_db` pozostały poza zakresem zmian; druga zachowuje rolę bazy testów automatycznych. BEFORE jest wariantem przeznaczonym do celowej rekonstrukcji, lecz podatności nadal są tylko planowane. Wszystkie 15 scenariuszy pozostaje `NOT RUN`.

## Scenariusze

**Stored XSS:** zapis nieszkodliwego payloadu w treści notatki, odczyt przez API i renderowanie po ponownym załadowaniu. Planowane BEFORE interpretuje treść jako HTML i dopuszcza handler zdarzenia wyłącznie na stronie notatek. AFTER używa `textContent` oraz restrykcyjnej CSP. Osobno oceniamy zapis, interpretację HTML, wykonanie JavaScript i trwałość. Prywatne notatki nie zapewniają automatycznie drogi do atakowania innych użytkowników.

**CSRF A:** trzy identyczne strukturalnie żądania JSON do `POST /api/notes`, różniące się tokenem Antiforgery: poprawny, błędny lub brakujący. To porównanie działania walidacji.

**CSRF B:** formularz bez tokenu ze strony `https://localhost:7443` do aplikacji `https://localhost:7028`. Są to różne originy, lecz ten sam site. Obsługa formularza będzie dodana wyłącznie w BEFORE jako powierzchnia laboratoryjna. Odrzucenia formularza przez AFTER nie przypisujemy wyłącznie Antiforgery: AFTER nie obsługuje tego formatu w tej akcji.

## Organizacja dokumentacji

[Instrukcja skryptów laboratorium](../../scripts/lab/README.md) opisuje setup-before.ps1, start-before.ps1 i verify-before.ps1. W 04D użytkownik potwierdził PASS dla WhatIf bez zmian SQL, setupu z ograniczonym kontem oraz pierwszego startu i migracji. Verify przed migracją potwierdził .NET 8, dostęp, parametry i granty; NOT READY tabel było oczekiwane. Ponowne uruchomienie uproszczonym startem z jednym maskowanym hasłem, START BEFORE i HTTPS/Development zakończyło się PASS, z bazą aktualną według komunikatu EF Core. Logowanie, zapis/odczyt i trwałość danych po restarcie zostały potwierdzone przez użytkownika. Pełny zakres obserwacji jest w [environment.md](environment.md); nie są to próby wykonane przez Codexa ani wyniki 15 scenariuszy bezpieczeństwa. Nie potwierdzono odtworzenia na drugim komputerze ani osobnego przebiegu -VerifyDatabase po uproszczeniu. Archiwizacja screenshotów, dat i metryk prób pozostaje otwarta, bez fikcyjnych nazw/ID.

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
