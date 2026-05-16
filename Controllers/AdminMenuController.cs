using ELearningPlatform.Data;
using ELearningPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

public class AdminMenuController :BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public AdminMenuController(ApplicationDbContext context, IWebHostEnvironment env) :base(context)
    {
        _context = context;
        _env = env;
    }


    // ============================================================
    // MENU SETTINGS (Add / Update / Delete / Order)
    // ============================================================
    private void LoadPageContents(string pageName)
    {
        try
        {

            if (pageName == "Ticker")
            {
                var ticker = _context.PageContents
                    .Where(p => p.PageName == "Ticker")
                    .OrderBy(p => p.Id)
                    .FirstOrDefault();

                ViewBag.Header = ticker?.Content ?? "لا يوجد نص للتيكر";
                ViewBag.Paragraph = ""; // التيكر لا يحتاج فقرة

                return; // مهم جداً
            }
            var header = _context.PageContents
                .FirstOrDefault(p => p.PageName == pageName && p.Section == "Header");

            var paragraph = _context.PageContents
                .FirstOrDefault(p => p.PageName == pageName && p.Section == "Paragraph");

            ViewBag.Header = header?.Content ?? $"{pageName} - لا يوجد عنوان";
            ViewBag.Paragraph = paragraph?.Content ?? "لا يوجد محتوى لهذه الصفحة حالياً.";

        }
        catch (Exception e)
        {
            //return Content("Error: " + e.Message);
        }
    }

    public IActionResult AdminMenuSetting()
    {
        try
        {
            var items = _context.MenuItems.OrderBy(m => m.Order).ToList();
            return View(items);
        }
        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }

    [HttpGet]
    public IActionResult Add()
    {
        try
        {
            ViewBag.MenuItemsString = string.Join("|",
                _context.MenuItems.OrderBy(m => m.Order)
                .Select(m => $"{m.Id}:{m.Name}")
            );

            return View(new MenuItem());
        }
        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }
   

    [HttpPost]
    public IActionResult Add(MenuItem item)
    {
        try
        {
           
             



            if (!ModelState.IsValid)
                return View(item);

            item.IsVisible = User.IsInRole("Admin");
            item.ParentId = null;

            _context.MenuItems.Add(item);
            _context.SaveChanges();

            return RedirectToAction("AdminMenuSetting");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "خطأ أثناء إضافة العنصر: " + ex.Message);
            return View(item);
        }
    }
    //=================================================
    public IActionResult Delete(int id)
    {
        try
        {
            var item = _context.MenuItems.Find(id);
        if (item == null) return NotFound();

        _context.MenuItems.Remove(item);
        _context.SaveChanges();

        return RedirectToAction("AdminMenuSetting");
        }
        catch (Exception ex)
        {
            return BadRequest("خطأ أثناء الحذف: " + ex.Message);
        }
    }
    //========================================================
    [HttpPost]
    public IActionResult SaveOrder(List<int> ids)
    {
        try
        {
            int order = 1;
            foreach (var id in ids)
            {
                var item = _context.MenuItems.Find(id);
                if (item != null)
                    item.Order = order++;
            }

            _context.SaveChanges();
            return Ok();
        }
        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }

    [HttpPost]
    public IActionResult Update(MenuItem item)
    {
        try
        {
            var dbItem = _context.MenuItems.Find(item.Id);
            if (dbItem == null) return NotFound();

            dbItem.Name = item.Name;
            dbItem.Url = item.Url;
            dbItem.Order = item.Order;
            dbItem.IsVisible = item.IsVisible;
            dbItem.Icon = item.Icon;
            dbItem.Roles = item.Roles;
            dbItem.ParentId = null;

            _context.SaveChanges();

            return RedirectToAction("AdminMenuSetting");
        }
        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }

    // ============================================================
    // STATIC PAGES (User View Only)
    // ============================================================
    
    public IActionResult Home() {

        LoadPageContents("Home");
        return View(); }
    public IActionResult Categories()
    {
        try
        {

            LoadPageContents("Categories");

            //ViewBag.Header = _context.PageContents.First(p => p.PageName == "About" && p.Section == "Header").Content;
            //  ViewBag.Paragraph = _context.PageContents.First(p => p.PageName == "About" && p.Section == "Paragraph").Content;

            return View();
        }
        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }
    public IActionResult About()
    {
        try
        {
            //ViewBag.Header = _context.PageContents.First(p => p.PageName == "About" && p.Section == "Header").Content;
            // ViewBag.Paragraph = _context.PageContents.First(p => p.PageName == "About" && p.Section == "Paragraph").Content;
            LoadPageContents("About");

            return View();
        }
        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }

    public IActionResult Blog()
    {
        LoadPageContents("Blog");

        return View();
    
    }
    public IActionResult Contact()
    {
        try
        {
           // ViewBag.Header = _context.PageContents.First(p => p.PageName == "Contact" && p.Section == "Header").Content;
           // ViewBag.Paragraph = _context.PageContents.First(p => p.PageName == "Contact" && p.Section == "Paragraph").Content;
            LoadPageContents("Contact");

            return View();
        }
        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }

    }

    

    public IActionResult Offers()
    {
        try
        {
           // ViewBag.Header = _context.PageContents.First(p => p.PageName == "Offers" && p.Section == "Header").Content;
          //  ViewBag.Paragraph = _context.PageContents.First(p => p.PageName == "Offers" && p.Section == "Paragraph").Content;
            LoadPageContents("Offers");

            return View();
        }
        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }
    public IActionResult Privacy()
    {
        try
        {
            // ViewBag.Header = _context.PageContents.First(p => p.PageName == "Privacy" && p.Section == "Header").Content;
            // ViewBag.Paragraph = _context.PageContents.First(p => p.PageName == "Privacy" && p.Section == "Paragraph").Content;
            LoadPageContents("Privacy");

            return View();
        }

        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }

    public IActionResult Terms()
    {
        try
        {
            //ViewBag.Header = _context.PageContents.First(p => p.PageName == "Terms" && p.Section == "Header").Content;
            // ViewBag.Paragraph = _context.PageContents.First(p => p.PageName == "Terms" && p.Section == "Paragraph").Content;
            LoadPageContents("Terms");

            return View();
        }
        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }

    public IActionResult RefundPolicy()
    {
        try
        {
            //ViewBag.Header = _context.PageContents.First(p => p.PageName == "RefundPolicy" && p.Section == "Header").Content;
           // ViewBag.Paragraph = _context.PageContents.First(p => p.PageName == "RefundPolicy" && p.Section == "Paragraph").Content;
            LoadPageContents("RefundPolicy");

            return View();
        }

        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }

    }

    // ============================================================
    // REVIEWS (CRUD)
    // ============================================================

    // SHOW REVIEWS PAGE
    public IActionResult Reviews(int page = 1, int ratingFilter = 0)
    {
        try { 
        int pageSize = 6;

        var query = _context.Reviews.AsQueryable();

        // FILTER BY RATING
        if (ratingFilter > 0)
            query = query.Where(r => r.Rating == ratingFilter);

        // PAGINATION
        var totalReviews = query.Count();
        var reviews = query
            .OrderByDescending(r => r.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // AVERAGE RATING
        double avgRating = _context.Reviews.Any()
            ? _context.Reviews.Average(r => r.Rating)
            : 0;

        ViewBag.AvgRating = avgRating;
        ViewBag.TotalPages = (int)Math.Ceiling(totalReviews / (double)pageSize);
        ViewBag.CurrentPage = page;
        ViewBag.RatingFilter = ratingFilter;

        return View(reviews);
    }
        catch(Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }

    [HttpGet]
    public IActionResult AddReview()
    {
        return View(new Review());
    }

    // ADD REVIEW
    [HttpPost]
    public IActionResult AddReview(string studentName, string comment, int rating)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(studentName))
                return Content("❌ يجب كتابة اسم الطالب");

            if (string.IsNullOrWhiteSpace(comment))
                return Content("❌ يجب كتابة التعليق");

            if (rating < 1 || rating > 5)
                return Content("❌ يجب اختيار تقييم من 1 إلى 5");
            var review = new Review
            {
                StudentName = studentName,
                Comment = comment,
                Rating = rating,

                ImageUrl ="/images/reviews/default-user.png"// student cannot upload image
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();

            return RedirectToAction("Reviews");
        }

        catch(Exception e)
        {
            return Content("Error: " + e.Message);
        }

    }
   

    public IActionResult EditReview(int id)
    {
        var review = _context.Reviews.Find(id);
        return View(review);
    }

    [HttpPost]
    public IActionResult EditReview(int id, string studentName, string comment, int rating, IFormFile image)
    {
        try
        {
            var review = _context.Reviews.Find(id);
            if (review == null)
                return Content("❌ التقييم غير موجود");

            if (string.IsNullOrWhiteSpace(studentName))
                return Content("❌ يجب كتابة اسم الطالب");

            if (string.IsNullOrWhiteSpace(comment))
                return Content("❌ يجب كتابة التعليق");

            if (rating < 1 || rating > 5)
                return Content("❌ يجب اختيار تقييم من 1 إلى 5");
            review.StudentName = studentName;
            review.Comment = comment;
            review.Rating = rating;
            
            // ADMIN ONLY IMAGE UPLOAD
            if (User.IsInRole("Admin") && image != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
               // string path = Path.Combine(_env.WebRootPath, "reviews", fileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/reviews", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    image.CopyTo(stream);
                }

                review.ImageUrl = fileName;
            }

            _context.SaveChanges();
            return RedirectToAction("Reviews");
        }
        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }

    // DELETE REVIEW
    public IActionResult DeleteReview(int id)
    {
        try
        {
            var review = _context.Reviews.Find(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                _context.SaveChanges();
            }

            return RedirectToAction("Reviews");
        }
        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }

    // TOGGLE VISIBILITY
    public IActionResult ToggleReview(int id)
    {
        try
        {
            var review = _context.Reviews.Find(id);
            if (review != null)
            {
                review.ImageUrl = review.ImageUrl; // keep image
                review.Comment = review.Comment;   // keep comment
            }

            return RedirectToAction("Reviews");
        }
        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }

    // ============================================================
    // GALLERY (CRUD)
    // ============================================================

    public IActionResult Gallery()
    {
        var images = _context.GalleryImages.ToList();
        return View(images);
    }

    [HttpGet]
    public IActionResult AddGallery()
    {
        return View(new GalleryImage());
    }

    [HttpPost]
    public IActionResult AddGallery(GalleryImage model, IFormFile file)
    {
        try
        {
            if (file != null && file.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/gallery", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                model.FileName = fileName;
            }

            _context.GalleryImages.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Gallery");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    public IActionResult DeleteGallery(int id)
    {
        try
        {
            var img = _context.GalleryImages.Find(id);
            if (img == null) return NotFound();

            if (!string.IsNullOrEmpty(img.FileName))
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/gallery", img.FileName);
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

            _context.GalleryImages.Remove(img);
            _context.SaveChanges();

            return RedirectToAction("Gallery");
        }
        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }

    [HttpGet]
    public IActionResult EditGallery(int id)
    {
        var img = _context.GalleryImages.Find(id);
        if (img == null)
            return Content("Image not found");

        return View(img);
    }

    [HttpPost]
    public IActionResult EditGallery(GalleryImage model, IFormFile file)
    {
        try
        {
            var db = _context.GalleryImages.Find(model.Id);
            if (db == null)
                return NotFound();

            if (file != null && file.Length > 0)
            {
                string folder = Path.Combine(_env.WebRootPath, "images/gallery");
                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                db.FileName = fileName;
            }

            db.ImageUrl = model.ImageUrl;

            _context.SaveChanges();

            return RedirectToAction("Gallery");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }





    // ============================================================
    // TEAM (CRUD)
    // ============================================================

    public IActionResult Team()
    {
        var team = _context.TeamMembers.ToList();
        return View(team);
    }

    [HttpGet]
    public IActionResult AddTeam()
    {
        return View(new TeamMember());
    }

    [HttpPost]
    public IActionResult AddTeam(TeamMember model)
    {
        try
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.TeamMembers.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Team");
        }
        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }

    [HttpGet]
    public IActionResult EditTeam(int id)
    {
        var member = _context.TeamMembers.Find(id);
        if (member == null) return NotFound();

        return View(member);
    }

    [HttpPost]
    public IActionResult EditTeam(TeamMember model)
    {
        try
        {
            var db = _context.TeamMembers.Find(model.Id);
            if (db == null) return NotFound();

            db.Name = model.Name;
            db.Role = model.Role;
            db.ImageUrl = model.ImageUrl;

            _context.SaveChanges();

            return RedirectToAction("Team");
        }
        catch(Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }

    public IActionResult DeleteTeam(int id)
    {

       try{ var member = _context.TeamMembers.Find(id);
        if (member == null) return NotFound();

        _context.TeamMembers.Remove(member);
        _context.SaveChanges();

        return RedirectToAction("Team");
        }
        catch (Exception e)
        {
            return Content("Error: " + e.Message);
        }
    }
    //==========================================
    
        [HttpGet]
        public IActionResult EditablePage(string page)
        {
            try
            {
                var contents = _context.PageContents
                    .Where(p => p.PageName == page)
                    .ToList();

                if (!contents.Any())
                {
                    ViewBag.Error = $"⚠ لا توجد بيانات لهذه الصفحة: {page}. يجب إضافة بيانات أولاً.";
                    return View(new List<PageContent>());
                }

                return View(contents);
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }



    [HttpGet]
    public IActionResult SendMessage()
    {
        return Content("✔ SendMessage GET is working (for testing only)");
    }
    [HttpPost]
    public IActionResult SavePage(string page, List<string> sections, List<string> contents)
    {
        try
        {
            // 1) التحقق من أن المستخدم لم يترك أي حقل فارغ
            for (int i = 0; i < contents.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(contents[i]))
                {
                    return Content($"⚠ لا يمكن حفظ الصفحة. القسم '{sections[i]}' فارغ ويجب تعبئته.");
                }
            }

            // 2) تحديث البيانات
            for (int i = 0; i < sections.Count; i++)
            {
                var item = _context.PageContents
                    .FirstOrDefault(p => p.PageName == page && p.Section == sections[i]);

                if (item == null)
                {
                    return Content($"⚠ القسم '{sections[i]}' غير موجود في قاعدة البيانات. يجب إضافة بيانات أولاً.");
                }

                item.Content = contents[i];
            }
          //  return Content("Received: " + string.Join(" | ", contents));

            _context.SaveChanges();
                if (page == "Ticker")
                    return RedirectToAction("Index", "Home"); // أو أي صفحة تعرض الـ Ticker

            return RedirectToAction(page);
        }
        catch (Exception ex)
        {
            return Content("Error: " + ex.Message);
        }
    }
    // send message from user to admin
    [HttpPost]

    public async Task<IActionResult> SendMessage(
string fullName,
string email,
string message,
string senderEmail,
string senderPassword,
string receiverEmail)
    {
        try
        {
            /*var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true
            };
            var smtp = new SmtpClient("localhost")
            {//fake sending method
                Port = 25,
                EnableSsl = false
            };
            
              var mail = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = "رسالة جديدة من صفحة التواصل",
                Body = $"الاسم: {fullName}\nالبريد: {email}\n\nالرسالة:\n{message}",
                IsBodyHtml = false
            };
             
              mail.To.Add(receiverEmail);

            await smtp.SendMailAsync(mail);

            TempData["Success"] = "✔ تم إرسال الرسالة بنجاح";
             
             */

            // اختبار فقط
            Console.WriteLine("FULL NAME: " + fullName);
            Console.WriteLine("EMAIL: " + email);
            Console.WriteLine("MESSAGE: " + message);
            Console.WriteLine("SENDER EMAIL: " + senderEmail);
            Console.WriteLine("PASSWORD: " + senderPassword);
            Console.WriteLine("RECEIVER EMAIL: " + receiverEmail);

            TempData["Success"] = "✔ الكود يعمل وتم استقبال البيانات بنجاح (بدون إرسال إيميل)";
            return RedirectToAction("Contact");

           

           
        }
        catch (Exception ex)
        {
            TempData["Error"] = "❌ فشل إرسال الرسالة: " + ex.Message;
        }

        return RedirectToAction("Contact");
    }
    //==========================================================================FAQ

    // =====================
    // FAQ ACTIONS (FINAL CLEAN VERSION)
    // =====================

    // عرض صفحة FAQ + الأسئلة
    public IActionResult FAQ()
    {
        try
        {
            var list = _context.FAQItems
                               .Where(x => x.IsVisible == true || User.IsInRole("Admin"))
                               .OrderByDescending(x => x.Id)
                               .ToList();

            ViewBag.FAQList = list;
            ViewBag.IsAdmin = User.IsInRole("Admin");


            return View();
        }
        catch (Exception ex)
        {
            TempData["Error"] = "❌ حدث خطأ أثناء تحميل الأسئلة.";
            Console.WriteLine("FAQ ERROR: " + ex.Message);
            return View();
        }
    }
    //================================================================

    // الطالب يرسل سؤال
    [HttpPost]
    public IActionResult AskQuestion(string studentName, string question)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(studentName))
            {
                TempData["Error"] = "❌ الرجاء كتابة اسمك.";
                return RedirectToAction("FAQ");
            }

            if (string.IsNullOrWhiteSpace(question))
            {
                TempData["Error"] = "❌ الرجاء كتابة السؤال.";
                return RedirectToAction("FAQ");
            }

            var item = new FAQItem
            {
                StudentName = studentName.Trim(),
                Question = question.Trim()
            };

            _context.FAQItems.Add(item);
            _context.SaveChanges();

            TempData["Success"] = "✔ تم إرسال سؤالك بنجاح.";
            return RedirectToAction("FAQ");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "❌ حدث خطأ أثناء إرسال السؤال.";
            Console.WriteLine("AskQuestion ERROR: " + ex.Message);
            return RedirectToAction("FAQ");
        }
    }


    // الأدمن يكتب الإجابة
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult AnswerQuestion(int id, string answer)
    {
        try
        {
            var item = _context.FAQItems.Find(id);

            if (item == null)
            {
                TempData["Error"] = "❌ السؤال غير موجود.";
                return RedirectToAction("FAQ");
            }

            if (string.IsNullOrWhiteSpace(answer))
            {
                TempData["Error"] = "❌ لا يمكن حفظ إجابة فارغة.";
                return RedirectToAction("FAQ");
            }

            item.Answer = answer.Trim();
            _context.SaveChanges();

            TempData["Success"] = "✔ تم حفظ الإجابة بنجاح.";
            return RedirectToAction("FAQ");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "❌ حدث خطأ أثناء حفظ الإجابة.";
            Console.WriteLine("AnswerQuestion ERROR: " + ex.Message);
            return RedirectToAction("FAQ");
        }
    }


    // الأدمن يخفي/يظهر السؤال
    [Authorize(Roles = "Admin")]
    public IActionResult ToggleQuestion(int id)
    {
        try
        {
            var item = _context.FAQItems.Find(id);

            if (item == null)
            {
                TempData["Error"] = "❌ السؤال غير موجود.";
                return RedirectToAction("FAQ");
            }

            item.IsVisible = !item.IsVisible;
            _context.SaveChanges();

            TempData["Success"] = item.IsVisible
                ? "✔ تم إظهار السؤال."
                : "✔ تم إخفاء السؤال.";

            return RedirectToAction("FAQ");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "❌ حدث خطأ أثناء تغيير حالة السؤال.";
            Console.WriteLine("ToggleQuestion ERROR: " + ex.Message);
            return RedirectToAction("FAQ");
        }
    }
    //=================================

    [Authorize(Roles = "Admin")]
    public IActionResult DeleteQuestion(int id)
    {
        try
        {
            var item = _context.FAQItems.Find(id);

            if (item == null)
            {
                TempData["Error"] = "❌ السؤال غير موجود.";
                return RedirectToAction("FAQ");
            }

            _context.FAQItems.Remove(item);
            _context.SaveChanges();

            TempData["Success"] = "✔ تم حذف السؤال بنجاح.";
            return RedirectToAction("FAQ");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "❌ حدث خطأ أثناء حذف السؤال.";
            Console.WriteLine("DeleteQuestion ERROR: " + ex.Message);
            return RedirectToAction("FAQ");
        }
    }
    //=================================================
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteAllQuestions()
    {
        try
        {
            var all = _context.FAQItems.ToList();

            if (all.Count == 0)
            {
                TempData["Error"] = "❌ لا توجد أسئلة لحذفها.";
                return RedirectToAction("FAQ");
            }

            _context.FAQItems.RemoveRange(all);
            _context.SaveChanges();

            TempData["Success"] = "✔ تم حذف جميع الأسئلة بنجاح.";
            return RedirectToAction("FAQ");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "❌ حدث خطأ أثناء حذف جميع الأسئلة.";
            Console.WriteLine("DeleteAllQuestions ERROR: " + ex.Message);
            return RedirectToAction("FAQ");
        }
    }


    //====================================



}

