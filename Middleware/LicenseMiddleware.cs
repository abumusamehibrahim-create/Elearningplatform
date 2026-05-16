using ELearningPlatform.Data;
using ELearningPlatform.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class LicenseMiddleware
{
    private readonly RequestDelegate _next;

    public LicenseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
      var domain = "localhost:7197"; context.Request.Host.Host.ToLower();

        var license = db.Licenses.FirstOrDefault(x => x.Domain.ToLower() == domain);

        if (license == null)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("License not found");
            return;
        }

        if (!license.IsActive)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("License disabled");
            return;
        }

        if (license.ExpirationDate < DateTime.UtcNow)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("License expired");
            return;
        }
        // 2) Check License Key
        if (string.IsNullOrWhiteSpace(license.LicenseKey))
        {
            await context.Response.WriteAsync("❌ License Key missing");
            return;
        }

        // 3) Validate License Key Format (optional)
        if (license.LicenseKey.Length < 10)
        {
            await context.Response.WriteAsync("❌ Invalid License Key format");
            return;
        }

        // 4) Check Expiration
        if (license.ExpirationDate < DateTime.UtcNow)
        {
            await context.Response.WriteAsync("❌ License expired");
            return;
        }
        // 5) Check Active Status
        if (!license.IsActive)
        {
            await context.Response.WriteAsync("❌ License is deactivated");
            return;
        }

        // Attach settings to the request

        await _next(context);
    }

    /*
    //⭐ رابعًا: تعديل الـ Middleware ليمنع الدخول قبل التفعيل
     public async Task InvokeAsync(HttpContext context)
{
    // Allow activation page
    if (context.Request.Path.StartsWithSegments("/Activation"))
    {
        await _next(context);
        return;
    }

    // Check if activated locally
    if (!context.Request.Cookies.ContainsKey("Activated"))
    {
        context.Response.Redirect("/Activation/Index");
        return;
    }

    // Continue with domain + license validation
    await _next(context);
}

     */




}
