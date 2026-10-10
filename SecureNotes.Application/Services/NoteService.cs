
using SecureNotes.Application.DTOs.Notes;
using SecureNotes.Application.Interfaces.Persistence;
using SecureNotes.Domain.Entities;
using SecureNotes.Domain.Repositories;

namespace SecureNotes.Application.Services
{
    public class NoteService
    {
        private readonly INoteRepository _noteRepository;
        private readonly IUnitOfWork _unitOfWork;

        public NoteService(
            INoteRepository noteRepository,
            IUnitOfWork unitOfWork)
        {
            _noteRepository = noteRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<NoteResponse> CreateAsync(
            Guid userId,
            CreateNoteRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var note = new Note(
                userId,
                request.Title,
                request.Content);

            await _noteRepository.AddAsync(
                note,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return new NoteResponse(
                note.Id,
                note.Title,
                note.Content,
                note.CreatedAtUtc,
                note.UpdatedAtUtc);
        }

        public async Task<List<NoteResponse>> GetAllAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException(
                    "User ID cannot be empty.",
                    nameof(userId));
            }

            var notes = await _noteRepository.GetByUserIdAsync(
                userId,
                cancellationToken);

            return notes.Select(note => new NoteResponse(
                note.Id,
                note.Title,
                note.Content,
                note.CreatedAtUtc,
                note.UpdatedAtUtc
            )).ToList();
        }

        public async Task<NoteResponse?> GetByIdAsync(
            Guid noteId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (noteId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Note ID cannot be empty.",
                    nameof(noteId));
            }

            if (userId == Guid.Empty)
            {
                throw new ArgumentException(
                    "User ID cannot be empty.",
                    nameof(userId));
            }

            var note = await _noteRepository.GetByIdAndUserIdAsync(
                noteId,
                userId,
                cancellationToken);

            if (note == null)
            {
                return null;
            }

            return new NoteResponse(
                note.Id,
                note.Title,
                note.Content,
                note.CreatedAtUtc,
                note.UpdatedAtUtc);
        }

        public async Task<NoteResponse?> UpdateAsync(
            Guid noteId,
            Guid userId,
            UpdateNoteRequest request,
            CancellationToken cancellationToken = default)
        {
            if (noteId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Note ID cannot be empty.",
                    nameof(noteId));
            }

            if (userId == Guid.Empty)
            {
                throw new ArgumentException(
                    "User ID cannot be empty.",
                    nameof(userId));
            }

            ArgumentNullException.ThrowIfNull(request);

            var note = await _noteRepository.GetByIdAndUserIdAsync(
                noteId,
                userId,
                cancellationToken);

            if (note == null)
            {
                return null;
            }

            note.Update(
                request.Title,
                request.Content);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new NoteResponse(
                note.Id,
                note.Title,
                note.Content,
                note.CreatedAtUtc,
                note.UpdatedAtUtc);
        }

        public async Task<bool> DeleteAsync(
            Guid noteId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (noteId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Note ID cannot be empty.",
                    nameof(noteId));
            }

            if (userId == Guid.Empty)
            {
                throw new ArgumentException(
                    "User ID cannot be empty.",
                    nameof(userId));
            }

            var note = await _noteRepository.GetByIdAndUserIdAsync(
                noteId,
                userId,
                cancellationToken);

            if (note == null)
            {
                return false;
            }

            _noteRepository.Remove(note);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

    }
}
