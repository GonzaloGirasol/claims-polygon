using System.Collections.Generic;
using System.Threading.Tasks;
using Claims.Polygon.Core;
using Claims.Polygon.Core.Csv;
using Microsoft.AspNetCore.Http;

namespace Claims.Polygon.Services.Interfaces
{
    public interface ICsvService
    {
        Task<IEnumerable<Claim>> GetIncrementalClaims(IFormFile csvFile);

        Task<byte[]> GetCumulativeCsv(CumulativeData cumulativeData);
    }
}
