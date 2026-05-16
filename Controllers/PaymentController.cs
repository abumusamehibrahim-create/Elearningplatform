using ELearningPlatform.Data;          // للتعامل مع قاعدة البيانات
using ELearningPlatform.Models;        // للوصول للنماذج مثل Course و Payment و ApplicationUser
using ELearningPlatform.Services;      // لإرسال الإيميلات
using Microsoft.AspNetCore.Identity;   // لإدارة المستخدمين والأدوار
using Microsoft.AspNetCore.Mvc;        // لإنشاء Controllers وActions
using Microsoft.EntityFrameworkCore;
using Stripe;                           // للتعامل مع مدفوعات Stripe

namespace ELearningPlatform.Controllers
{
    public class PaymentController :BaseController
    {
        public string transferNumber;
        public string PlainPassword { get; set; }
      //  private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;
        private readonly UserRegistrationService _userRegistrationService;


        public PaymentController(ApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    EmailService emailService,
    IConfiguration config,
    UserRegistrationService userRegistrationService):base(db)  // ← أضف هذا
        {
           // _context = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _config = config;

            _userRegistrationService = userRegistrationService; // ← السطر الذي كان يعطي خطأ
        }

        [HttpGet]
        public IActionResult Checkout(int courseId)
        {
           try{ var course = _context.Courses.Find(courseId);
            if (course == null) return NotFound();
            ViewBag.Course = course;
            return View();
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int courseId, string email, string transferNumber,
            string fullName, string stripeToken)
        {
            
            try
            { var course = _context.Courses.Find(courseId);
            if (course == null) return NotFound();

            if (string.IsNullOrEmpty(transferNumber))
            {
                TempData["Error"] = "يرجى إدخال رقم التحويل";
                return RedirectToAction("Checkout", new { courseId });
            }

            // 🔥 UPDATED: Save transfer number as payment reference
            string stripePaymentId = "TRANSFER_" + transferNumber;

            // توليد اسم مستخدم وكلمة مرور عشوائية
            var username = "std_" + Guid.NewGuid().ToString("N")[..8];
            var password = GeneratePassword();

            var user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                FullName = fullName,
                IsPaid = true,
                PaymentDate = DateTime.UtcNow,
                EmailConfirmed = true,
                    PlainPassword = password   // ← أضف هذا
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
                return RedirectToAction("Checkout", new { courseId });
            }

            if (!await _roleManager.RoleExistsAsync("Student"))
                await _roleManager.CreateAsync(new IdentityRole("Student"));

            await _userManager.AddToRoleAsync(user, "Student");

            // حفظ تفاصيل الدفع في قاعدة البيانات
            _context.Payments.Add(new Payment
            {
                UserId = user.Id,
                CourseId = courseId,
                Amount = course.Price,

                // 🔥 UPDATED: Save transfer number inside StripePaymentId
                StripePaymentId = stripePaymentId,

                Status = "Pending"
    
            });

            await _context.SaveChangesAsync();

            await _emailService.SendCredentialsAsync(email, fullName, username, password, course.Title);
            TempData["Message"] = "تم إرسال طلبك، سيتم تفعيل الحساب بعد مراجعة التحويل";

            // 🔥 UPDATED: Send user details to Success page
            TempData["Username"] = username;
            TempData["Password"] = password;
            TempData["CourseName"] = course.Title;

            TempData["FullName"] = fullName;            // 🔥 ADDED
            TempData["Email"] = email;                  // 🔥 ADDED
            TempData["TransferNumber"] = transferNumber; // 🔥 ADDED

            return RedirectToAction("Success");
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }

        public IActionResult Success() => View();


        private string GeneratePassword()
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghjklmnpqrstuvwxyz";
            const string digits = "23456789";
            const string special = "@#$!";
            var rng = new Random();
            var parts = new[]
            {
                upper[rng.Next(upper.Length)].ToString(),
                upper[rng.Next(upper.Length)].ToString(),
                lower[rng.Next(lower.Length)].ToString(),
                lower[rng.Next(lower.Length)].ToString(),
                lower[rng.Next(lower.Length)].ToString(),
                digits[rng.Next(digits.Length)].ToString(),
                digits[rng.Next(digits.Length)].ToString(),
                digits[rng.Next(digits.Length)].ToString(),
                special[rng.Next(special.Length)].ToString(),
            };
            return string.Concat(parts.OrderBy(_ => rng.Next()));
        }
       

    }
}
