using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Claims.Polygon.Core;
using Claims.Polygon.Core.Enums;
using Claims.Polygon.Core.Exceptions;
using Claims.Polygon.Services.Interfaces;

namespace Claims.Polygon.Services
{
    public class CumulativeService : ICumulativeService
    {
        public async Task<IEnumerable<Claim>> GetCumulativeData(IEnumerable<Claim> incrementalData)
        {
            if (incrementalData == null)
            {
                throw new CumulativeException(CumulativeExceptionType.InvalidInput, "Invalid input parameter");
            }

            var cumulativeData = new List<Claim>();

            var groupedClaims = incrementalData.GroupBy(data => new { data.Type, data.OriginYear });
            foreach (var group in groupedClaims)
            {
                await Task.Run(() =>
                {
                    var cumulativeForYear = GetCumulativeDataForOriginYear(group);
                    cumulativeData.AddRange(cumulativeForYear);
                });
            }

            return cumulativeData;
        }

        private static IEnumerable<Claim> GetCumulativeDataForOriginYear(IEnumerable<Claim> originYear)
        {
            var cumulativeData = new List<Claim>();

            var orderedClaims = originYear.OrderBy(d => d.OriginYear)
                .ThenBy(d => d.DevelopmentYear);

            foreach (var claim in orderedClaims)
            {
                var cumulativeValue = claim.Value;

                if (claim.OriginYear < claim.DevelopmentYear)
                {
                    // find the previous development year
                    var previousClaim = cumulativeData.SingleOrDefault(c =>
                        c.Type == claim.Type &&
                        c.OriginYear == claim.OriginYear &&
                        c.DevelopmentYear == claim.DevelopmentYear - 1);

                    cumulativeValue += previousClaim?.Value;
                }

                cumulativeData.Add(new Claim
                {
                    OriginYear = claim.OriginYear,
                    DevelopmentYear = claim.DevelopmentYear,
                    Type = claim.Type,
                    Value = cumulativeValue
                });
            }

            return cumulativeData;
        }
    }
}
