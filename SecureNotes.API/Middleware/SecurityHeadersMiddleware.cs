
namespace SecureNotes.API.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;

                headers["X-Content-Type-Options"] = "nosniff";

                headers["X-Frame-Options"] = "DENY";

                headers["Referrer-Policy"] = "no-referrer";

                headers["Permissions-Policy"] =
                    "camera=(), microphone=(), geolocation=()";

                headers["Content-Security-Policy"] =
                    "default-src 'none'; " +
                    "script-src 'self'; " +
                    "script-src-attr 'none'; " +
                    "style-src 'self'; " +
                    "style-src-attr 'none'; " +
                    "img-src 'self'; " +
                    "font-src 'self'; " +
                    "connect-src 'self'; " +
                    "form-action 'self'; " +
                    "base-uri 'none'; " +
                    "object-src 'none'; " +
                    "frame-ancestors 'none'";

                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
