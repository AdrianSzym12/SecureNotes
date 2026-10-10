
import { apiRequest } from './api.js';

const notesList = document.getElementById('notes-list');
const notesEmpty = document.getElementById('notes-empty');
const notesMessage = document.getElementById('notes-message');

const createNoteForm = document.getElementById('create-note-form');
const noteTitle = document.getElementById('note-title');
const noteContent = document.getElementById('note-content');

const createButton = createNoteForm.querySelector(
    'button[type="submit"]'
);

function showMessage(message) {
    notesMessage.textContent = message;
}

async function checkSession() {
    const response = await apiRequest('/api/auth/me');

    if (!response.ok) {
        window.location.replace('/login.html');
        return false;
    }

    return true;
}

function createEditForm(note, article) {
    const form = document.createElement('form');

    const titleLabel = document.createElement('label');
    titleLabel.textContent = 'Tytuł';

    const titleInput = document.createElement('input');
    titleInput.type = 'text';
    titleInput.value = note.title;
    titleInput.maxLength = 150;
    titleInput.required = true;

    const contentLabel = document.createElement('label');
    contentLabel.textContent = 'Treść';

    const contentInput = document.createElement('textarea');
    contentInput.value = note.content;
    contentInput.maxLength = 10000;

    const saveButton = document.createElement('button');
    saveButton.type = 'submit';
    saveButton.textContent = 'Zapisz zmiany';

    const cancelButton = document.createElement('button');
    cancelButton.type = 'button';
    cancelButton.textContent = 'Anuluj';

    cancelButton.addEventListener('click', () => {
        form.replaceWith(createNoteElement(note));
        showMessage('');
    });

    form.appendChild(titleLabel);
    form.appendChild(titleInput);
    form.appendChild(contentLabel);
    form.appendChild(contentInput);
    form.appendChild(saveButton);
    form.appendChild(cancelButton);

    form.addEventListener('submit', async (event) => {
        event.preventDefault();

        const title = titleInput.value.trim();
        const content = contentInput.value;

        if (!title) {
            showMessage('Tytuł notatki nie może być pusty.');
            titleInput.focus();
            return;
        }

        saveButton.disabled = true;
        cancelButton.disabled = true;

        showMessage('Zapisywanie zmian...');

        try {
            const response = await apiRequest(
                `/api/notes/${note.id}`,
                'PUT',
                {
                    title: title,
                    content: content
                }
            );

            if (response.status === 401) {
                window.location.replace('/login.html');
                return;
            }

            if (response.status === 404) {
                await loadNotes();
                showMessage('Nie znaleziono notatki.');
                return;
            }

            if (response.status === 400) {
                showMessage(
                    'Nieprawidłowe dane notatki lub token CSRF.'
                );
                return;
            }

            if (!response.ok) {
                showMessage('Nie udało się zaktualizować notatki.');
                return;
            }

            await loadNotes();
            showMessage('Notatka została zaktualizowana.');
        }
        catch (error) {
            console.error('Update note error:', error);

            showMessage(
                'Wystąpił problem z połączeniem z serwerem.'
            );
        }
        finally {
            saveButton.disabled = false;
            cancelButton.disabled = false;
        }
    });

    return form;
}


async function deleteNote(note, deleteButton) {
    const confirmed = window.confirm(
        `Czy na pewno chcesz usunąć notatkę "${note.title}"?`
    );

    if (!confirmed) {
        return;
    }

    deleteButton.disabled = true;

    showMessage('Usuwanie notatki...');

    try {
        const response = await apiRequest(
            `/api/notes/${note.id}`,
            'DELETE'
        );

        if (response.status === 401) {
            window.location.replace('/login.html');
            return;
        }

        if (response.status === 404) {
            await loadNotes();
            showMessage('Nie znaleziono notatki.');
            return;
        }

        if (response.status === 400) {
            showMessage(
                'Nie udało się zweryfikować żądania lub tokenu CSRF.'
            );
            return;
        }

        if (response.status !== 204) {
            showMessage('Nie udało się usunąć notatki.');
            return;
        }

        await loadNotes();

        showMessage('Notatka została usunięta.');
    }
    catch (error) {
        console.error('Delete note error:', error);

        showMessage(
            'Wystąpił problem z połączeniem z serwerem.'
        );
    }
    finally {
        deleteButton.disabled = false;
    }
}

function createNoteElement(note) {
    const article = document.createElement('article');

    const title = document.createElement('h3');
    title.textContent = note.title;

    const content = document.createElement('p');
    content.textContent = note.content;
    content.classList.add('note-content');

    const createdAt = document.createElement('small');

    createdAt.textContent =
        `Utworzono: ${new Date(note.createdAtUtc).toLocaleString('pl-PL')}`;

    const editButton = document.createElement('button');
    editButton.type = 'button';
    editButton.textContent = 'Edytuj';

    editButton.addEventListener('click', () => {
        const editForm = createEditForm(note, article);
        article.replaceWith(editForm);
    });

    const deleteButton = document.createElement('button');
    deleteButton.type = 'button';
    deleteButton.textContent = 'Usuń';

    deleteButton.addEventListener('click', () => {
        deleteNote(note, deleteButton);
    });

    article.appendChild(title);
    article.appendChild(content);
    article.appendChild(createdAt);
    article.appendChild(editButton);
    article.appendChild(deleteButton);

    return article;
}

async function loadNotes() {
    showMessage('Ładowanie notatek...');

    const response = await apiRequest('/api/notes');

    if (response.status === 401) {
        window.location.replace('/login.html');
        return;
    }

    if (!response.ok) {
        showMessage('Nie udało się pobrać notatek.');
        return;
    }

    const notes = await response.json();

    notesList.replaceChildren();

    notesEmpty.hidden = notes.length !== 0;

    for (const note of notes) {
        const element = createNoteElement(note);
        notesList.appendChild(element);
    }

    showMessage('');
}

async function createNote(event) {
    event.preventDefault();

    const title = noteTitle.value.trim();
    const content = noteContent.value;

    if (!title) {
        showMessage('Tytuł notatki nie może być pusty.');
        noteTitle.focus();
        return;
    }

    createButton.disabled = true;

    showMessage('Zapisywanie notatki...');

    try {
        const response = await apiRequest(
            '/api/notes',
            'POST',
            {
                title: title,
                content: content
            }
        );

        if (response.status === 401) {
            window.location.replace('/login.html');
            return;
        }

        if (response.status === 400) {
            showMessage(
                'Nieprawidłowe dane notatki lub token CSRF.'
            );
            return;
        }

        if (response.status !== 201) {
            showMessage('Nie udało się utworzyć notatki.');
            return;
        }

        createNoteForm.reset();

        await loadNotes();

        showMessage('Notatka została dodana.');
    }
    catch (error) {
        console.error('Create note error:', error);

        showMessage(
            'Wystąpił problem z połączeniem z serwerem.'
        );
    }
    finally {
        createButton.disabled = false;
    }
}

async function initializeNotesPage() {
    try {
        const isLoggedIn = await checkSession();

        if (!isLoggedIn) {
            return;
        }

        await loadNotes();
    }
    catch (error) {
        console.error('Notes page error:', error);

        showMessage(
            'Wystąpił problem z połączeniem z serwerem.'
        );
    }
}

createNoteForm.addEventListener('submit', createNote);

initializeNotesPage();
