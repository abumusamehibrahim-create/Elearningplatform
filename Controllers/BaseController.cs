using ELearningPlatform.Data;
using ELearningPlatform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

public class BaseController : Controller
{
    protected readonly ApplicationDbContext _context;
   // protected readonly ILogger<BaseController> _logger;


    public BaseController(ApplicationDbContext context)
    {
        _context = context;
    }

   /*
    public BaseController(ApplicationDbContext context, ILogger<BaseController> logger)
    {
        _context = context;
        _logger = logger;
    }
   */
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        try
        {
            if (User.Identity.IsAuthenticated)
            {
                var log = new PageVisitLog
                {
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    UserName = User.Identity.Name,
                    PageUrl = context.HttpContext.Request.Path,
                    PageTitle = context.ActionDescriptor.DisplayName
                };

                _context.PageVisitLogs.Add(log);
                _context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
          //  _logger.LogWarning(ex, "Error while logging page visit");

            // Do NOT crash app بسبب اللوق
            // ممكن تضيف logging لاحقاً
        }


        base.OnActionExecuting(context);
    }
   





}
