
using Microsoft.AspNetCore.Mvc;
using SecureNotes.Application.DTOs.Auth;
using SecureNotes.Application.Exceptions;
using SecureNotes.Application.Interfaces.Security;
using SecureNotes.Application.Services;
using Microsoft.Extensions.Options;
using SecureNotes.Infrastructure.Configuration;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SecureNotes.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly RegisterService _registerService;
        private readonly LoginService _loginService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly JwtConfiguration _jwtConfiguration;
        private readonly IAntiforgery _antiforgery;

        public AuthController(
            RegisterService registerService,
            LoginService loginService,
            IJwtTokenService jwtTokenService,
            IOptions<JwtConfiguration> jwtOptions,
            IAntiforgery antiforgery)
        {
            _registerService = registerService;
            _loginService = loginService;
            _jwtTokenService = jwtTokenService;
            _jwtConfiguration = jwtOptions.Value;
            _antiforgery = antiforgery;
        }


        [HttpPost("register")]
        [ValidateAntiForgeryToken]
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

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken)
        {
            var userId = await _loginService.LoginAsync(
                request,
                cancellationToken);

            if (userId == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Invalid credentials",
                    detail: "Invalid email or password.");
            }

            var token = _jwtTokenService.GenerateToken(userId.Value);

            Response.Cookies.Append(
                "SecureNotes.Auth",
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Path = "/",
                    IsEssential = true,
                    MaxAge = TimeSpan.FromMinutes(
                        _jwtConfiguration.ExpirationMinutes)
                });

            return Ok(new
            {
                Message = "Login successful."
            });
        }

        [HttpPost("logout")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(
                "SecureNotes.Auth",
                new CookieOptions
                {
                    Secure = true,
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                    Path = "/"
                });

            Response.Cookies.Delete(
                "SecureNotes.Csrf",
                new CookieOptions
                {
                    Secure = true,
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                    Path = "/"
                });

            Response.Headers.CacheControl = "no-store";

            return Ok(new
            {
                Message = "Logout successful."
            });
        }

        [HttpGet("csrf-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetCsrfToken()
        {
            Response.Headers.CacheControl = "no-store";

            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

            return Ok(new
            {
                Token = tokens.RequestToken
            });
        }

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            Response.Headers.CacheControl = "no-store";

            return Ok(new
            {
                UserId = userId
            });
        }

    }
}
