using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using PdfViewer.Services;

QuestPDF.Settings.License = LicenseType.Community;

// Register embedded JetBrains Mono fonts so QuestPDF can find them on any OS
var assembly = typeof(Program).Assembly;
foreach (var name in new[] { "JetBrainsMono-Regular.ttf", "JetBrainsMono-Bold.ttf" })
{
    var resourceName = $"PdfViewer.Resources.Fonts.{name}";
    using var stream = assembly.GetManifestResourceStream(resourceName)
        ?? throw new InvalidOperationException($"Embedded font not found: {resourceName}");
    FontManager.RegisterFont(stream);
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddSingleton<PdfGeneratorService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Security response headers
app.Use(async (ctx, next) =>
{
    var headers = ctx.Response.Headers;

    // Prevent this app from being embedded in a foreign iframe (clickjacking)
    headers["X-Frame-Options"] = "SAMEORIGIN";

    // Stop browsers from MIME-sniffing the response content-type
    headers["X-Content-Type-Options"] = "nosniff";

    // Only send the origin as the referrer when crossing origins
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    // Content Security Policy:
    //   - default: same-origin only
    //   - scripts: same-origin + Bootstrap CDN (inline needed for Razor @section Scripts)
    //   - styles:  same-origin + Bootstrap CDN (unsafe-inline needed by PDF.js viewer)
    //   - frames:  same-origin only (our viewer.html)
    //   - objects: blocked entirely (no Flash / plugin content)
    headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "img-src 'self' data: blob:; " +
        "font-src 'self' https://cdn.jsdelivr.net; " +
        "frame-src 'self'; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self';";

    await next();
});

app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapControllers();

app.Run();
