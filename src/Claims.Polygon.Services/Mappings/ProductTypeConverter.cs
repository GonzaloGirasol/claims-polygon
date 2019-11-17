using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Claims.Polygon.Core.Enums;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Claims.Polygon.Services.Mappings
{
    public class ProductTypeConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            var displayNames = GetEnumDisplayNames();
            return Enum.TryParse<ProductType>(text, out var parsedProductType) &&
                   Enum.IsDefined(typeof(ProductType), parsedProductType)
                ? parsedProductType
                : displayNames[text.ToLower()];
        }

        public override string ConvertToString(object value, IWriterRow row,MemberMapData memberMapData)
        {
            return value.GetType()
                .GetMember(value.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                .GetName();
        }

        private IDictionary GetEnumDisplayNames()
        {
            IDictionary displayNameMapping = new Dictionary<string, ProductType>();

            var type = typeof(ProductType);

            Enum.GetNames(type).ToList().ForEach(name =>
            {
                var member = type.GetMember(name).First();
                var displayAttribute = (DisplayAttribute)member.GetCustomAttributes(typeof(DisplayAttribute), false).First();
                displayNameMapping.Add(displayAttribute.Name.ToLower(), (ProductType)Enum.Parse(type, name));
            });

            return displayNameMapping;
        }
    }
}
