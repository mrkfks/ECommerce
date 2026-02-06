using Dashboard.Web.Models;
using Dashboard.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Web.Controllers;

[Authorize]
public class CampaignController : Controller
{
    private readonly CampaignApiService _campaignApiService;

    public CampaignController(CampaignApiService campaignApiService)
    {
        _campaignApiService = campaignApiService;
    }

    // GET: Campaign
    public async Task<IActionResult> Index()
    {
        var campaigns = await _campaignApiService.GetAllAsync();
        return View(campaigns);
    }

    // GET: Campaign/Create
    public IActionResult Create()
    {
        return View(new CampaignCreateVm());
    }

    // POST: Campaign/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CampaignCreateVm model, IFormFile? bannerFile)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Clear banner URL for creation - banner will be uploaded separately
        model.BannerImageUrl = string.Empty;

        var (success, campaignId) = await _campaignApiService.CreateAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Kampanya oluşturulurken hata oluştu.");
            return View(model);
        }

        // If a banner file is provided, upload it
        if (bannerFile != null && bannerFile.Length > 0 && campaignId.HasValue)
        {
            var (uploadSuccess, imageUrl) = await _campaignApiService.UploadBannerAsync(campaignId.Value, bannerFile);
            if (uploadSuccess)
            {
                TempData["SuccessMessage"] = "Kampanya ve banner başarıyla oluşturuldu.";
            }
            else
            {
                TempData["WarningMessage"] = "Kampanya oluşturuldu, ancak banner yüklenemedi. Lütfen Düzenle sayfasından tekrar deneyin.";
            }
        }
        else
        {
            TempData["SuccessMessage"] = "Kampanya başarıyla oluşturuldu.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Campaign/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var campaign = await _campaignApiService.GetByIdAsync(id);
        if (campaign == null)
            return NotFound();

        var model = new CampaignUpdateVm
        {
            Name = campaign.Name,
            Description = campaign.Description,
            DiscountPercent = campaign.DiscountPercent,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            BannerImageUrl = campaign.BannerImageUrl
        };

        return View(model);
    }

    // POST: Campaign/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CampaignUpdateVm model, IFormFile? bannerFile)
    {
        if (!ModelState.IsValid)
            return View(model);

        var success = await _campaignApiService.UpdateAsync(id, model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Kampanya güncellenirken hata oluştu.");
            return View(model);
        }

        // If a banner file is provided, upload it
        if (bannerFile != null && bannerFile.Length > 0)
        {
            var (uploadSuccess, imageUrl) = await _campaignApiService.UploadBannerAsync(id, bannerFile);
            if (uploadSuccess)
            {
                TempData["SuccessMessage"] = "Kampanya ve banner başarıyla güncellendi.";
            }
            else
            {
                TempData["WarningMessage"] = "Kampanya güncellendi, ancak banner yüklenemedi.";
            }
        }
        else
        {
            TempData["SuccessMessage"] = "Kampanya başarıyla güncellendi.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Campaign/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _campaignApiService.DeleteAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = "Kampanya başarıyla silindi.";
        }
        else
        {
            TempData["ErrorMessage"] = "Kampanya silinirken hata oluştu.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Campaign/Products/5
    public async Task<IActionResult> Products(int id)
    {
        var campaign = await _campaignApiService.GetByIdAsync(id);
        if (campaign == null)
            return NotFound();

        var products = await _campaignApiService.GetCampaignProductsAsync(id);
        ViewBag.CampaignId = id;
        ViewBag.CampaignName = campaign.Name;

        return View(products);
    }
}
