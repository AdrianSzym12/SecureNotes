
import { apiRequest } from './api.js';

const guestSection = document.getElementById('guest-section');
const userSection = document.getElementById('user-section');
const logoutButton = document.getElementById('logout-button');
const userMessage = document.getElementById('user-message');
const sessionMessage = document.getElementById('session-message');

async function checkSession() {
    try {
        const response = await apiRequest('/api/auth/me');

        if (response.ok) {
            guestSection.hidden = true;
            userSection.hidden = false;

            sessionMessage.textContent = '';
            userMessage.textContent = 'Jesteś zalogowany.';
            return;
        }

        guestSection.hidden = false;
        userSection.hidden = true;

        if (response.status === 401) {
            sessionMessage.textContent = '';
        } else {
            sessionMessage.textContent =
                'Nie udało się sprawdzić sesji.';
        }
    } catch (error) {
        console.error('Session check failed:', error.message);

        guestSection.hidden = false;
        userSection.hidden = true;

        sessionMessage.textContent =
            'Nie można połączyć się z serwerem.';
    }
}

logoutButton.addEventListener('click', async function () {
    logoutButton.disabled = true;
    logoutButton.textContent = 'Wylogowywanie...';

    userMessage.textContent = '';

    try {
        const response = await apiRequest(
            '/api/auth/logout',
            'POST'
        );

        if (response.ok) {
            window.location.replace('/');
            return;
        }

        userMessage.textContent =
            'Nie udało się wylogować. Spróbuj ponownie.';
    } catch (error) {
        console.error('Logout request failed:', error.message);

        userMessage.textContent =
            'Nie można połączyć się z serwerem.';
    } finally {
        logoutButton.disabled = false;
        logoutButton.textContent = 'Wyloguj się';
    }
});

checkSession();
