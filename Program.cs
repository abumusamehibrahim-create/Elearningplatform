using ELearningPlatform.Data;
using ELearningPlatform.Models;
using ELearningPlatform.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static System.Formats.Asn1.AsnWriter;

var builder = WebApplication.CreateBuilder(args);

// ===================== DATABASE =====================
/*builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.CommandTimeout(180) // Prevent timeout
    )
);
*/
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )
    )
);





// ===================== IDENTITY =====================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ===================== COOKIE =====================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

// ===================== SERVICES =====================
builder.Services.AddTransient<EmailService>();
builder.Services.AddScoped<VideoAccessService>();
builder.Services.AddScoped<UserRegistrationService>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Configuration.AddEnvironmentVariables();
builder.Services.Configure<ClientSetting>(
    builder.Configuration.GetSection("ClientSettings"));// client setting
builder.Services.AddScoped<FileCleanupService>();// rmove file and viedo not connected to database


var app = builder.Build();


// ===================== APPLY MIGRATIONS FIRST =====================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

// ===================== SEED MENU ITEMS =====================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // 1) حذف البيانات القديمة
    var oldMenu = context.MenuItems.ToList();
    if (oldMenu.Any())
    {
        context.MenuItems.RemoveRange(oldMenu);
        context.SaveChanges();
    }

   // if (!context.MenuItems.Any())
   // {
        context.MenuItems.AddRange(
           new MenuItem { Name = "التقييم", Url = "Reviews", Order = 1, IsVisible = true },
new MenuItem { Name = "من نحن", Url = "About", Order = 2, IsVisible = true },
new MenuItem { Name = "المدونة", Url = "Blog", Order = 3, IsVisible = true },
new MenuItem { Name = "التصنيفات", Url = "Categories", Order = 4, IsVisible = true },
new MenuItem { Name = "اتصل بنا", Url = "Contact", Order = 5, IsVisible = true },
new MenuItem { Name = "الأسئلة الشائعة", Url = "FAQ", Order = 6, IsVisible = true },
new MenuItem { Name = "العروض", Url = "Offers", Order = 7, IsVisible = true },
new MenuItem { Name = "سياسة الخصوصية", Url = "Privacy", Order = 8, IsVisible = true },
new MenuItem { Name = "الشروط والأحكام", Url = "Terms", Order = 9, IsVisible = true },
new MenuItem { Name = "صور الطلاب", Url = "Gallery", Order = 10, IsVisible = true }

        );

        context.SaveChanges();
   // }
}//===============================================================
// the text page

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    Console.WriteLine(context.Database.GetDbConnection().ConnectionString);

    if (!context.PageContents.Any())
    {

        context.PageContents.AddRange(

    // About
    new PageContent { PageName = "About", Section = "Header", Content = "من نحن" },
    new PageContent { PageName = "About", Section = "Paragraph", Content = "نحن منصة تعليمية تهدف إلى تقديم أفضل تجربة تعليمية للطلاب من مختلف الأعمار والمستويات." },

    // Blog
    new PageContent { PageName = "Blog", Section = "Header", Content = "المدونة" },
    new PageContent { PageName = "Blog", Section = "Paragraph", Content = "اقرأ أحدث المقالات التعليمية والنصائح المفيدة التي تساعدك على تطوير مهاراتك." },

    // Contact
    new PageContent { PageName = "Contact", Section = "Header", Content = "تواصل معنا" },
    new PageContent { PageName = "Contact", Section = "Paragraph", Content = "نحن هنا لخدمتك والإجابة على جميع استفساراتك في أي وقت." },

    // FAQ
    new PageContent { PageName = "FAQ", Section = "Header", Content = "الأسئلة الشائعة" },
    new PageContent { PageName = "FAQ", Section = "Paragraph", Content = "هنا تجد الإجابات على الأسئلة الأكثر شيوعاً حول منصتنا التعليمية." },

    // Offer
    new PageContent { PageName = "Offer", Section = "Header", Content = "العروض الخاصة" },
    new PageContent { PageName = "Offer", Section = "Paragraph", Content = "استفد من أفضل العروض والخصومات المتاحة حالياً على دوراتنا." },

    // Privacy
    new PageContent { PageName = "Privacy", Section = "Header", Content = "سياسة الخصوصية" },
    new PageContent { PageName = "Privacy", Section = "Paragraph", Content = "نلتزم بحماية خصوصيتك وضمان سرية بياناتك أثناء استخدامك للمنصة." },

    // RefundPolicy
    new PageContent { PageName = "RefundPolicy", Section = "Header", Content = "سياسة الاسترجاع" },
    new PageContent { PageName = "RefundPolicy", Section = "Paragraph", Content = "يمكنك طلب استرجاع المبلغ خلال 14 يومًا من تاريخ الشراء وفق الشروط المحددة." },

    // Team
    new PageContent { PageName = "Team", Section = "Header", Content = "فريق العمل" },
    new PageContent { PageName = "Team", Section = "Paragraph", Content = "تعرف على فريق الخبراء الذين يعملون على تقديم أفضل تجربة تعليمية لك." },

    // Terms
    new PageContent { PageName = "Terms", Section = "Header", Content = "الشروط والأحكام" },
    new PageContent { PageName = "Terms", Section = "Paragraph", Content = "باستخدامك للمنصة فإنك توافق على جميع الشروط والأحكام المذكورة هنا." },

        // Categories
        new PageContent { PageName = "Categories", Section = "Header", Content = "التصنيف " },
    new PageContent { PageName = "Categories", Section = "Paragraph", Content = "Catagories" },
        // Categories
        new PageContent { PageName = "Ticker", Section = "Header", Content = "شريط الاعلانات " },
    new PageContent { PageName = "Ticker", Section = "Paragraph", Content = "اعلانات هامة" }

    );



        context.SaveChanges();
    }
}
// ===================== SEED ROLES + SUPERADMIN =====================
using (var scope = app.Services.CreateScope())
{
    try { 

            var services = scope.ServiceProvider;

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
              var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // Create all roles
          string[] roles = { "SuperAdmin", "Admin", "Student", "Instructor", "Finance", "Support", "ContentManager" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Create SuperAdmin user if not exists
    var existing = await userManager.FindByNameAsync("ABUHMAM84");

    if (existing == null)
    {
        var user = new ApplicationUser
        {
            UserName = "ABUHMAM84",
            Email = "abuhmam84@gmail.com",
            EmailConfirmed = true,
            FullName = "Super Admin",
            IsPaid = true,
            CreatedAt = DateTime.Now,
            PlainPassword = null,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user);

        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, "SuperAdmin");
    }

}
    catch (Exception ex)
    {
        Console.WriteLine($"Seed error: {ex.Message}");
    }

}


