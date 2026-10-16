# Plan prezentacji i demonstracji

Status: `PLANNED`, wszystkie nowe testy `NOT RUN`. Planowany czas: **11 minut**, z tolerancją 1 minuty w zakresie 10–12 minut. Nie przypisujemy jeszcze zadań członkom zespołu. Baza projektu: `21be7bb6aadd69c729250eb5d5248cabb114de04`.

## Slajdy i czas

| Czas | Slajd / część | Treść i pokaz |
|---|---|---|
| 0:00–0:45 | 1. Temat i cel | Jedna ścieżka Stored XSS i tworzenie notatki; własne laboratorium; BEFORE jako rekonstrukcja, AFTER jako istniejąca referencja |
| 0:45–1:45 | 2. Architektura | Uproszczony diagram z architecture.md, zapis/odczyt i granice zaufania; JWT w cookie, właściciel z claims |
| 1:45–2:30 | 3. Model zagrożeń | Prywatna notatka, drugi origin, aktywna sesja; ograniczenie dostarczenia między kontami; wpływ na integralność |
| 2:30–5:00 | 4. Stored XSS | T-XSS-02: zapisany payload, ponowne pobranie, DOM, alert/znacznik w BEFORE i tekst w AFTER; T-XSS-03 wspiera trwałość |
| 5:00–6:30 | 5. CSRF A | T-CSRF-A01, T-CSRF-A02, T-CSRF-A03: kontrola poprawnego tokenu, brak/błąd; strukturalnie te same żądania JSON i stan danych |
| 6:30–8:30 | 6. CSRF B | T-CSRF-B01: strona HTTPS na drugim porcie wysyła formularz bez tokenu; Network i nowa notatka; asymetria obsługi formularza w AFTER |
| 8:30–9:30 | 7. Zachowane bariery | T-REG-04: flagi cookies i lokalny zakres wyjątku CSP; różnica origin/site, znaczenie CORS/preflight |
| 9:30–11:00 | 8. Porównanie i wnioski | Cztery zmiany, tabela rzeczywistych wyników po testach, ograniczenia, mapowanie standardów i odpowiedź na hipotezy |

Sześć głównych testów odpowiada wyborowi z [test-plan.md](test-plan.md). Pozostałe próby dostarczają dowodów pomocniczych/regresyjnych; ich pełny przebieg nie mieści się w pokazie. Nie prezentować oczekiwanej tabeli jako uzyskanych wyników.

## Przygotowanie przed pokazem

1. Dopiero po wykonaniu testów wybrać potwierdzone próby i ID dowodów z [results.md](results.md).
2. Zapisać SHA obu badanych wersji, przygotować fikcyjne konta i notatki oraz sprawdzić certyfikaty, porty i izolację bazy.
3. Przygotować osobne profile przeglądarki dla wariantów. Uruchamiać aplikacje kolejno albo wcześniej uzgodnić izolację, nie współdzielić cookies przypadkowo.
4. Włączyć zachowanie Network, przygotować filtry do API i bezpieczne widoki nagłówków. Ukryć wartości cookie i tokenów.
5. Przećwiczyć czas i sprzątanie. Nie wyłączać CSP, CORS ani flag cookies w celu ratowania pokazu.

## Kolejność demonstracji

XSS: pokazać odpowiedź GET z zapisaną treścią, następnie efekt i ponowne załadowanie; w AFTER wskazać tekst oraz brak elementu z payloadu. Wyjaśnić, że sam uszkodzony obraz jest dowodem HTML, a alert/znacznik dowodem kodu. Jeżeli pokazujemy etap pośredni CSP, korzystać z odrębnie zarejestrowanego wyniku T-XSS-04, bez edytowania referencji na żywo.

CSRF A: przedstawić trzy żądania o tym samym formacie i ważnym uwierzytelnieniu. Zamiast wypisywania tokenu pokazać tylko jego obecność lub brak i odpowiedź wraz ze stanem danych. Odróżnić zgodność eksperymentu od skuteczności zabezpieczenia.

CSRF B: pokazać pasek adresu localhost:7443, formularz bez tokenu i faktyczny POST do localhost:7028. Po powrocie do aplikacji potwierdzić notatkę przez świeży odczyt. Dla AFTER opisać rzeczywiście ustaloną przyczynę odmowy. Powiedzieć wprost, że dodany adapter formularza oznacza różną powierzchnię wariantów, więc ten pokaz nie izoluje Antiforgery.

## Wnioski i pytania

Należy umieć wyjaśnić: dlaczego HttpOnly nie blokuje CSRF; dlaczego Strict dopuszcza opisany kontekst same-site; dlaczego CORS nie jest ogólną ochroną formularza; dlaczego `innerHTML` i CSP to dwie bariery; dlaczego prywatne notatki nie dowodzą ataku na inne konto. Podkreślić, że AFTER już zawiera zabezpieczenia, a projekt rekonstruuje ich brak, zamiast tworzyć fałszywą historię remediacji.

## Plan awaryjny

Jeśli próba na żywo nie działa, sprawdzić kolejno certyfikat i HTTPS, ważność sesji, wysłanie cookie, format danych, routing, token i CSP. Nie traktować awarii jako wyniku bezpieczeństwa. Można pokazać wcześniejsze, rzeczywiste nagranie z ID próby, datą i SHA, wyraźnie nazywając je nagraniem. Jeśli takich dowodów brak, przedstawić plan oraz status NOT RUN i ograniczenie prezentacji; nie tworzyć zastępczych wyników. Ostateczne slajdy i wnioski są **DO UZUPEŁNIENIA PO TESTACH**.
