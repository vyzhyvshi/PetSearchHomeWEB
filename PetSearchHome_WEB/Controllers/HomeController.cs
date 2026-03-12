using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PetSearchHome_WEB.Application.Services;
using PetSearchHome_WEB.Models;
using PetSearchHome_WEB.Models.Home;
using PetSearchHome_WEB.Models.Listing;

namespace PetSearchHome_WEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ListingService _listingService;

        public HomeController(ILogger<HomeController> logger, ListingService listingService)
        {
            _logger = logger;
            _listingService = listingService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
