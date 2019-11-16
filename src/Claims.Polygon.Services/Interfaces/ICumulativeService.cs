namespace Claims.Polygon.Services.Interfaces
{
    using System.Collections.Generic;
    using Core;

    public interface ICumulativeService
    {
        IEnumerable<Claim> GetCumulativeData(IEnumerable<Claim> incrementalData);
    }
}
