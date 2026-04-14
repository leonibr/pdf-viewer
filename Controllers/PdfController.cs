using Microsoft.AspNetCore.Mvc;
using PdfViewer.Services;

namespace PdfViewer.Controllers;

[ApiController]
[Route("pdf")]
public class PdfController : ControllerBase
{
    [HttpGet("stream")]
    public IActionResult Stream([FromServices] PdfGeneratorService svc)
    {
        var bytes = svc.Generate();

        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        return File(bytes, "application/pdf");
    }
}
