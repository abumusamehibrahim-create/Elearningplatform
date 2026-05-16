using ELearningPlatform.Data;
using ELearningPlatform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ELearningPlatform.Controllers
{
    public class ActivationController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ActivationController(ApplicationDbContext db)
        {
            _db = db;
        }

        //==============================⭐ 1) توليد Offline Token=========================================================================
        public static string GenerateOfflineToken(string licenseKey, string deviceId)
        {
            string raw = licenseKey + "|" + deviceId + "|" + DateTime.UtcNow;
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(raw));
        }
        //==============================⭐ 3) التحقق Offline عند تشغيل التطبيق===================================
        public static bool ValidateOfflineToken()  

        {
            if (!System.IO.File.Exists("license.token"))
                return false;

            string token = System.IO.File.ReadAllText("license.token");
            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));

            var parts = decoded.Split('|');
            string licenseKey = parts[0];
            string deviceId = parts[1];

            // Compare device ID
            if (deviceId != GetDeviceId())
                return false;

            return true;
        }



        //======================⭐ 4) كود التفعيل حسب عدد الأجهزة==============================================
        [HttpPost]
        public async Task<IActionResult> Verify(string key)
        {
            var license = await _db.Licenses.FirstOrDefaultAsync(x => x.LicenseKey == key);

            if (license == null)
                return Content("❌ Invalid Activation Key");

            if (license.IsActive)
                return Content("❌ This key is already used");

            if (license.ExpirationDate < DateTime.UtcNow)
                return Content("❌ License expired");

            // Activate it
            license.IsActive = true;
            await _db.SaveChangesAsync();

            // Save activation locally (cookie)
            Response.Cookies.Append("Activated", "true", new CookieOptions
            {
                Expires = DateTime.UtcNow.AddYears(5)
            });

            return RedirectToAction("Index", "Home");
        }
        //=====⭐ 3) كيف نولد Device ID؟===========================================
        public static string GetDeviceId()
        {
            string cpu = Environment.MachineName;
            string os = Environment.OSVersion.VersionString;
            string unique = cpu + "_" + os;

            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(unique));
        }

        public async Task<IActionResult> Activate(string key)
        {
            var license = await _db.Licenses.FirstOrDefaultAsync(x => x.LicenseKey == key);

            if (license == null)
                return Content("❌ Invalid key");

            if (license.ExpirationDate < DateTime.UtcNow)
                return Content("❌ License expired");

            string deviceId = GetDeviceId();

            // Check if device already activated
            bool exists = await _db.LicenseDevices
     .AnyAsync(x => x.LicenseId == license.Id && x.DeviceId == deviceId);


            if (exists)
                return Content("✔ Already activated on this device");

            // Check device limit
            if (license.UsedDevices >= license.MaxDevices)
                return Content("❌ Device limit reached");

            // Register device
            _db.LicenseDevices.Add(new LicenseDevice
            {
                LicenseId = license.Id,
                //LicenseKey = key,
                DeviceId = deviceId,
                DeviceName = Environment.MachineName,
                OSVersion = Environment.OSVersion.VersionString
            });

            license.UsedDevices++;
            await _db.SaveChangesAsync();

            // Save activation locally
            Response.Cookies.Append("Activated", "true", new CookieOptions
            {
                Expires = DateTime.UtcNow.AddYears(10)
            });

            return Content("✔ Activation successful");
        }





    }
}
