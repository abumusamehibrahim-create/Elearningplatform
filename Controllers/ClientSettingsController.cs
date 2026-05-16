using ELearningPlatform.Data;
using ELearningPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;

[Authorize(Roles = "SuperAdmin")]
public class ClientSettingsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public ClientSettingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) :base(context)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            // Get logged-in username
            var user = await _userManager.GetUserAsync(User);
            var username = user?.UserName ?? "Client";

            var settings = _context.ClientSettings.FirstOrDefault();

            if (settings == null)
            {
                settings = new ClientSetting
                {
                    ClientName = "",
                    ThemeColor = "#007bff",
                    LogoPath = "/uploads/logos/default.png",
                            ThemeName = "theme1"


                };

                _context.ClientSettings.Add(settings);
                _context.SaveChanges();
            }
            // If settings exist but ClientName is empty → set username
            if (string.IsNullOrWhiteSpace(settings.ClientName))
            {
                settings.ClientName = username;
                _context.SaveChanges();
            }

            var logos = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/logos"))
                                 .Select(x => "/uploads/logos/" + Path.GetFileName(x))
                                 .ToList();

            var vm = new ClientSettingsDashboardViewModel
            {
                Settings = settings,
                Logos = logos,
                Licenses = new List<License>() // always safe
            };

            return View(vm);
        }
        catch (Exception ex) 
        {


            throw new Exception(ex.Message,ex);
        }
        
         
    }

    [HttpPost]
    public IActionResult Save(ClientSettingsDashboardViewModel model)
    {
       try
        {
            var setting = _context.ClientSettings.FirstOrDefault();

            if (setting == null)
            {
                setting = new ClientSetting();
                _context.ClientSettings.Add(setting);
            }

            setting.ClientName = model.Settings.ClientName;
            setting.ThemeColor = model.Settings.ThemeColor;
            setting.LogoPath = model.Settings.LogoPath;
                // ⭐ أهم شيء
            setting.ThemeName = model.Settings.ThemeName;

            _context.SaveChanges();

            TempData["Success"] = "تم حفظ الإعدادات بنجاح";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {


            throw new Exception(ex.Message, ex);
        }
    }
    [HttpPost]
    public IActionResult Update(ClientSetting model)
    {
        try{var setting = _context.ClientSettings.FirstOrDefault();

        if (setting == null)
            return NotFound();

        setting.ClientName = model.ClientName;
        setting.ThemeColor = model.ThemeColor;
        setting.LogoPath = model.LogoPath;

        // ⭐ إضافة الثيم
        setting.ThemeName = model.ThemeName;

        _context.SaveChanges();

        TempData["Success"] = "تم تحديث الإعدادات";
        return RedirectToAction("Index");
        }
        catch (Exception ex)
        {


            throw new Exception(ex.Message, ex);
        }
    }


    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete()
    {
       try{ var settings = _context.ClientSettings.FirstOrDefault();

        if (settings != null)
        {
            _context.ClientSettings.Remove(settings);
            _context.SaveChanges();
        }

        // إعادة إنشاء صف جديد فارغ
        var newSetting = new ClientSetting
        {
            ClientName = "",
            ThemeColor = "#007bff",
            LogoPath = ""
        };

        _context.ClientSettings.Add(newSetting);
        _context.SaveChanges();

        TempData["Success"] = "Client settings reset.";

        return RedirectToAction("Index");
        }
        catch (Exception ex)
        {


            throw new Exception(ex.Message, ex);
        }
    }
    [HttpPost]
    public IActionResult ResetSettings()
    {
       try{ var setting = _context.ClientSettings.FirstOrDefault();
        if (setting != null)
        {
            setting.ClientName = "";
            setting.ThemeColor = "#007bff";
            setting.LogoPath = "/uploads/logos/default.png";

            _context.SaveChanges();
        }

        TempData["Success"] = "تمت إعادة الإعدادات للوضع الطبيعي";
        return RedirectToAction("Index");
        }
        catch (Exception ex)
        {


            throw new Exception(ex.Message, ex);
        }
    }


}
