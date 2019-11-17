using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Claims.Polygon.Core;
using Claims.Polygon.Services.Interfaces;
using Claims.Polygon.Services.Mappings;
using CsvHelper;
using Microsoft.AspNetCore.Http;

namespace Claims.Polygon.Services
{
    public class CsvService : ICsvService
    {
        public async Task<IEnumerable<Claim>> GetIncrementalClaims(IFormFile csvFile)
        {
            using var streamReader = new StreamReader(csvFile.OpenReadStream());

            using var csvReader = new CsvReader(streamReader);
            csvReader.Configuration.RegisterClassMap<ClaimMap>();

            var result = await Task.Run(() => csvReader.GetRecords<Claim>().ToList());

            return result;
        }
    }
}
