using Claims.Polygon.Core.Enums;

namespace Claims.Polygon.Core
{
    public class Claim
    {
        public ProductType Type { get; set; }
        public int OriginYear { get; set; }
        public int DevelopmentYear { get; set; }
        public double? Value { get; set; }
    }
}
