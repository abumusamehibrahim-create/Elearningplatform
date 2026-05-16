using ELearningPlatform.Data;
using ELearningPlatform.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
public class WorksheetController : BaseController
{
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<ApplicationUser> _userManager;

    public WorksheetController(ApplicationDbContext context, UserManager<ApplicationUser> userManager):base(context)
    {
       // _context = context;
        _userManager = userManager;
    }
   // private readonly ApplicationDbContext _context;


    private byte[] AddWatermark(byte[] pdfBytes, string watermarkText)
    {
        try
        {
            using (var ms = new MemoryStream())
        using (var reader = new PdfReader(pdfBytes))
        using (var stamper = new PdfStamper(reader, ms))
        {
            int pageCount = reader.NumberOfPages;

            BaseFont font = BaseFont.CreateFont(BaseFont.HELVETICA,
                                                BaseFont.CP1252,
                                                BaseFont.NOT_EMBEDDED);

            string fullWatermark = watermarkText;

            for (int i = 1; i <= pageCount; i++)
            {
                PdfContentByte canvas = stamper.GetUnderContent(i);

                PdfGState gs = new PdfGState();
                gs.FillOpacity = 0.18f; // شفافية 18%
                canvas.SetGState(gs);

                canvas.BeginText();
                canvas.SetColorFill(BaseColor.GRAY);
                canvas.SetFontAndSize(font, 48);

                // تكرار الـ Watermark على كامل الصفحة
                for (int x = 80; x < 600; x += 180)
                {
                    for (int y = 80; y < 800; y += 180)
                    {
                        canvas.ShowTextAligned(Element.ALIGN_CENTER,
                                               fullWatermark,
                                               x, y, 45);
                    }
                }

                canvas.EndText();
            }

            stamper.Close();
            return ms.ToArray();
        }
        }
        catch
        {
            // If PDF is invalid → return original file without watermark
            return pdfBytes;
        }
    }


    //============================download Action========
    public IActionResult Download(int id)
    {
        try { 
        return SafeExecute<IActionResult>(() =>
        {
            var file = _context.WorksheetFiles.FirstOrDefault(f => f.Id == id);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "ProtectedWorksheetFile", file.FileName);

            if (file == null)
                return NotFound();

            if (!file.AllowDownload && !User.IsInRole("Admin"))
                return Unauthorized();

            var fileBytes = System.IO.File.ReadAllBytes(path);

            var user = _userManager.GetUserAsync(User).Result;
            var watermarked = AddWatermark(fileBytes, $"Student: {user.UserName}");

            return File(watermarked, "application/pdf", file.FileName);
        });
        }
        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }
    //=============================================================================
    public IActionResult ViewPdf(int id)
    {
        try { 
        return SafeExecute<IActionResult>(() =>
        {
            var file = _context.WorksheetFiles.FirstOrDefault(f => f.Id == id);
            if (file == null)
                return NotFound();

            var path = Path.Combine(Directory.GetCurrentDirectory(), "ProtectedWorksheetFile", file.FileName);

            if (!System.IO.File.Exists(path))
                return NotFound();

            var bytes = System.IO.File.ReadAllBytes(path);

            var user = _userManager.GetUserAsync(User).Result;

            var watermarked = AddWatermark(
                 bytes,
                 $"islam teacher  "
             );

            Response.Headers["Content-Disposition"] = "inline";
            return File(watermarked, "application/pdf");
        });

        }
        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }

    //================================================

    public IActionResult WorksheetView(int videoId)
    {
       try{
            return SafeExecute<IActionResult>(() =>
        {
            var video = _context.Videos
            .Include(v => v.WorksheetFiles)
            .Include(v => v.WorksheetItems)
            .FirstOrDefault(v => v.Id == videoId);

            if (video == null)
                return NotFound();

            var model = new WorksheetViewModel
            {
                VideoId = video.Id,
                Title = video.Title,
                WorksheetFiles = video.WorksheetFiles.ToList(),
                WorksheetItems = video.WorksheetItems.ToList()
            };

            return View(model);
        });
        }
        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }

    //==============================uploadWorksheet=============
    [HttpPost]
    public async Task<IActionResult> UploadWorksheet(int videoId, IFormFile file)
    {
        try{
            return await SafeExecuteAsync<IActionResult>(async () => {
        
            if (file == null)
            {
                TempData["Error"] = "يرجى اختيار ملف";
                return RedirectToAction("WorksheetDetails", new { videoId });
            }

            var allowed = new[]
            {
        ".pdf",
        ".jpg", ".jpeg", ".png",
        ".doc", ".docx",
        ".ppt", ".pptx",
        ".xls", ".xlsx"
    };

            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!allowed.Contains(ext))
            {
                TempData["Error"] = "نوع الملف غير مدعوم";
                return RedirectToAction("WorksheetDetails", new { videoId });
            }

            var folder = Path.Combine(_env.ContentRootPath, "ProtectedWorksheetFile");
            Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid().ToString("N") + ext;
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _context.WorksheetFiles.Add(new WorksheetFile
            {
                VideoId = videoId,
                FileName = fileName
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم رفع ورقة العمل بنجاح";
            return RedirectToAction("WorksheetDetails", new { videoId });
        });

        }
        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }
    private async Task<T> SafeExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            _context.ActivityLogs.Add(new ActivityLog
            {
                Action = "Worksheet Error",
                AdminName = User.Identity?.Name ?? "Unknown",
                Details = ex.Message,
                Date = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return default(T);
        }
    }

    private T SafeExecute<T>(Func<T> action)
    {
        try
        {
            return action();
        }
        catch (Exception ex)
        {
            _context.ActivityLogs.Add(new ActivityLog
            {
                Action = "Worksheet Error",
                AdminName = User.Identity?.Name ?? "Unknown",
                Details = ex.Message,
                Date = DateTime.UtcNow
            });

            _context.SaveChanges();
            return default(T);
        }
    }


}
