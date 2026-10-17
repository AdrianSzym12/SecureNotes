# SecureNotes — scenariusze testów bezpieczeñstwa

## 1. Cel testów

Celem audytu jest sprawdzenie skutecznoœci mechanizmów bezpieczeñstwa aplikacji SecureNotes, ze szczególnym uwzglêdnieniem uwierzytelniania, autoryzacji, ochrony przed IDOR, CSRF i XSS oraz konfiguracji nag³ówków HTTP.

Testy s¹ wykonywane wy³¹cznie na lokalnej instancji aplikacji i przy u¿yciu przygotowanych kont testowych.

## 2. Œrodowisko

|	Element		|				Konfiguracja						|
|---------------|---------------------------------------------------|
| Aplikacja		| SecureNotes										|
| Backend		| ASP.NET Core, .NET 8								|
| Baza danych	| MariaDB											|
| Adres testowy | `https://localhost:7028/`							|
| Branch audytu | `test/security-audit`								|
| Narzêdzia		| Opera GX DevTools, OWASP ZAP, Visual Studio 2022	|
| Konta testowe | U¿ytkownik A i u¿ytkownik B						|

Nie zapisujemy w dokumentacji hase³, wartoœci tokenów JWT, tokenów CSRF ani zawartoœci ciasteczek sesyjnych.

## 3. Scenariusze testowe

|	ID		|		Obszar		|						Scenariusz											|				 Oczekiwany wynik								|
|-----------|-------------------|---------------------------------------------------------------------------|---------------------------------------------------------------|
| AUTH-01	| Uwierzytelnianie	| GET `/api/notes` bez aktywnej sesji										| 401 Unauthorized												|
| AUTH-02	| Uwierzytelnianie	| GET `/api/auth/me` bez aktywnej sesji										| 401 Unauthorized												|
| AUTH-03	| Uwierzytelnianie	| GET `/api/notes` po poprawnym logowaniu									| 200 OK														|
| AUTH-04	| Sesja				| Próba dostêpu do notatek po wylogowaniu									| 401 Unauthorized												|
| IDOR-01	| Autoryzacja		| U¿ytkownik B odczytuje notatkê u¿ytkownika A								| 404 Not Found													|
| IDOR-02	| Autoryzacja		| U¿ytkownik B edytuje notatkê u¿ytkownika A								| 404 Not Found; brak zmiany notatki							|
| IDOR-03	| Autoryzacja		| U¿ytkownik B usuwa notatkê u¿ytkownika A									| 404 Not Found; notatka nadal istnieje							|
| CSRF-01	| CSRF				| POST `/api/notes` bez tokenu CSRF											| 400 Bad Request												|
| CSRF-02	| CSRF				| POST `/api/notes` z niepoprawnym tokenem									| 400 Bad Request												|
| CSRF-03	| CSRF				| POST `/api/notes` z prawid³ow¹ sesj¹ i tokenem							| 201 Created													|
| XSS-01	| XSS				| Zapisanie i wyœwietlenie testowego ci¹gu przypominaj¹cego HTML/JavaScript | Ci¹g jest widoczny jako zwyk³y tekst; skrypt siê nie wykonuje |
| HEAD-01	| Nag³ówki			| Kontrola nag³ówka Content-Security-Policy | Restrykcyjna CSP jest obecna	|																|
| HEAD-02	| Nag³ówki			| Kontrola X-Content-Type-Options											| `nosniff`														|
| HEAD-03	| Nag³ówki			| Kontrola X-Frame-Options													| `DENY`														|
| COOKIE-01 | Sesja				| Kontrola ciasteczka `SecureNotes.Auth`									| Atrybuty HttpOnly, Secure i SameSite=Strict					|
| ZAP-01	| OWASP ZAP			| Analiza lokalnej aplikacji												| Wyniki zapisane i przeanalizowane								|

## 4. Dokumentowanie wyników

Dla ka¿dego wykonanego scenariusza zapisujemy:

- identyfikator testu;
- datê wykonania;
- wykorzystane narzêdzie;
- otrzymany kod HTTP lub zaobserwowane zachowanie;
- ocenê PASS / FAIL;
- nazwê pliku z dowodem, je¿eli wykonano zrzut ekranu;
- ewentualne uwagi.

Wyniki zapisujemy w `results.md`.

## 5. Znane ograniczenia

Interfejs Swagger UI obecnie wyœwietla pust¹ stronê przy aktywnej restrykcyjnej polityce CSP. Nie wy³¹czamy globalnie `SecurityHeadersMiddleware` na potrzeby audytu.

To ograniczenie zosta³o opisane w g³ównym pliku README.md.

## 6. Status

Dokument stanowi plan koñcowego audytu. Umieszczenie scenariusza w tabeli nie oznacza jeszcze jego zaliczenia. Wyniki bêd¹ zapisywane po rzeczywistym wykonaniu testów.
