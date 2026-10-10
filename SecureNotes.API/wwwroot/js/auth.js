
import { apiRequest } from './api.js';

const loginForm = document.getElementById('login-form');

if (loginForm) {
    loginForm.addEventListener('submit', async function (event) {
        event.preventDefault();

        const email = document.getElementById('email').value;
        const password = document.getElementById('password').value;
        const message = document.getElementById('form-message');
        const submitButton = loginForm.querySelector('button[type="submit"]');

        message.textContent = '';
        message.className = 'form-message';

        submitButton.disabled = true;
        submitButton.textContent = 'Logowanie...';

        try {
            const response = await apiRequest('/api/auth/login', 'POST', {
                email: email,
                password: password
            });

            if (response.ok) {
                // Hasło nie jest już potrzebne w formularzu.
                document.getElementById('password').value = '';

                // Po zalogowaniu przechodzimy do strony głównej.
                window.location.replace('/');
                return;
            }

            if (response.status === 401) {
                message.textContent = 'Nieprawidłowy adres e-mail lub hasło.';
            } else if (response.status === 400) {
                message.textContent = 'Nie udało się zweryfikować żądania. Spróbuj ponownie.';
            } else {
                message.textContent = 'Wystąpił błąd logowania. Spróbuj ponownie później.';
            }

            message.classList.add('error');
        } catch (error) {
            console.error('Login request failed:', error.message);

            message.textContent = 'Nie można połączyć się z serwerem.';
            message.classList.add('error');
        } finally {
            submitButton.disabled = false;
            submitButton.textContent = 'Zaloguj się';
        }
    });
}


const registerForm = document.getElementById('register-form');

if (registerForm) {
    registerForm.addEventListener('submit', async function (event) {
        event.preventDefault();

        const email = document.getElementById('email').value.trim();
        const passwordInput = document.getElementById('password');
        const confirmPasswordInput =
            document.getElementById('confirm-password');

        const password = passwordInput.value;
        const confirmPassword = confirmPasswordInput.value;

        const message = document.getElementById('form-message');
        const submitButton =
            registerForm.querySelector('button[type="submit"]');

        message.textContent = '';
        message.className = 'form-message';

        if (password !== confirmPassword) {
            message.textContent = 'Podane hasła nie są identyczne.';
            message.classList.add('error');
            return;
        }

        submitButton.disabled = true;
        submitButton.textContent = 'Rejestracja...';

        try {
            const response = await apiRequest(
                '/api/auth/register',
                'POST',
                {
                    email: email,
                    password: password
                }
            );

            if (response.status === 201) {
                // Usuwamy hasła z pól formularza.
                passwordInput.value = '';
                confirmPasswordInput.value = '';

                message.textContent =
                    'Konto zostało utworzone. Możesz się teraz zalogować.';

                message.classList.add('success');
                return;
            }

            if (response.status === 409) {
                message.textContent =
                    'Konto z podanym adresem e-mail już istnieje.';
            } else if (response.status === 400) {
                message.textContent =
                    'Nie udało się zarejestrować konta. Sprawdź dane i spróbuj ponownie.';
            } else {
                message.textContent =
                    'Wystąpił błąd rejestracji. Spróbuj ponownie później.';
            }

            message.classList.add('error');
        } catch (error) {
            console.error('Registration request failed:', error.message);

            message.textContent =
                'Nie można połączyć się z serwerem.';

            message.classList.add('error');
        } finally {
            submitButton.disabled = false;
            submitButton.textContent = 'Zarejestruj się';
        }
    });
}

