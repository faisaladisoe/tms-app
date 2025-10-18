using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
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
        else
        {
            TempData["Error"] = $"There is no data with the inserted product code ({productCode}).";
            return RedirectToAction("Index");
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportData(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please upload a valid Excel file.";
            return RedirectToAction("Index");
        }

        string[] allowedExtensions = { ".xlsx", ".xls" };
        var extension = Path.GetExtension(file.FileName);
        if (!Array.Exists(allowedExtensions, item => item == extension))
        {
            TempData["Error"] = "Only Excel files (.xlsx, .xls) are supported.";
            return RedirectToAction("Index");
        }

        var dashboard = HttpContext.Session.GetObject<Dashboard>("Dashboard") ?? new Dashboard();

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);

        List<Order> orders = new List<Order>();
        try
        {
            orders = ImportHelper.ReadSheet<Order>(package.Workbook.Worksheets["Orders"]);
        }
        catch
        {
            TempData["Error"] = "Invalid data format. There is no Orders sheet in your excel file.";
            return RedirectToAction("Index");
        }

        var errors = new List<string>();
        errors.AddRange(ImportValidator.Validate(orders, _context));
        if (errors.Count > 0)
            return BadRequest(new { Errors = errors });
        errors.Clear();

        foreach (var order in orders)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Code == order.Code);
            if (product != null)
            {
                dashboard.Products.Add(new LoadedProduct
                {
                    Product = product,
                    Quantity = order.Quantity
                });
                dashboard.TotalQuantity += order.Quantity;
            }
        }

        HttpContext.Session.SetObject("Dashboard", dashboard);
        TempData["Success"] = "Products imported successfully.";

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? shipmentRoute)
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

                if (ranked.Count <= 0)
                {
                    TempData["Error"] = $"Sorry, no trucks are available that can handle {viewModel.TotalWeight:F3} kg or {viewModel.TotalVolume:F3} pallets.";
                    return RedirectToAction("Index");
                }

                viewModel.Trucks = ranked;
            }
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ClearProducts()
    {
        HttpContext.Session.Remove("Dashboard");

        TempData["Success"] = "All products cleared.";
        return RedirectToAction("Index");
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
