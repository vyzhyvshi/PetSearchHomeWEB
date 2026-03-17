using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PetSearchHome_WEB.Application.Services;
using PetSearchHome_WEB.Models;
using PetSearchHome_WEB.Models.Home;
using PetSearchHome_WEB.Models.Listing;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ListingService _listingService;
        private readonly IAuditLogGateway _auditLogGateway;

        public HomeController(ILogger<HomeController> logger, ListingService listingService, IAuditLogGateway auditLogGateway)
        {
            _logger = logger;
            _listingService = listingService;
            _auditLogGateway = auditLogGateway;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = Guid.NewGuid();

                await _auditLogGateway.RecordAsync(
                    action: "Перегляд головної сторінки", 
                    actorId: currentUserId, 
                    context: "HomeController/Index", 
                    cancellationToken);

                var listings = await _listingService.GetFeaturedAsync(6, cancellationToken);

                var viewModel = new HomeIndexViewModel
                {
                    FeaturedListings = listings
                        .Select(listing => new ListingSummaryViewModel
                        {
                            Id = listing.Id,
                            Title = listing.Title,
                            AnimalType = listing.AnimalType,
                            Location = listing.Location,
                            ListedAt = listing.ListedAt,
                            IsUrgent = listing.IsUrgent
                        })
                        .ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // 2. Логуємо технічну помилку (Error)
                _logger.LogError(ex, "Сталася критична помилка під час завантаження головної сторінки.");
                
                // Повертаємо сторінку з помилкою
                return RedirectToAction(nameof(Error));
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
           // Логуємо сам факт потрапляння на сторінку помилки
            _logger.LogWarning("Користувач потрапив на сторінку Error. RequestId: {RequestId}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
