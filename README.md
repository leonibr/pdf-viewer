# PDF Viewer POC

A proof-of-concept for server-side PDF generation and in-browser viewing.

## What it does

Generates a random sales report PDF on demand and displays it in an embedded viewer. Each reload produces a new report with different KPIs and sales data.

## Stack

- **Backend:** ASP.NET Core 8.0 + Razor Pages
- **PDF generation:** [QuestPDF](https://www.questpdf.com/) (Community License)
- **Frontend:** PDF.js, Bootstrap 5, jQuery

## Running

```bash
dotnet run
```

Then open `https://localhost:<port>` and click **Reload PDF** to generate a new report.

## Project structure

```
Controllers/PdfController.cs       # GET /pdf/stream — returns generated PDF bytes
Services/PdfGeneratorService.cs    # Builds the report with QuestPDF
Pages/Index.cshtml                 # UI with PDF.js viewer
Program.cs                         # App startup, DI, security headers
```
