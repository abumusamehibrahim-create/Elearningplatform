using ELearningPlatform.Data;
using ELearningPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

[Authorize(Roles = "SuperAdmin")]
public class LicensesController : Controller
{
    private readonly ApplicationDbContext _context;

    public LicensesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var licenses = _context.Licenses.ToList();
        return View(licenses);
    }

    public IActionResult Create()
    {
        return View(new License { IsActive = true, ExpirationDate = DateTime.UtcNow.AddYears(1) });
    }

    [HttpPost]
    public IActionResult Create(License model)
    {
        try
        {
            // USER ERRORS
            if (string.IsNullOrWhiteSpace(model.ClientName))
            {
                ModelState.AddModelError("", "Client Name is required.");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Domain))
            {
                ModelState.AddModelError("", "Domain is required.");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.LicenseKey))
            {
                ModelState.AddModelError("", "License Key is required.");
                return View(model);
            }

            if (model.ExpirationDate == default)
            {
                ModelState.AddModelError("", "Expiration Date is required.");
                return View(model);
            }

            // SYSTEM ERRORS (Database, Null, Exceptions)
            _context.Licenses.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "License created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            // SYSTEM ERROR MESSAGE
            TempData["SystemError"] = "A system error occurred while saving the license.";
            return View(model);
        }
    }


    public IActionResult Edit(int id)
    {
        var license = _context.Licenses.Find(id);
        if (license == null) return NotFound();
        return View(license);
    }

    // EDII=================================================================
    [HttpPost]
    public IActionResult Edit(License model)
    {
        try
        {
            // USER ERRORS
            if (string.IsNullOrWhiteSpace(model.ClientName))
            {
                ModelState.AddModelError("", "Client Name is required.");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Domain))
            {
                ModelState.AddModelError("", "Domain is required.");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.LicenseKey))
            {
                ModelState.AddModelError("", "License Key is required.");
                return View(model);
            }

            if (model.ExpirationDate == default)
            {
                ModelState.AddModelError("", "Expiration Date is required.");
                return View(model);
            }

            // SYSTEM ERRORS (Database, Null, Exceptions)
            var existing = _context.Licenses.Find(model.Id);
            if (existing == null)
            {
                TempData["SystemError"] = "License not found.";
                return RedirectToAction(nameof(Index));
            }

            existing.ClientName = model.ClientName;
            existing.Domain = model.Domain;
            existing.LicenseKey = model.LicenseKey;
            existing.ExpirationDate = model.ExpirationDate;
            existing.IsActive = model.IsActive;

            _context.Licenses.Update(existing);
            _context.SaveChanges();

            TempData["Success"] = "License updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            TempData["SystemError"] = "A system error occurred while updating the license.";
            return View(model);
        }
    }
    //=========================================================EDIT
    public IActionResult Status()
    {
        var license = _context.Licenses.FirstOrDefault();
        return View(license);
    }
    public IActionResult LicenseInfo()
    {
        var license = _context.Licenses.FirstOrDefault();
        return View(license);
    }
    private string GenerateLicenseKey(string clientName)
    {
        string prefix = "LIC";
        string client = clientName.Replace(" ", "").ToUpper();
        string year = DateTime.UtcNow.Year.ToString();
        string random1 = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
        string random2 = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

        return $"{prefix}-{client}-{year}-{random1}-{random2}";
    }
    public IActionResult GenerateKey(string client)
    {
        var key = GenerateLicenseKey(client ?? "CLIENT");
        return Content(key);
    }


   /* public IActionResult Dashboard()
    {
        var license = _context.Licenses.FirstOrDefault();
        if (license == null) return View("NoLicense");

        var daysLeft = (license.ExpirationDate - DateTime.UtcNow).Days;
        var status = "Active";

        if (!license.IsActive)
            status = "Disabled";
        else if (license.ExpirationDate < DateTime.UtcNow)
            status = "Expired";
        else if (daysLeft <= 30)
            status = "ExpiringSoon";

        var model = new LicenseDashboardViewModel
        {
            ClientName = license.ClientName,
            Domain = license.Domain,
            LicenseKey = license.LicenseKey,
            ExpirationDate = license.ExpirationDate,
            IsActive = license.IsActive,
            DaysLeft = daysLeft,
            Status = status
        };

        return View(model);
    }*/
    public IActionResult Dashboard()
    {
        var licenses = _context.Licenses.ToList();
        return View(licenses);
    }
  
    //==================================================
    public IActionResult Delete(int id)
    {
        var license = _context.Licenses.Find(id);

        if (license == null)
        {
            TempData["SystemError"] = "License not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(license);
    }
    //======================================================
    [HttpPost]
    public IActionResult DeleteConfirmed(int id)
    {
        try
        {
            var license = _context.Licenses.Find(id);

            if (license == null)
            {
                TempData["SystemError"] = "License not found.";
                return RedirectToAction(nameof(Index));
            }

            _context.Licenses.Remove(license);
            _context.SaveChanges();

            TempData["Success"] = "License deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            TempData["SystemError"] = "A system error occurred while deleting the license.";
            return RedirectToAction(nameof(Index));
        }
    }


}
