using System.Collections.Generic;

namespace Claims.Polygon.Core.Csv
{
    public class CumulativeData
    {
        public CumulativeHeader Header { get; set; }
        public IEnumerable<CumulativeValue> CumulativeValues { get; set; }
    }
}
