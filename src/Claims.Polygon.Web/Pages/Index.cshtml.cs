using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Claims.Polygon.Core.Csv;
using Claims.Polygon.Core.Enums;
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

            var header = new CumulativeHeader
            {
                MinOriginYear = 1990,
                DevelopmentYears = 5
            };
            var data = new CumulativeValue
            {
                Type = ProductType.Comp,
                Values = new List<double> { 5, 10, 15, 20 }
            };
            var data2 = new CumulativeValue
            {
                Type = ProductType.NonComp,
                Values = new List<double> { 50, 100, 150, 200 }
            };

            var cumulativeData = new CumulativeData {Header = header, Values = new List<CumulativeValue> {data, data2}};

            var temp = await _csvService.GetCumulativeCsv(cumulativeData);

            var memoryStream = new MemoryStream(temp);

            return new FileStreamResult(memoryStream, "text/csv") {FileDownloadName = "cumulative.csv"};
        }
    }
}
