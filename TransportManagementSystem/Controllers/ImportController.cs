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
                    errors.Add($"Route '{routeName}' not found");
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
                        errors.Add($"Expedition '{expeditionName}' not found for route '{routeName}'");
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
    }
}
