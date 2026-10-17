# Model zagrożeń i jakościowa ocena ryzyka

Podstawa: audyt kodu `21be7bb6aadd69c729250eb5d5248cabb114de04` (`VERIFIED`). Stan 05D: CHG-XSS-01 wdrożono i częściowo zweryfikowano przez użytkownika; CHG-XSS-02 IMPLEMENTED / PARTIALLY VERIFIED — użytkownik potwierdził wykonanie JS w BEFORE (wyjątek inline handlers tylko dla dokładnej ścieżki `/notes.html`). Pozostałe zmiany są PLANNED. Historyczne T-XSS-02/04 mają PARTIAL — BEFORE; pozostałe 13 scenariuszy NOT RUN. Wyjątek dopuszcza wszystkie inline event handlers na tej stronie, co jest celowym ryzykiem laboratoryjnym; wykonanie JavaScript w BEFORE potwierdził użytkownik 17.10.2026. Rzeczywiste nagłówki innych ścieżek i zachowanie AFTER wymagają osobnej weryfikacji. Ocena jest jakościowa; nie obliczamy CVSS bez uzgodnionej metodologii.

## Zasoby, aktorzy i zakres

Chronione zasoby to treść i integralność prywatnych notatek, konto użytkownika, zdolność wykonywania operacji w jego kontekście, JWT, hasła, konfiguracja i baza danych. Dostępność API i czytelność interfejsu są zasobami pomocniczymi.

Potencjalni aktorzy obejmują użytkownika wprowadzającego spreparowaną treść, operatora strony na innym originie oraz nieuwierzytelnionego klienta API. W laboratorium wszystkie te role odgrywa uprawniony zespół na fikcyjnych kontach. Nie zakładamy, że użytkownik A może zapisać notatkę użytkownikowi B. Administrator hosta i kradzież klucza JWT nie są przedmiotem tego eksperymentu.

Powierzchnia obejmuje pole treści, odpowiedź JSON i DOM listy, `POST /api/notes`, cookie uwierzytelnienia, token Antiforgery oraz lokalną stronę formularza. Operacje PUT/DELETE i autoryzacja właściciela są objęte regresją, a nie celowym osłabieniem.

## Zagrożenia i wymagane warunki

| Zagrożenie | Warunki | Możliwy wpływ | Bariery AFTER | Plan BEFORE |
|---|---|---|---|---|
| Stored XSS | Payload zapisany, ponownie pobrany i interpretowany jako HTML; handler dopuszczony przez CSP | Poufność: dostęp kodu do danych dostępnych stronie; integralność: operacje w sesji; dostępność: zakłócenie interfejsu | `textContent`, zakaz handlerów w CSP, bezpieczne `.value` | CHG-XSS-01 i CHG-XSS-02 |
| CSRF JSON | Ważne uwierzytelnienie, poprawny JSON i brak skutecznej walidacji intencji | Integralność: utworzenie niechcianej notatki; brak wykazanej kradzieży danych | Antiforgery, format JSON, polityka originów | CHG-CSRF-01; doświadczenie A izoluje token |
| CSRF formularzowy | Aktywna sesja, strona innego originu, same-site HTTPS, akceptacja formularza bez tokenu | Integralność: zapis jako zalogowany właściciel | Antiforgery i brak akcji formularzowej; Strict ogranicza osobny przypadek cross-site | CHG-CSRF-02 dodaje powierzchnię laboratoryjną |

HttpOnly chroni przed bezpośrednim odczytem cookie przez JavaScript, nie przed wysyłaniem uwierzytelnionych żądań. Secure wymaga HTTPS. SameSite dotyczy site, a nie pełnego originu. Origin obejmuje schemat, host i port; dwa porty HTTPS `localhost` są różnymi originami, lecz tym samym site. Nie planuje się usuwania tych zabezpieczeń. W doświadczeniu B SecureNotes działa na `https://localhost:7028`, a serwer demonstracyjny na `https://localhost:7443`. Cookies nie są izolowane numerem portu, więc serwer demonstracyjny może otrzymać cookie uwierzytelniające już przy otwarciu strony. Musi być całkowicie lokalny, kontrolowany przez zespół i nie może zapisywać ani publikować nagłówków Cookie. Scenariusz nie dowodzi ataku z niezależnej domeny cross-site ani obejścia SameSite=Strict.

## Ocena jakościowa i priorytety

Stosujemy oceny niski/średni/wysoki oddzielnie dla możliwego wpływu i wykazanej ekspozycji. Priorytet wynika z połączenia wpływu i realnych warunków dostarczenia, bez mnożenia arbitralnych liczb.

| Obszar | Potencjalny wpływ | Ekspozycja w modelu laboratoryjnym | Priorytet i uzasadnienie |
|---|---|---|---|
| Wykonanie zapisanego JavaScript | Wysoki dla kontekstu aktywnej strony | Ograniczona do własnej prywatnej notatki; brak potwierdzonej drogi między kontami | Wysoki edukacyjnie: trzeba rozdzielić renderer i CSP; nie oznacza wykazanego wysokiego ryzyka między użytkownikami |
| Utworzenie notatki przez formularz | Średni dla integralności danych testowych | Wymaga aktywnej sesji i same-site originu; powierzchnia jest dodawana celowo | Wysoki edukacyjnie: rzeczywiste żądanie przeglądarki i precyzyjne przypisanie przyczyny |
| Cross-site przy SameSite=Strict | Skutek zależny od operacji | Przewidywana blokada cookie w opisanym POST | Kontrola pomocnicza; potwierdza granice scenariusza |
| Niepoprawna interpretacja dowodów | Wysoki dla wiarygodności projektu | Możliwa przy ocenianiu samego statusu HTTP lub DOM | Wysoki: osobne doświadczenia A/B i jawne pola wyników |

W aktualnej bazie planowane osłabienia nie istnieją. Nie stwierdzamy podatności działającej aplikacji wyłącznie na podstawie możliwości jej przyszłego wprowadzenia. Nie deklarujemy też pełnej odporności AFTER bez testów.

## Ograniczenia i ryzyko rezydualne

- Zapis HTML w bazie nie jest samodzielnym dowodem XSS. Element DOM dowodzi interpretacji; wykonanie handlera wymaga osobnego dowodu.
- Prywatny charakter notatek ogranicza model dostarczenia payloadu. Nie demonstrujemy odczytu cudzych notatek ani przejęcia konta.
- Pokaz formularza porównuje warianty o różnej obsłudze formatów. Odrzucenie w AFTER nie izoluje Antiforgery.
- CORS nie blokuje każdej nawigacji formularza. Blokada preflight JSON nie dowodzi braku wszelkiego CSRF.
- Nie badamy wszystkich wejść, przeglądarek, wdrożenia Production, aktywnego skanowania ani aktualności zależności.
- JWT może pozostawać ważny po usunięciu cookie; mechanizm unieważniania nie jest celem zmian.
- Konfiguracja bazy i cookies między portami może zaburzyć porównanie. Wymagane są izolacja danych i osobne profile przeglądarki.

Ryzyko po testach należy zaktualizować na podstawie [wyników](results.md), zachowując oddzielenie potencjalnego wpływu od rzeczywiście wykazanego skutku.
