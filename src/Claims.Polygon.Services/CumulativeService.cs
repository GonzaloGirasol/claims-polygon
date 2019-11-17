using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Claims.Polygon.Core;
using Claims.Polygon.Core.Csv;
using Claims.Polygon.Core.Enums;
using Claims.Polygon.Core.Exceptions;
using Claims.Polygon.Services.Interfaces;

namespace Claims.Polygon.Services
{
    public class CumulativeService : ICumulativeService
    {
        public async Task<CumulativeData> GetCumulativeData(IEnumerable<Claim> incrementalData)
        {
            if (incrementalData == null)
            {
                throw new CumulativeException(CumulativeExceptionType.InvalidInput, "Invalid input parameter");
            }

            var cumulativeClaims = (await GetCumulativeClaims(incrementalData)).ToList();

            var header = GetCumulativeHeader(
                cumulativeClaims.Min(d => d.OriginYear),
                cumulativeClaims.Max(d => d.DevelopmentYear));

            var values = await GetCumulativeValues(cumulativeClaims);

            return new CumulativeData
            {
                Header = header,
                CumulativeValues = values
            };
        }

        internal async Task<IEnumerable<Claim>> GetCumulativeClaims(IEnumerable<Claim> incrementalData)
        {
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
                    var previousClaim = GetPreviousDevelopmentYear(claim.Type,
                        claim.OriginYear,
                        claim.DevelopmentYear,
                        cumulativeData);

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

        /// <summary>
        /// Get the previous development year. If it does not exist,
        /// recursively go back by each development year until it's found. 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="originYear"></param>
        /// <param name="developmentYear"></param>
        /// <param name="cumulativeData"></param>
        /// <returns></returns>
        private static Claim GetPreviousDevelopmentYear(ProductType type,
            int originYear,
            int developmentYear,
            ICollection<Claim> cumulativeData)

        {
            var previousDevelopmentYear = developmentYear - 1;

            if (originYear > previousDevelopmentYear)
            {
                return null;
            }

            // find the previous development year
            var previousClaim = cumulativeData.SingleOrDefault(c =>
                c.Type == type &&
                c.OriginYear == originYear &&
                c.DevelopmentYear == previousDevelopmentYear);

            if (previousClaim != null)
            {
                return previousClaim;
            }

            previousClaim = GetPreviousDevelopmentYear(type,
                originYear,
                previousDevelopmentYear,
                cumulativeData);

            var newPreviousClaim = new Claim
            {
                OriginYear = previousClaim.OriginYear,
                DevelopmentYear = previousDevelopmentYear,
                Type = previousClaim.Type,
                Value = previousClaim.Value
            };

            cumulativeData.Add(newPreviousClaim);

            return newPreviousClaim;
        }

        private static CumulativeHeader GetCumulativeHeader(int minOriginYear, int maxDevelopmentYear)
        {
            return new CumulativeHeader
            {
                MinOriginYear = minOriginYear,
                DevelopmentYears = maxDevelopmentYear - minOriginYear + 1
            };
        }

        private static async Task<IEnumerable<CumulativeValue>> GetCumulativeValues(
            IReadOnlyCollection<Claim> cumulativeClaims)
        {
            var cumulativeValues = new List<CumulativeValue>();

            var groupedClaims = cumulativeClaims.GroupBy(data => data.Type);
            var cumulativeYears = cumulativeClaims
                .GroupBy(data => new { data.OriginYear, data.DevelopmentYear })
                .Select(g => new CumulativeYear
                {
                    OriginYear = g.Key.OriginYear,
                    DevelopmentYear = g.Key.DevelopmentYear
                });

            foreach (var group in groupedClaims)
            {
                await Task.Run(() =>
                {
                    var cumulativeForType = GetCumulativeDataForType(group.Key, group, cumulativeYears);
                    cumulativeValues.Add(cumulativeForType);
                });
            }

            return cumulativeValues;
        }

        private static CumulativeValue GetCumulativeDataForType(ProductType type,
            IEnumerable<Claim> cumulativeByType,
            IEnumerable<CumulativeYear> cumulativeYears)
        {
            var orderedYears = cumulativeYears
                .OrderBy(y => y.OriginYear)
                .ThenBy(y => y.DevelopmentYear);

            var values = orderedYears
                .Select(year =>
                    cumulativeByType.SingleOrDefault(c =>
                        c.Type == type &&
                        c.OriginYear == year.OriginYear &&
                        c.DevelopmentYear == year.DevelopmentYear))
                .Select(claim => claim?.Value ?? 0).ToList();

            return new CumulativeValue
            {
                Type = type,
                Values = values
            };
        }
    }
}
