using ELearningPlatform.Data;
using ELearningPlatform.Models;
using ELearningPlatform.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ELearningPlatform.Controllers
{
    [Authorize(Roles = "SuperAdmin")]//يعني لا أحد يدخل إلا السوبر أدمن فقط.
    public class SuperAdminUserController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;//هذان هما أهم خدمتين في ASP.NET Identity:
        private readonly RoleManager<IdentityRole> _roleManager;

        public SuperAdminUserController(UserManager<ApplicationUser> userManager,
                                        RoleManager<IdentityRole> roleManager,
                                        ApplicationDbContext context) : base(context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ================================
        // عرض جميع المستخدمين
        // ================================
        public IActionResult Index()
        {
            try
            {
                var users = _userManager.Users.Where(u => u.IsActive).ToList();

                //var users = _userManager.Users.ToList();
                return View(users);
            }
            catch (Exception ex)
            {
                TempData["Message"] = "حدث خطأ أثناء تحميل المستخدمين.";
                return View(new List<ApplicationUser>());
            }
        }

        // ================================
        // GET: إنشاء مستخدم جديد
        // ================================
        public IActionResult CreateUser()
        {
            try
            {
                var roles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View(new CreateUserViewModel { AvailableRoles = roles });
            }
            catch
            {
                TempData["Message"] = "حدث خطأ أثناء تحميل صفحة إنشاء المستخدم.";
                return RedirectToAction("Index");
            }
        }

        // ================================
        // POST: إنشاء مستخدم جديد
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);

                    model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                    return View(model);
                }

                if (!string.IsNullOrEmpty(model.SelectedRole))
                {
                    if (await _roleManager.RoleExistsAsync(model.SelectedRole))
                        await _userManager.AddToRoleAsync(user, model.SelectedRole);
                }

                TempData["Message"] = "✔ تم إنشاء المستخدم بنجاح";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"حدث خطأ: {ex.Message}");
                model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View(model);
            }
        }



        // ================================
        // GET: تعديل الأدوار
        // ================================
        public async Task<IActionResult> EditRoles(string id)
        {
            try
            {
                

                if (id == null)
                {
                    TempData["Message"] = "معرّف المستخدم غير صالح.";
                    return RedirectToAction("Index");
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Message"] = "المستخدم غير موجود.";
                    return RedirectToAction("Index");
                }
                if (!user.IsActive)
                {
                    TempData["Message"] = "لا يمكن تعديل مستخدم معطّل.";
                    return RedirectToAction("Index");
                }
                var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                var userRoles = await _userManager.GetRolesAsync(user);

                var model = new EditUserRolesViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Roles = allRoles.Select(r => new RoleSelection
                    {
                        RoleName = r,
                        Selected = userRoles.Contains(r)
                    }).ToList()
                };

                return View(model);
            }
            catch
            {
                TempData["Message"] = "حدث خطأ أثناء تحميل الأدوار.";
                return RedirectToAction("Index");
            }
        }

        // ================================
        // POST: حفظ الأدوار
        // ================================
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> EditRoles(EditUserRolesViewModel model)
        {
            try
            {
                // المستخدم الذي يتم تعديل أدواره
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    TempData["Message"] = "المستخدم غير موجود.";
                    return RedirectToAction("Index");
                }

                // المستخدم الحالي (الذي يقوم بالتعديل)
                var currentUser = await _userManager.GetUserAsync(User);

                // الأدوار المختارة من الفورم
                var selectedRoles = model.Roles
                    .Where(r => r.Selected)
                    .Select(r => r.RoleName)
                    .ToList();
                // ================================
                // 0) منع عدم اختيار أي دور
                // ================================
                if (selectedRoles.Count == 0)
                {
                    TempData["Message"] = "يجب اختيار دور واحد على الأقل.";
                    return RedirectToAction("EditRoles", new { id = model.UserId });
                }
                // الأدوار الحالية للمستخدم
                var currentRoles = await _userManager.GetRolesAsync(user);

                // ================================
                // 1) منع السوبر أدمن من إزالة دوره بنفسه
                // ================================
                if (currentUser.Id == user.Id && !selectedRoles.Contains("SuperAdmin"))
                {
                    TempData["Message"] = "لا يمكنك إزالة دور SuperAdmin من نفسك.";
                    return RedirectToAction("Index");
                }

                // ================================
                // 2) منع أي مستخدم غير سوبر أدمن من منح SuperAdmin
                // ================================
                if (!User.IsInRole("SuperAdmin") && selectedRoles.Contains("SuperAdmin"))
                {
                    TempData["Message"] = "لا يمكنك منح دور SuperAdmin.";
                    return RedirectToAction("Index");
                }
                // ================================
                // 3) منع تغيير آخر سوبر أدمن إلى Admin
                // ================================
                if (currentRoles.Contains("SuperAdmin") && !selectedRoles.Contains("SuperAdmin"))
                {
                    // كم عدد السوبر أدمن الموجودين في النظام؟
                    var allSuperAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");

                    if (allSuperAdmins.Count == 1)
                    {
                        TempData["Message"] = "لا يمكن إزالة دور SuperAdmin من هذا المستخدم لأنه آخر سوبر أدمن في النظام.";
                        return RedirectToAction("Index");
                    }
                }

                // ================================
                // 3) تحديث الأدوار
                // ================================
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRolesAsync(user, selectedRoles);

                TempData["Message"] = "تم تحديث الأدوار بنجاح";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Message"] = "حدث خطأ أثناء تحديث الأدوار.";
                return RedirectToAction("Index");
            }
        }




        // ================================
        // GET: Reset Password
        // ================================
        
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                TempData["Message"] = "❌ المستخدم غير موجود.";
                return RedirectToAction("Index");
            }

            var model = new ResetPasswordViewModel
            {
                UserId = user.Id,
                UserName = user.UserName
            };

            return View(model);
        }

        // ================================
        // POST: Reset Password
        // ================================
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var user = await _userManager.FindByIdAsync(model.UserId);

                if (user == null)
                {
                    TempData["Message"] = "❌ المستخدم غير موجود.";
                    return RedirectToAction("Index");
                }

                // منع SuperAdmin من إعادة تعيين كلمة المرور لنفسه
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser.Id == user.Id)
                {
                    TempData["Message"] = "⚠ لا يمكنك إعادة تعيين كلمة المرور لحسابك أنت.";
                    return RedirectToAction("Index");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);

                    return View(model);
                }
                // ⭐ تحديث كلمة المرور الظاهرة للطالب
                user.PlainPassword = model.NewPassword;
                await _userManager.UpdateAsync(user);
                TempData["Message"] = "✔ تم إعادة تعيين كلمة المرور بنجاح.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Message"] = "❌ خطأ: " + ex.Message;
                return RedirectToAction("Index");
            }
        }



        // ================================
        // GET: تغيير كلمة مرور السوبر أدمن
        // ================================
        public IActionResult ChangePassword()
        {
            return View();
        }

        // ================================
        // POST: تغيير كلمة مرور السوبر أدمن
        // ================================
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid) return View(model);

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Message"] = "المستخدم غير موجود.";
                    return RedirectToAction("Index");
                }

                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);

                    return View(model);
                }

                TempData["Message"] = "تم تغيير كلمة المرور بنجاح";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Message"] = "حدث خطأ أثناء تغيير كلمة المرور.";
                return RedirectToAction("Index");
            }
        }

        // ================================
        // حذف مستخدم
        // ================================
        

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Message"] = "❌ المستخدم غير موجود.";
                    return RedirectToAction("Index");
                }

                var currentUser = await _userManager.GetUserAsync(User);

                // 1) منع حذف نفسك
                if (currentUser.Id == user.Id)
                {
                    TempData["Message"] = "⚠ لا يمكنك حذف حسابك أنت.";
                    return RedirectToAction("Index");
                }

                // 2) جلب أدوار المستخدم
                var roles = await _userManager.GetRolesAsync(user);

                // 3) منع حذف آخر SuperAdmin
                if (roles.Contains("SuperAdmin"))
                {
                    var allSuperAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");

                    if (allSuperAdmins.Count == 1)
                    {
                        TempData["Message"] = "❌ لا يمكن حذف هذا المستخدم لأنه آخر SuperAdmin في النظام.";
                        return RedirectToAction("Index");
                    }
                }

                // 4) هل المستخدم مرتبط بأي دورة؟
                bool hasCourses = await _context.UserCourses
                    .AnyAsync(uc => uc.UserId == user.Id);

                // 5) هل لديه دفعات Approved؟
                bool hasApprovedPayments = await _context.Payments
                    .AnyAsync(p => p.UserId == user.Id && p.Status == "Approved");

                // 6) هل لديه دفعات Pending؟
                bool hasPendingPayments = await _context.Payments
                    .AnyAsync(p => p.UserId == user.Id && p.Status == "Pending");

                // ================================
                // 🔥 إذا كان المستخدم مرتبط بأي شيء → Soft Delete
                // ================================
                if (hasCourses || hasApprovedPayments || hasPendingPayments)
                {
                    user.IsActive = false;
                    await _userManager.UpdateAsync(user);

                    TempData["Message"] = "⚠ تم تعطيل المستخدم لأنه مرتبط ببيانات ولا يمكن حذفه نهائيًا.";
                    return RedirectToAction("Index");
                }

                // ================================
                // 🔥 إذا لم يكن مرتبط بأي شيء → Hard Delete
                // ================================
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    TempData["Message"] = "✔ تم حذف المستخدم نهائيًا لأنه غير مرتبط بأي دورة أو دفعة.";
                }
                else
                {
                    TempData["Message"] = "❌ حدث خطأ أثناء الحذف النهائي.";
                }

                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Message"] = "❌ حدث خطأ أثناء حذف المستخدم.";
                return RedirectToAction("Index");
            }
        }


    }
}
