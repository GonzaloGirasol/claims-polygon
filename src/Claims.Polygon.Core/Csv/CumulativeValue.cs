using System.Collections.Generic;
using Claims.Polygon.Core.Enums;

namespace Claims.Polygon.Core.Csv
{
    public class CumulativeValue
    {
        public ProductType Type { get; set; }
        public IEnumerable<double> Values { get; set; }
    }
}
