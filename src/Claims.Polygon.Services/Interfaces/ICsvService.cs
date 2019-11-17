using System.Collections.Generic;
using System.Threading.Tasks;
using Claims.Polygon.Core;
using Microsoft.AspNetCore.Http;

namespace Claims.Polygon.Services.Interfaces
{
    public interface ICsvService
    {
        Task<IEnumerable<Claim>> GetIncrementalClaims(IFormFile csvFile);
    }
}
