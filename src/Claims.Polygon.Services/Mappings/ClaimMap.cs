using Claims.Polygon.Core;
using CsvHelper.Configuration;

namespace Claims.Polygon.Services.Mappings
{
    public sealed class ClaimMap : ClassMap<Claim>
    {
        public ClaimMap()
        {
            Map(m => m.Type).Index(0).TypeConverter<ProductTypeConverter>();
            Map(m => m.OriginYear).Index(1);
            Map(m => m.DevelopmentYear).Index(2);
            Map(m => m.Value).Index(3);
        }
    }
}
