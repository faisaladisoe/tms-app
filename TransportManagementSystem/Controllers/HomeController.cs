using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    [HttpGet]
    public async Task<IActionResult> Index(string? shipmentRoute, string? productCode, int quantity = 1)
    {
        var viewModel = new Dashboard();

        var routes = await _context.Routes
            .OrderBy(r => r.Name)
            .ToListAsync();
        ViewBag.Routes = routes;

        if (!string.IsNullOrEmpty(shipmentRoute))
        {
            // Find route
            var route = await _context.Routes
                .FirstOrDefaultAsync(r => r.Name == shipmentRoute);

            if (route != null)
            {
                // Find cheapest operation (truck cost per kg)
                var cheapestTruck = await _context.Operations
                    .Include(o => o.Expedition).ThenInclude(e => e.Trucks)
                    .Where(o => o.RouteId == route.Id)
                    .OrderBy(o => o.Rate)
                    .FirstOrDefaultAsync();

                if (cheapestTruck != null)
                {
                    var truck = cheapestTruck.Expedition.Trucks.FirstOrDefault();
                    if (truck != null)
                    {
                        viewModel.Truck = truck;
                        viewModel.RemainingTruckTonnage = truck?.Tonnage ?? 0;
                        viewModel.RemainingTruckVolume = truck?.Volume ?? 0;

                        if (!string.IsNullOrEmpty(productCode))
                        {
                            var product = await _context.Products.FirstOrDefaultAsync(p => p.Code == productCode);
                            if (product != null)
                            {
                                var loaded = new LoadedProduct
                                {
                                    Product = product,
                                    Quantity = quantity,
                                };
                                viewModel.Products.Add(loaded);
                                viewModel.RemainingTruckTonnage -= product.GrossWeight * quantity;
                                viewModel.RemainingTruckVolume -= quantity / product.BoxPerPallet;
                            }
                        }
                    }
                }
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

    [HttpPost("import/all")]
    public async Task<IActionResult> ImportAll(IFormFile file)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);

        var expeditions = ImportHelper.ReadSheet<Expedition>(package.Workbook.Worksheets["Expeditions"]);
        var trucks = ImportHelper.ReadSheet<Truck>(package.Workbook.Worksheets["Trucks"]);
        var routes = ImportHelper.ReadSheet<Models.Route>(package.Workbook.Worksheets["Routes"]);
        var operations = ImportHelper.ReadSheet<Operation>(package.Workbook.Worksheets["Operations"]);
        var products = ImportHelper.ReadSheet<Product>(package.Workbook.Worksheets["Products"]);

        var errors = new List<string>();
        errors.AddRange(ImportValidator.Validate(expeditions, _context));
        errors.AddRange(ImportValidator.Validate(trucks, _context));
        errors.AddRange(ImportValidator.Validate(routes, _context));
        errors.AddRange(ImportValidator.Validate(operations, _context));
        errors.AddRange(ImportValidator.Validate(products, _context));

        if (errors.Any())
            return BadRequest(new { Errors = errors });

        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Expeditions.AddRange(expeditions);
            _context.Trucks.AddRange(trucks);
            _context.Routes.AddRange(routes);
            _context.Operations.AddRange(operations);
            _context.Products.AddRange(products);

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        return Ok("All data imported successfully");
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
