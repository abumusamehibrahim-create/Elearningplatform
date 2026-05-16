using ELearningPlatform.Models;
using Microsoft.AspNetCore.Identity;

public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SessionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task Invoke(HttpContext context,
     UserManager<ApplicationUser> userManager,
     SignInManager<ApplicationUser> signInManager)
    {
        var path = context.Request.Path.Value?.ToLower();
        // 1) السماح الكامل لصفحات الدخول والخروج والتسجيل
        // Skip middleware for login, logout, register
        if (path.Contains("/account/login") ||
            path.Contains("/account/logout") ||
            path.Contains("/account/register"))
        {
            await _next(context);
            return;
        }

        // If user is NOT logged in → skip
        if (!context.User.Identity.IsAuthenticated)
        {
            await _next(context);
            return;
        }

        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            await signInManager.SignOutAsync();
            context.Response.Redirect("/Account/Login");
            return;
        }

        // SUPERADMIN bypass ALL checks
        if (await userManager.IsInRoleAsync(user, "SuperAdmin"))
        {
            await _next(context);
            return;
        }

        // ADMIN bypass ALL checks
        if (await userManager.IsInRoleAsync(user, "Admin"))
        {
            await _next(context);
            return;
        }
        /*
         // INSTRUCTOR bypass ALL checks
if (await userManager.IsInRoleAsync(user, "Instructor"))
{
    await _next(context);
    return;
}

// FINANCE bypass ALL checks
if (await userManager.IsInRoleAsync(user, "Finance"))
{
    await _next(context);
    return;
}

// SUPPORT bypass ALL checks
if (await userManager.IsInRoleAsync(user, "Support"))
{
    await _next(context);
    return;
}

// CONTENT MANAGER bypass ALL checks
if (await userManager.IsInRoleAsync(user, "ContentManager"))
{
    await _next(context);
    return;
}
          
         
         
         
         */

        // Students only → validate session
        string cookieToken = context.Request.Cookies["SessionToken"];

        if (string.IsNullOrEmpty(cookieToken) || cookieToken != user.SessionToken)
        {
            await signInManager.SignOutAsync();
            context.Response.Redirect("/Account/Login?session=expired");
            return;
        }

        await _next(context);
    }




    /* public async Task Invoke(HttpContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
     {
         if (context.User.Identity.IsAuthenticated)
         {
             var user = await userManager.GetUserAsync(context.User);

             // Admin مستثنى من كل القيود
             // Admin و SuperAdmin مستثنون من كل القيود
             if (await userManager.IsInRoleAsync(user, "Admin") ||
                 await userManager.IsInRoleAsync(user, "SuperAdmin"))
             {
                 await _next(context);
                 return;
             }


             string cookieToken = context.Request.Cookies["SessionToken"];
             string deviceId = context.Request.Headers["User-Agent"].ToString();
             string ip = context.Connection.RemoteIpAddress?.ToString();

             // 1) منع تسجيل الدخول من جهاز آخر
             if (cookieToken != user.SessionToken)
             {
                 await signInManager.SignOutAsync();
                 context.Response.Redirect("/Account/Login?session=expired");
                 return;
             }

             // 2) منع تسجيل الدخول من متصفح آخر
             if (deviceId != user.LastDeviceId)
             {
                 await signInManager.SignOutAsync();
                 context.Response.Redirect("/Account/Login?device=changed");
                 return;
             }

             // 3) منع تسجيل الدخول من IP آخر
             if (ip != user.LastIP)
             {
                 await signInManager.SignOutAsync();
                 context.Response.Redirect("/Account/Login?ip=changed");
                 return;
             }
         }

         await _next(context);
     }*/
}
