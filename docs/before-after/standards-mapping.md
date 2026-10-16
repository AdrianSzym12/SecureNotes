# Mapowanie standardów

Baza kodu: `21be7bb6aadd69c729250eb5d5248cabb114de04`. Identyfikatory i przypisanie tematyczne poniżej odnoszą się do oficjalnych, wersjonowanych źródeł wskazanych w przeglądzie 02A.1. Nie są wynikami oceny zgodności aplikacji. Zmiany: `PLANNED`; testy: `NOT RUN`.

Top 10 opisuje ryzyka, ASVS wymagania, a WSTG metodykę testowania. **Bezpośrednie** powiązanie oznacza, że zmiana i test dotyczą danej kontroli. **Warunkowe** wymaga dodatkowego założenia architektonicznego. **Pomocnicze** dostarcza kontekstu lub regresji i samo nie potwierdza całego wymagania. Żadne z tych oznaczeń nie zastępuje wyniku testu.

## Powiązanie z eksperymentem

| Standard i wydanie | Obszar | Identyfikator | Zmiany | Testy | Rodzaj i zakres powiązania |
|---|---|---|---|---|---|
| OWASP Top 10:2025 | Injection — XSS, CWE-79 | A05:2025 | CHG-XSS-01, CHG-XSS-02 | T-XSS-01–04 | Bezpośrednie ryzyko: niezaufany HTML i wykonanie; T-XSS-01 pomocniczo potwierdza zapis |
| OWASP Top 10:2025 | Broken Access Control — CSRF, CWE-352 | A01:2025 | CHG-CSRF-01, CHG-CSRF-02 | T-CSRF-A01–03, T-CSRF-B01–03 | Bezpośrednie ryzyko wymuszenia zapisu; B02/B03 pomocniczo badają granice; adapter formularza jest dodany laboratoryjnie |
| OWASP ASVS 5.0.0 | Bezpieczne renderowanie tekstu | V3.2.2 | CHG-XSS-01 | T-XSS-02, T-XSS-03; pomocniczo T-XSS-01 | Bezpośrednie: textContent zachowuje tekst, a planowane innerHTML go interpretuje; tylko treść notatki |
| OWASP ASVS 5.0.0 | Content Security Policy | V3.4.3 | CHG-XSS-02 | T-XSS-04, T-REG-04; pomocniczo T-XSS-02 | Bezpośrednie: pomiar blokady handlera i zakresu wyjątku; nie jest to pełna ocena wszystkich dyrektyw CSP |
| OWASP ASVS 5.0.0 | Kontrola pochodzenia żądań, w tym Antiforgery | V3.5.1 | CHG-CSRF-01, CHG-CSRF-02 | T-CSRF-A01–03, T-CSRF-B01; pomocniczo T-REG-02 | Bezpośrednie: A izoluje walidację tokenu; B bada żądanie innego originu, ale ma asymetrię formatu |
| OWASP ASVS 5.0.0 | Ochrona funkcjonalności polegającej na preflight przed żądaniami omijającymi preflight | V3.5.2 | CHG-CSRF-02; kontekst CHG-CSRF-01 | T-CORS-01, T-CSRF-B01 | Warunkowe: dotyczy polegania na preflight jako barierze; formularz dodaje prostą ścieżkę bez preflight. Nie twierdzimy, że AFTER polega wyłącznie na CORS |
| OWASP ASVS 5.0.0 | Właściwe ustawienie SameSite | V3.3.2 | Zachowanie cookies przy CHG-CSRF-01/02 | T-REG-04, T-CSRF-B03; pomocniczo T-CSRF-B01 | Bezpośrednia kontrola flag i cross-site; B01 wyjaśnia granicę same-site, nie obejście Strict |
| OWASP ASVS 5.0.0 | HttpOnly dla danych niedostępnych dla skryptów | V3.3.4 | Zachowanie cookies przy CHG-XSS-01/02 i CHG-CSRF-01/02 | T-REG-04; pomocniczo T-XSS-02, T-CSRF-B01 | Bezpośrednia kontrola flag Auth/Csrf; wykonanie JS lub zapis formularzem nie dowodzą odczytu cookie ani naruszenia HttpOnly |
| OWASP WSTG 4.2 | Stored XSS | WSTG-v42-INPV-02 | CHG-XSS-01, CHG-XSS-02 | T-XSS-01–04 | Bezpośrednia metodyka: zapis, odczyt, wykonanie i trwałość; etap CSP pomocniczo izoluje barierę |
| OWASP WSTG 4.2 | CSRF | WSTG-v42-SESS-05 | CHG-CSRF-01, CHG-CSRF-02 | T-CSRF-A01–03, T-CSRF-B01–03 | Bezpośrednia metodyka: kontrola tokenu i atak przeglądarkowy; B02/B03 to kontrole pomocnicze |
| OWASP WSTG 4.2 | Atrybuty cookies | WSTG-v42-SESS-02 | Kontrola zachowania cookies przy CHG-CSRF-01/02 | T-REG-04, T-CSRF-B03; pomocniczo T-CSRF-B01 | Bezpośrednia kontrola atrybutów; zachowanie same-site pomaga interpretować ich zakres |

Zakresy ID w tabeli oznaczają wszystkie kolejne testy z [planu](test-plan.md), nie identyfikatory OWASP.

## Oficjalne źródła wersjonowane

- [Top 10:2025 — A05 Injection, w tym CWE-79](https://top10.owasp.org/2025/A05_2025-Injection/).
- [Top 10:2025 — A01 Broken Access Control, w tym CWE-352](https://top10.owasp.org/2025/A01_2025-Broken_Access_Control/).
- [ASVS 5.0.0 — V3 Web Frontend Security, tag v5.0.0](https://github.com/OWASP/ASVS/blob/v5.0.0/5.0/en/0x12-V3-Web-Frontend-Security.md): V3.2.2, V3.4.3, V3.5.1, V3.5.2, V3.3.2, V3.3.4.
- [WSTG 4.2 — Testing for Stored Cross Site Scripting](https://wstg.owasp.org/v4.2/4-Web_Application_Security_Testing/07-Input_Validation_Testing/02-Testing_for_Stored_Cross_Site_Scripting/): WSTG-v42-INPV-02.
- [WSTG 4.2 — Testing for Cross Site Request Forgery](https://wstg.owasp.org/v4.2/4-Web_Application_Security_Testing/06-Session_Management_Testing/05-Testing_for_Cross_Site_Request_Forgery/): WSTG-v42-SESS-05.
- [WSTG 4.2 — Testing for Cookies Attributes](https://wstg.owasp.org/v4.2/4-Web_Application_Security_Testing/06-Session_Management_Testing/02-Testing_for_Cookies_Attributes/): WSTG-v42-SESS-02.

Identyfikatory podano na podstawie źródeł zebranych podczas przeglądu 02A.1; wznowienie prac nie wymagało ponownego dostępu do internetu. Opisy są omówieniem zakresu powiązania, nie cytatami pełnej treści wymagań. Nie deklarujemy pełnej zgodności SecureNotes z ASVS, żadnego poziomu ASVS ani kompletnego pokrycia Top 10 lub WSTG. Po testach należy dopisać rzeczywiste dowody i ograniczenia oceny; T-REG-01–03 pozostają regresją funkcjonalną i autoryzacyjną, nie pełną oceną powyższych standardów.
