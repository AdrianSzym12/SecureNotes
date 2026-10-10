
using Microsoft.AspNetCore.Mvc;
using SecureNotes.Application.DTOs.Auth;
using SecureNotes.Application.Exceptions;
using SecureNotes.Application.Services;

namespace SecureNotes.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly RegisterService _registerService;

        public AuthController(RegisterService registerService)
        {
            _registerService = registerService;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = await _registerService.RegisterAsync(
                    request,
                    cancellationToken);

                return StatusCode(
                    StatusCodes.Status201Created,
                    new
                    {
                        Id = userId,
                        Message = "User registered successfully."
                    });
            }
            catch (DuplicateEmailException)
            {
                return Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Email already registered",
                    detail: "An account with this email already exists.");
            }
            catch (ArgumentException)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid registration data",
                    detail: "Check the provided email and password.");
            }
        }
    }
}
