
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureNotes.Application.DTOs.Notes;
using SecureNotes.Application.Services;
using System.Security.Claims;

namespace SecureNotes.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/notes")]
    public class NotesController : ControllerBase
    {
        private readonly NoteService _noteService;

        public NotesController(NoteService noteService)
        {
            _noteService = noteService;
        }

        private Guid? GetCurrentUserId()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userId, out var parsedUserId) ||
                parsedUserId == Guid.Empty)
            {
                return null;
            }

            return parsedUserId;
        }

        [HttpGet]
        public async Task<ActionResult<List<NoteResponse>>> GetAll(
            CancellationToken cancellationToken)
        {
            Response.Headers.CacheControl = "no-store";

            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var notes = await _noteService.GetAllAsync(
                userId.Value,
                cancellationToken);

            return Ok(notes);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<NoteResponse>> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            Response.Headers.CacheControl = "no-store";

            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            var note = await _noteService.GetByIdAsync(
                id,
                userId.Value,
                cancellationToken);

            if (note == null)
            {
                return NotFound();
            }

            return Ok(note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<NoteResponse>> Create(
            [FromBody] CreateNoteRequest request,
            CancellationToken cancellationToken)
        {
            Response.Headers.CacheControl = "no-store";

            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            if (request == null)
            {
                return BadRequest();
            }

            try
            {
                var note = await _noteService.CreateAsync(
                    userId.Value,
                    request,
                    cancellationToken);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = note.Id },
                    note);
            }
            catch (ArgumentException)
            {
                return BadRequest(new
                {
                    Message = "Invalid note data."
                });
            }
        }

        [HttpPut("{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<NoteResponse>> Update(
            Guid id,
            [FromBody] UpdateNoteRequest request,
            CancellationToken cancellationToken)
        {
            Response.Headers.CacheControl = "no-store";

            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            if (id == Guid.Empty || request == null)
            {
                return BadRequest();
            }

            try
            {
                var note = await _noteService.UpdateAsync(
                    id,
                    userId.Value,
                    request,
                    cancellationToken);

                if (note == null)
                {
                    return NotFound();
                }

                return Ok(note);
            }
            catch (ArgumentException)
            {
                return BadRequest(new
                {
                    Message = "Invalid note data."
                });
            }
        }

        [HttpDelete("{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken)
        {
            Response.Headers.CacheControl = "no-store";

            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            var deleted = await _noteService.DeleteAsync(
                id,
                userId.Value,
                cancellationToken);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

    }
}
