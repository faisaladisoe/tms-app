using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TransportManagementSystem.Data;
using TransportManagementSystem.Helpers;
using TransportManagementSystem.Models;

namespace TransportManagementSystem.Controllers;

public class HomeController : Controller
{
    private readonly TmsDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(TmsDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProduct(string productCode, int quantity = 1)
    {
        var dashboard = HttpContext.Session.GetObject<Dashboard>("Dashboard") ?? new Dashboard();

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Code == productCode);
        if (product != null)
        {
            dashboard.Products.Add(new LoadedProduct 
            { 
                Product = product, 
                Quantity = quantity 
            });
            dashboard.TotalQuantity += quantity;
            HttpContext.Session.SetObject("Dashboard", dashboard);
        }

        return RedirectToAction("Index"); // no query params, safe to refresh
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? shipmentRoute, string? productCode, int quantity = 1)
    {
        // Load dashboard from session
        var viewModel = HttpContext.Session.GetObject<Dashboard>("Dashboard") ?? new Dashboard();

        // If route is chosen, suggest trucks
        if (!string.IsNullOrEmpty(shipmentRoute))
        {
            var route = await _context.Routes.FirstOrDefaultAsync(r => r.Name == shipmentRoute);
            if (route != null)
            {
                viewModel.ShipmentRoute = route;

                var operations = await _context.Operations
                    .Include(o => o.Expedition).ThenInclude(e => e.Trucks)
                    .Where(o => o.RouteId == route.Id)
                    .ToListAsync();

                var allTruckOps = operations
                    .SelectMany(o => o.Expedition.Trucks.Select(t => new 
                    { 
                        Operation = o, Truck = t 
                    }))
                    .ToList();

                // Compute fit score: prioritize tonnage, then volume
                var demandWeight = viewModel.TotalWeight;
                var demandVolume = viewModel.TotalVolume;

                var ranked = allTruckOps
                    .Select(x => new SuggestedTruck
                    {
                        Truck = x.Truck,
                        Rate = x.Operation.Rate,
                        TotalRate = x.Operation.Rate * viewModel.TotalQuantity,
                        TonnageFit = x.Truck.Tonnage - demandWeight,
                        VolumeFit = x.Truck.Volume - demandVolume
                    })
                    .Where(x => x.TonnageFit >= 0 && x.VolumeFit >= 0) // only trucks that can carry the load
                    .OrderBy(x => x.Rate)       // sort by the cheapest rate
                    .ThenBy(x => x.TonnageFit) // then, minimize leftover tonnage
                    .ThenBy(x => x.VolumeFit)   // then, minimize leftover volume
                    .Take(3)
                    .ToList();

                viewModel.Trucks = ranked;
            }
        }

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> SearchRoutes(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return Json(Enumerable.Empty<string>());

        var results = await _context.Routes
            .Where(r => r.Name.ToLower().StartsWith(term.ToLower()))
            .OrderBy(r => r.Name)
            .Select(r => r.Name)
            .Take(10)
            .ToListAsync();

        return Json(results);
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
