using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
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
    }
}
