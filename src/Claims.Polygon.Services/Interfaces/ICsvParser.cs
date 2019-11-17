using System.Collections.Generic;
using System.Threading.Tasks;
using Claims.Polygon.Core;
using Microsoft.AspNetCore.Http;

namespace Claims.Polygon.Services.Interfaces
{
    public interface ICsvParser
    {
        Task<IEnumerable<Claim>> GetIncrementalClaims(IFormFile csvFile);
    }
}
