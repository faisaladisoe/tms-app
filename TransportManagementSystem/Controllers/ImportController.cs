using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using OfficeOpenXml;
using System.Reflection;
using TransportManagementSystem.Data;
using TransportManagementSystem.Helpers;
using TransportManagementSystem.Models;

namespace TransportManagementSystem.Controllers
{
    public class ImportController : Controller
    {
        private readonly TmsDbContext _context;
        private readonly ILogger<ImportController> _logger;

        public ImportController(TmsDbContext context, ILogger<ImportController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportAll(IFormFile file)
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

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);

            var expeditions = ImportHelper.ReadSheet<Expedition>(package.Workbook.Worksheets["Expeditions"]);
            var routes = ImportHelper.ReadSheet<Models.Route>(package.Workbook.Worksheets["Routes"]);
            var products = ImportHelper.ReadSheet<Product>(package.Workbook.Worksheets["Products"]);

            var errors = new List<string>();
            errors.AddRange(ImportValidator.Validate(expeditions, _context));
            errors.AddRange(ImportValidator.Validate(routes, _context));
            errors.AddRange(ImportValidator.Validate(products, _context));

            // Expeditions, Routes, Products
            if (errors.Count > 0)
                return BadRequest(new { Errors = errors });
            errors.Clear();
            using (var tx = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Expeditions.AddRange(expeditions);
                    _context.Routes.AddRange(routes);
                    _context.Products.AddRange(products);
                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }

            var trucks = new List<Truck>();
            var trucksDto = ImportHelper.ReadSheet<TruckImportDto>(package.Workbook.Worksheets["Trucks"]);
            foreach (var dto in trucksDto)
            {
                var expedition = _context.Expeditions.FirstOrDefault(e => e.Name == dto.ExpeditionName);
                if (expedition == null)
                {
                    errors.Add($"Expedition '{dto.ExpeditionName}' is not found for Truck '{dto.Type}'");
                    continue;
                }

                trucks.Add(new Truck
                {
                    Type = dto.Type,
                    Tonnage = dto.Tonnage,
                    Volume = dto.Volume,
                    ExpeditionId = expedition.Id,
                });
            }
            errors.AddRange(ImportValidator.Validate(trucks, _context));

            // Trucks
            if (errors.Count > 0)
                return BadRequest(new { Errors = errors });
            errors.Clear();
            using (var tx = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Trucks.AddRange(trucks);
                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }

            var operations = new List<Operation>();
            var ws = package.Workbook.Worksheets["Operations"];
            var routeCount = ws.Dimension.Rows;
            var expeditionCount = ws.Dimension.Columns;

            var expeditionNames = new List<string>();
            for (int col = 2; col <= expeditionCount; col++)
                expeditionNames.Add(ws.Cells[1, col].Text.Trim());

            for (int row = 2; row <= routeCount; row++)
            {
                var routeName = ws.Cells[row, 1].Text.Trim();
                var route = _context.Routes.FirstOrDefault(r => r.Name == routeName);
                if (route == null)
                {
                    errors.Add($"Route '{routeName}' is not found");
                    continue;
                }

                for (int col = 2; col <= expeditionCount; col++)
                {
                    var expeditionName = expeditionNames[col - 2];
                    if (string.IsNullOrEmpty(expeditionName))
                        continue;

                    var expedition = _context.Expeditions.FirstOrDefault(e => e.Name == expeditionName);
                    if (expedition == null)
                    {
                        errors.Add($"Expedition '{expeditionName}' is not found for route '{routeName}'");
                        continue;
                    }

                    var rateText = ws.Cells[row, col].Text.Trim();
                    if (string.IsNullOrEmpty(rateText))
                        continue;
                    
                    if (!float.TryParse(rateText, out var rate))
                    {
                        errors.Add($"Invalid rate '{rateText}' for {routeName} / {expeditionName}");
                        continue;
                    }

                    operations.Add(new Operation
                    {
                        Rate = rate,
                        ExpeditionId = expedition.Id,
                        RouteId = route.Id,
                    });
                }
            }
            errors.AddRange(ImportValidator.Validate(operations, _context));

            // Operations
            if (errors.Count > 0)
                return BadRequest(new { Errors = errors });
            using (var tx = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Operations.AddRange(operations);
                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }

            TempData["Success"] = "All data imported successfully";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAll()
        {
            try
            {
                _context.Operations.RemoveRange(_context.Operations);
                _context.Trucks.RemoveRange(_context.Trucks);
                _context.Products.RemoveRange(_context.Products);
                _context.Routes.RemoveRange(_context.Routes);
                _context.Expeditions.RemoveRange(_context.Expeditions);

                await _context.SaveChangesAsync();

                TempData["Success"] = "All data has been cleared successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to clear data: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
