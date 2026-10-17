
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
                    (string.Equals(context.Request.Path.Value, "/notes.html", StringComparison.Ordinal)
                        ? "script-src-attr 'unsafe-inline'; "
                        : "script-src-attr 'none'; ") +
                    "style-src 'self'; " +
                    "style-src-attr 'none'; " +
                    "img-src 'self'; " +
                    "font-src 'self'; " +
                    "connect-src 'self'; " +
                    "form-action 'self'; " +
                    "base-uri 'none'; " +
                    "object-src 'none'; " +
                    "frame-ancestors 'none'";

                
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    headers["Cache-Control"] = "no-store";
                }

                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
