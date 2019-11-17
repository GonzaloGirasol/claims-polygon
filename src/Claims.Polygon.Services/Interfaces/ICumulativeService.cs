using System.Collections.Generic;
using System.Threading.Tasks;
using Claims.Polygon.Core;
using Claims.Polygon.Core.Csv;

namespace Claims.Polygon.Services.Interfaces
{
    public interface ICumulativeService
    {
        Task<CumulativeData> GetCumulativeData(IEnumerable<Claim> incrementalData);
    }
}
