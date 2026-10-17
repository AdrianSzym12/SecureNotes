var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
{
    Args = Array.Empty<string>(),
    ContentRootPath = AppContext.BaseDirectory
});

// Ignore inherited endpoint/certificate configuration; use only the local dev certificate.
builder.Configuration.Sources.Clear();
builder.Logging.ClearProviders();
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    options.ListenLocalhost(7443, endpoint => endpoint.UseHttps());
});

var app = builder.Build();
var pagePath = Path.Combine(AppContext.BaseDirectory, "demo.html");

app.Use(async (context, next) =>
{
    if (!context.Request.IsHttps ||
        !string.Equals(context.Request.Host.Host, "localhost", StringComparison.OrdinalIgnoreCase) ||
        context.Request.Host.Port != 7443)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    context.Response.Headers["Cache-Control"] = "no-store";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'none'; form-action https://localhost:7028; " +
        "frame-src https://localhost:7028; " +
        "base-uri 'none'; frame-ancestors 'none'";
    await next(context);
});

// Only one page; no static-file directory, auth middleware or request logging.
app.MapGet("/", () => Results.File(pagePath, "text/html; charset=utf-8"));

app.Run();
