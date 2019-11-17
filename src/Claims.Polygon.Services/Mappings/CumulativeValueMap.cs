using Claims.Polygon.Core.Csv;
using Claims.Polygon.Core.Enums;
using CsvHelper.Configuration;

namespace Claims.Polygon.Services.Mappings
{
    public sealed class CumulativeValueMap : ClassMap<CumulativeValue>
    {
        public CumulativeValueMap()
        {
            Map(m => m.Type).TypeConverter<ProductTypeConverter>();
            Map(m => m.Values);
        }
    }
}