/*


// ===================== SEED ROLES + ADMIN =====================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        if (!await roleManager.RoleExistsAsync("Student"))
            await roleManager.CreateAsync(new IdentityRole("Student"));

        //==================================
        // تأكد من وجود الدور
        var existing = await userManager.FindByNameAsync("ABUHMAM84");

        if (existing == null)
        {
            var user = new ApplicationUser
            {
                UserName = "ABUHMAM84",
                Email = "abuhmam84@gmail.com",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "SuperAdmin123!");

            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, "SuperAdmin");
        }

        //========================================

        /* if (await userManager.FindByNameAsync("admin") == null)
         {
             var admin = new ApplicationUser
             {
                 UserName = "admin",
                 Email = "admin@example.com",
                 FullName = "مدير النظام",
                 EmailConfirmed = true,
                 IsPaid = false,
                 PlainPassword = "Admin123!"
             };

             var result = await userManager.CreateAsync(admin, "Admin123!");
             if (result.Succeeded)
                 await userManager.AddToRoleAsync(admin, "Admin");
         }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Seed error: {ex.Message}");
    }
}
*/
// ===================== MIDDLEWARE =====================



if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<ClientSettingMiddleware>();
app.UseMiddleware<LicenseMiddleware>();// LICENSE MIDDLEWARE==========================

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<SessionValidationMiddleware>();


//تدخل لوحة التحكم → LicensesController / Index

//ClientName: اسم العميل

//LicenseKey: تضعه يدويًا الآن (مثلاً: CL1-2025-ABC123)

//ExpirationDate: تاريخ انتهاء الاشتراك

//IsActive: = true

//ترفع نسخة Publish من الموقع على سيرفر العميل.

//يربط الدومين بالسيرفر.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapRazorPages();

app.Run();
