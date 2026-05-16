using Microsoft.EntityFrameworkCore;
using ELearningPlatform.Data;
using ELearningPlatform.Models;

public class ClientSettingMiddleware
{
    private readonly RequestDelegate _next;

    public ClientSettingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        /* ========================= CLIENT SETTINGS ========================= */

        var settings = await db.ClientSettings
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (settings == null)
        {
            settings = new ClientSetting
            {
                ClientName = "Client",
                ThemeColor = "#007bff",
                LogoPath = "/uploads/logos/default.png",
                ThemeName = "theme1"
            };

            db.ClientSettings.Add(settings);
            await db.SaveChangesAsync();
        }

        context.Items["ClientSetting"] = settings;


        /* ========================= MENU ITEMS ========================= */

        var menu = await db.MenuItems
            .Where(m => m.IsVisible)
            .OrderBy(m => m.Order)
            .AsNoTracking()
            .ToListAsync();

        context.Items["Menu"] = menu;


        /* ========================= TICKER ========================= */

        var tickerRow = await db.PageContents
            .Where(p => p.PageName == "Ticker")
            .OrderBy(p => p.Id)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        context.Items["Ticker"] = tickerRow?.Content ?? "";


        /* ========================= CONTINUE PIPELINE ========================= */

        await _next(context);
    }
}
