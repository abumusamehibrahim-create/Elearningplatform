using ELearningPlatform.Data;
using ELearningPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELearningPlatform.Controllers
{
    public class AccountController : BaseController
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        //public AccountController(UserManager<ApplicationUser> userManager,
          //  SignInManager<ApplicationUser> signInManager) :base(dbBase)

            public AccountController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager
    ) : base(context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Dashboard()
        {
            var users = _context.Users.ToList();
            return View(users);
        }


        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            try
            {
                // 1) اجلب المستخدم
               // var user = await _userManager.FindByNameAsync(username);
                var user = await _userManager.Users.FirstOrDefaultAsync(
                    u => u.UserName.ToLower() == username.ToLower());

                if (user == null)
                {
                    ViewBag.Error = "اسم المستخدم غير موجود";
                    return View();
                }

                // 2) تحقق من كلمة المرور
                var passwordValid = await _userManager.CheckPasswordAsync(user, password);

                if (!passwordValid)
                {
                    ViewBag.Error = "كلمة المرور غير صحيحة";
                    return View();
                }

                // 3) تحقق من أن الحساب نشط
                if (!user.IsActive)
                {
                    ViewBag.Error = "الحساب غير مفعل";
                    return View();
                }

                // 4) تسجيل الدخول
                await _signInManager.SignInAsync(user, true);

                // 5) إذا كان Admin → ادخله لوحة التحكم
               if(await _userManager.IsInRoleAsync(user, "SuperAdmin") ||
     await _userManager.IsInRoleAsync(user, "Admin"))
{
                    return RedirectToAction("Index", "Admin");
                }

                // 6) تحديث بيانات الجهاز
                // 6) تحديث بيانات الجهاز
                user.SessionToken = Guid.NewGuid().ToString();
                user.LastDeviceId = Request.Headers["User-Agent"].ToString();
                user.LastIP = HttpContext.Connection.RemoteIpAddress?.ToString();

                await _userManager.UpdateAsync(user);
                // 6.1) إضافة الكوكيز
Response.Cookies.Append("SessionToken", user.SessionToken, new CookieOptions
{
    HttpOnly = true,
    Secure = true,
    SameSite = SameSiteMode.Strict,
    Expires = DateTime.UtcNow.AddDays(7)
});
                // 7) تسجيل الدخول مرة ثانية بعد التحديث
                await _signInManager.SignInAsync(user, true);
                return RedirectToAction("Index", "Home");
                // 8) إعادة التوجيه
                //  return RedirectToLocal(returnUrl) ?? RedirectToAction("Index", "Home");

            }
            catch (Exception ex)
            {
                ViewBag.Error = "حدث خطأ أثناء تسجيل الدخول: " + ex.Message;
                return View();
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        private IActionResult? RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return null;
        }

        // FOR HASHPASSWORD CREATE  
        [AllowAnonymous]
        public async Task<IActionResult> FixSuperAdminPassword()
        {
            var user = await _userManager.FindByNameAsync("ABUHMAM84");
            if (user == null)
                return Content("SuperAdmin user not found");

            // Remove old password (if exists)
            try
            {
                await _userManager.RemovePasswordAsync(user);
            }
            catch { }

            // Add new password
            var result = await _userManager.AddPasswordAsync(user, "SuperAdmin123!");

            if (result.Succeeded)
                return Content("SuperAdmin password fixed successfully");

            return Content("Failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }


    }
}
