
async function getCsrfToken() {
    const response = await fetch('/api/auth/csrf-token', {
        method: 'GET',
        credentials: 'same-origin',
        cache: 'no-store'
    });

    if (!response.ok) {
        throw new Error('Nie udało się pobrać tokenu CSRF.');
    }

    const data = await response.json();

    if (!data.token) {
        throw new Error('Odpowiedź API nie zawiera tokenu CSRF.');
    }

    return data.token;
}

export async function apiRequest(url, method = 'GET', data = null) {
    const headers = {};

    const requestMethod = method.toUpperCase();

    if (data !== null) {
        headers['Content-Type'] = 'application/json';
    }

    // Żądania zmieniające stan wymagają tokenu CSRF.
    if (!['GET', 'HEAD', 'OPTIONS'].includes(requestMethod)) {
        const csrfToken = await getCsrfToken();

        headers['X-CSRF-TOKEN'] = csrfToken;
    }

    const response = await fetch(url, {
        method: requestMethod,
        credentials: 'same-origin',
        headers: headers,
        body: data !== null ? JSON.stringify(data) : undefined,
        cache: 'no-store'
    });

    return response;
}
