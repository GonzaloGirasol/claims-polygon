using System.IO;
using System.Threading.Tasks;
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

        [BindProperty]
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
            var incrementalClaims = await _csvService.GetIncrementalClaims(CsvFile);

            var cumulativeClaims = await _cumulativeService.GetCumulativeData(incrementalClaims);

            var temp = await _csvService.GetCumulativeCsv(cumulativeClaims);

            var memoryStream = new MemoryStream(temp);

            return new FileStreamResult(memoryStream, "text/csv") {FileDownloadName = "cumulative.csv"};
        }
    }
}
