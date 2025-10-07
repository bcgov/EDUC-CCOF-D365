using Microsoft.AspNetCore.Mvc;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace CCOF.Infrastructure.WebAPI.Controllers;
public class PdfMergeRequest
{
    public string File1Base64 { get; set; } = string.Empty;
    public string File2Base64 { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class PdfController : ControllerBase
{
    [HttpPost("Merge")]
    [Consumes("application/json")]
    public IActionResult MergePdfFilesJson([FromBody] PdfMergeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.File1Base64) || string.IsNullOrWhiteSpace(request.File2Base64))
            return BadRequest("Both Base64 PDF strings are required.");

        using var outputDocument = new PdfDocument();

        try
        {
            // Decode and merge first PDF
            var file1Bytes = Convert.FromBase64String(request.File1Base64);
            using var stream1 = new MemoryStream(file1Bytes);
            var inputDocument1 = PdfReader.Open(stream1, PdfDocumentOpenMode.Import);
            foreach (var page in inputDocument1.Pages)
                outputDocument.AddPage(page);

            // Decode and merge second PDF
            var file2Bytes = Convert.FromBase64String(request.File2Base64);
            using var stream2 = new MemoryStream(file2Bytes);
            var inputDocument2 = PdfReader.Open(stream2, PdfDocumentOpenMode.Import);
            foreach (var page in inputDocument2.Pages)
                outputDocument.AddPage(page);
        }
        catch (Exception ex)
        {
            return StatusCode(400, $"PDF merge failed: {ex}");
        }

        using var outputStream = new MemoryStream();
        outputDocument.Save(outputStream);
        var mergedBase64 = Convert.ToBase64String(outputStream.ToArray());

        return Ok(new
        {
            fileName = "merged.pdf",
            base64Content = mergedBase64
        });
    }
}