using System.Collections.Generic;
using System.Threading.Tasks;
using Claims.Polygon.Core;

namespace Claims.Polygon.Services.Interfaces
{
    public interface ICumulativeService
    {
        Task<IEnumerable<Claim>> GetCumulativeData(IEnumerable<Claim> incrementalData);
    }
}
