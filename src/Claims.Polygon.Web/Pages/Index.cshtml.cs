using System.Threading.Tasks;
using Claims.Polygon.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Claims.Polygon.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ICsvParser _csvParser;

        [BindProperty]
        public IFormFile CsvFile { get; set; }

        public IndexModel(ICsvParser csvParser)
        {
            _csvParser = csvParser;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _csvParser.GetIncrementalClaims(CsvFile);
            return Page();
        }
    }
}
