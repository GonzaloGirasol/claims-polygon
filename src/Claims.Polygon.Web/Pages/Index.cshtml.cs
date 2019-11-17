using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Claims.Polygon.Core.Constants;
using Claims.Polygon.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Claims.Polygon.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ICsvService _csvService;
        private readonly ICumulativeService _cumulativeService;
        private readonly string[] _permittedExtensions = { ".csv" };

        [BindProperty]
        [Required(ErrorMessage = "Please select a file")]
        public IFormFile CsvFile { get; set; }

        public IndexModel(ICsvService csvService, ICumulativeService cumulativeService)
        {
            _csvService = csvService;
            _cumulativeService = cumulativeService;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var ext = Path.GetExtension(CsvFile.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !_permittedExtensions.Contains(ext))
            {
                ModelState.AddModelError("CsvFile", "Invalid file type.");
                return Page();
            }

            var incrementalClaims = await _csvService.GetIncrementalClaims(CsvFile);

            var cumulativeData = await _cumulativeService.GetCumulativeData(incrementalClaims);

            var temp = await _csvService.GetCumulativeCsv(cumulativeData);

            var memoryStream = new MemoryStream(temp);

            return new FileStreamResult(memoryStream, FileUpload.CsvContentType)
            { FileDownloadName = FileUpload.CumulativeCsvFileName };
        }
    }
}
