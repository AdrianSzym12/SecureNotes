# SecureNotes — zasady dokumentowania testów bezpieczeñstwa

## 1. Cel dokumentacji

Celem dokumentacji jest zachowanie informacji o przebiegu koñcowego audytu bezpieczeñstwa aplikacji SecureNotes.

Ka¿dy test powinien pozwalaæ na odtworzenie jego przebiegu oraz weryfikacjê otrzymanego wyniku.

## 2. Œrodowisko testowe

- Aplikacja: SecureNotes
- Backend: ASP.NET Core (.NET 8)
- Baza danych: MariaDB
- Adres lokalny: `https://localhost:7028/`
- Branch: `test/security-audit`
- Narzêdzia: Opera GX DevTools, Visual Studio 2022, OWASP ZAP
- Konta testowe: u¿ytkownik A i u¿ytkownik B

Testy wykonywane s¹ wy³¹cznie w lokalnym œrodowisku projektu.

## 3. Schemat dokumentacji pojedynczego testu

Dla ka¿dego testu zapisujemy:

1. **ID** — identyfikator, np. AUTH-01.
2. **Cel** — jakie zabezpieczenie jest weryfikowane.
3. **Warunki pocz¹tkowe** — np. brak aktywnej sesji.
4. **Przebieg** — wykonane czynnoœci krok po kroku.
5. **Oczekiwany wynik** — prawid³owe zachowanie aplikacji.
6. **Wynik rzeczywisty** — zaobserwowany kod HTTP lub zachowanie.
7. **Ocena** — PASS, FAIL albo BLOCKED.
8. **Dowód** — œcie¿ka do zrzutu ekranu lub innego materia³u.
9. **Uwagi** — dodatkowe informacje potrzebne do interpretacji wyniku.

### Znaczenie ocen

- PASS — wynik rzeczywisty jest zgodny z oczekiwanym.
- FAIL — wynik odbiega od oczekiwanego.
- BLOCKED — nie uda³o siê przeprowadziæ testu z powodu przeszkody technicznej.
- NOT RUN — test jeszcze nie zosta³ wykonany.

## 4. Zrzuty ekranu

Ka¿dy zrzut powinien przedstawiaæ element pozwalaj¹cy zweryfikowaæ wynik, np.:

- adres testowanego endpointu;
- metodê HTTP;
- status odpowiedzi w zak³adce Network;
- zachowanie interfejsu u¿ytkownika;
- komunikat zwi¹zany z badan¹ ochron¹;
- wynik analizy OWASP ZAP.

Nazwy plików zapisujemy ma³ymi literami wed³ug schematu:

`identyfikator-testu-krotki-opis.png`

Przyk³ady:

- `auth-01-unauthorized-notes.png`
- `auth-03-authorized-notes.png`
- `idor-01-foreign-note-404.png`
- `csrf-01-missing-token-400.png`
- `xss-01-literal-content.png`
- `headers-01-csp.png`

## 5. Ochrona danych w materia³ach dowodowych

Przed dodaniem zrzutu do repozytorium nale¿y sprawdziæ, czy nie zawiera:

- hase³;
- tokenów JWT;
- wartoœci tokenów CSRF;
- wartoœci ciasteczek uwierzytelniaj¹cych;
- rzeczywistych danych osobowych;
- kluczy konfiguracyjnych i sekretów.

Wartoœci te nale¿y zamazaæ w kopii przeznaczonej do dokumentacji.

Nie umieszczamy w publicznym repozytorium surowych plików zawieraj¹cych poufne nag³ówki HTTP ani pe³nych sesji z narzêdzi testowych.

## 6. Zasady wykonywania audytu

- Ka¿dy scenariusz wykonujemy na aktualnej lokalnej wersji aplikacji.
- Nie wy³¹czamy globalnie mechanizmów bezpieczeñstwa.
- U¿ywamy wy³¹cznie sztucznych kont i notatek testowych.
- Rzeczywiste wyniki zapisujemy bezpoœrednio po zakoñczeniu testu.
- Nie oznaczamy testu jako PASS przed jego wykonaniem.
- Je¿eli test wykryje problem, dokumentujemy go zamiast pomijaæ.
- Materia³ dowodowy umieszczamy w folderze odpowiadaj¹cym kategorii testu.

## 7. Wykorzystanie dokumentacji

Zapisane wyniki i zrzuty ekranu pos³u¿¹ do przygotowania koñcowego raportu projektu, obejmuj¹cego metodykê badañ, analizê zastosowanych zabezpieczeñ, wyniki testów oraz wnioski.