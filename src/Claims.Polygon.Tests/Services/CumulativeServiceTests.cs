using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Claims.Polygon.Core;
using Claims.Polygon.Core.Enums;
using Claims.Polygon.Core.Exceptions;
using Claims.Polygon.Services;
using NUnit.Framework;

namespace Claims.Polygon.Tests.Unit.Services
{
    public class CumulativeServiceTests
    {
        [Test]
        public void GetCumulativeData_ShouldThrowException_IfInputIsInvalid()
        {
            // Arrange
            var service = new CumulativeService();

            // Act
            async Task AsyncAct() => await service.GetCumulativeData(null);

            // Assert
            var ex = Assert.ThrowsAsync<CumulativeException>(AsyncAct);

            Assert.That(ex.Type, Is.EqualTo(CumulativeExceptionType.InvalidInput));
        }

        [Test]
        public async Task GetCumulativeData_ShouldReturn_ExpectedHeader()
        {
            // Arrange
            var service = new CumulativeService();

            const int originYear = 2000;

            var claim1 = new Claim { OriginYear = originYear, DevelopmentYear = originYear, Value = 5, Type = ProductType.Comp };
            var claim2 = new Claim { OriginYear = originYear, DevelopmentYear = originYear + 1, Value = 10, Type = ProductType.Comp };
            var claim3 = new Claim { OriginYear = originYear, DevelopmentYear = originYear + 2, Value = 15, Type = ProductType.Comp };

            // Act
            var result = await service.GetCumulativeData(new List<Claim> { claim1, claim2, claim3 });

            // Assert
            Assert.That(result.Header.MinOriginYear, Is.EqualTo(originYear));
            Assert.That(result.Header.DevelopmentYears, Is.EqualTo(claim3.DevelopmentYear - originYear + 1));
        }

        [Test]
        public async Task GetCumulativeData_ShouldReturn_ExpectedValues()
        {
            // Arrange
            var service = new CumulativeService();

            const int originYear = 2000;

            var claim1 = new Claim { OriginYear = originYear, DevelopmentYear = originYear, Value = 5, Type = ProductType.Comp };
            var claim2 = new Claim { OriginYear = originYear, DevelopmentYear = originYear + 1, Value = 10, Type = ProductType.Comp };
            var claim3 = new Claim { OriginYear = originYear, DevelopmentYear = originYear + 2, Value = 15, Type = ProductType.Comp };

            // Act
            var result = await service.GetCumulativeData(new List<Claim> { claim1, claim2, claim3 });

            // Assert
            var expectedValues = new List<double>
            {
                claim1.Value.Value,
                claim1.Value.Value + claim2.Value.Value,
                claim1.Value.Value + claim2.Value.Value + claim3.Value.Value
            };

            Assert.AreEqual(expectedValues, result.CumulativeValues.First().Values);
        }

        [Test]
        public async Task GetCumulativeData_ShouldReturn_ExpectedCumulativeValuesCount()
        {
            // Arrange
            var service = new CumulativeService();

            const int originYear = 2000;

            var claim1 = new Claim { OriginYear = originYear, DevelopmentYear = originYear, Value = 5, Type = ProductType.Comp };
            var claim2 = new Claim { OriginYear = originYear, DevelopmentYear = originYear + 1, Value = 10, Type = ProductType.Comp };

            var claimA = new Claim { OriginYear = originYear, DevelopmentYear = originYear, Value = 5, Type = ProductType.NonComp };
            var claimB = new Claim { OriginYear = originYear, DevelopmentYear = originYear + 1, Value = 10, Type = ProductType.NonComp };

            var incrementalData = new List<Claim> { claim1, claim2, claimA, claimB };

            var expectedCount = incrementalData.GroupBy(data => data.Type).Select(g => g.Key).Count();

            // Act
            var result = await service.GetCumulativeData(incrementalData);

            // Assert
            Assert.AreEqual(expectedCount, result.CumulativeValues.Count());
        }

        [Test]
        public async Task GetCumulativeData_ShouldReturn_EqualValues_ForAllCumulativeValues()
        {
            // Arrange
            var service = new CumulativeService();

            const int originYear = 2000;
            const int originYear2 = originYear + 1;

            // span over 3 development years - missing 2nd year.
            var claim1 = new Claim { OriginYear = originYear, DevelopmentYear = originYear, Value = 5, Type = ProductType.Comp };
            var claim2 = new Claim { OriginYear = originYear, DevelopmentYear = originYear + 2, Value = 10, Type = ProductType.Comp };

            var claimA = new Claim { OriginYear = originYear2, DevelopmentYear = originYear2, Value = 5, Type = ProductType.NonComp };
            var claimB = new Claim { OriginYear = originYear2, DevelopmentYear = originYear2 + 1, Value = 10, Type = ProductType.NonComp };

            var incrementalData = new List<Claim> { claim1, claim2, claimA, claimB };

            const int expectedCount = 5;

            // Act
            var result = await service.GetCumulativeData(incrementalData);

            // Assert
            var comps = result.CumulativeValues.First(c => c.Type == ProductType.Comp);
            var nonComps = result.CumulativeValues.First(c => c.Type == ProductType.NonComp);

            Assert.AreEqual(expectedCount, comps.Values.Count());
            Assert.AreEqual(expectedCount, nonComps.Values.Count());
        }

        [Test]
        public async Task GetCumulativeData_ShouldReturn_CumulativeValues_WithDefaultValue_IfMissing()
        {
            // Arrange
            var service = new CumulativeService();

            const int originYear = 2000;
            const int originYear2 = originYear + 1;

            var claim1 = new Claim { OriginYear = originYear, DevelopmentYear = originYear, Value = 5, Type = ProductType.Comp };
            var claim2 = new Claim { OriginYear = originYear, DevelopmentYear = originYear + 1, Value = 10, Type = ProductType.Comp };
            var claim3 = new Claim { OriginYear = originYear, DevelopmentYear = originYear + 2, Value = 15, Type = ProductType.Comp };

            var claimA = new Claim { OriginYear = originYear2, DevelopmentYear = originYear2, Value = 5, Type = ProductType.NonComp };
            var claimB = new Claim { OriginYear = originYear2, DevelopmentYear = originYear2 + 1, Value = 10, Type = ProductType.NonComp };

            var incrementComps = new List<Claim> { claim1, claim2, claim3 };
            var incrementNonComps = new List<Claim> { claimA, claimB };

            // Act
            var result = await service.GetCumulativeData(incrementComps.Concat(incrementNonComps));

            // Assert
            var comps = result.CumulativeValues.First(c => c.Type == ProductType.Comp);
            var nonComps = result.CumulativeValues.First(c => c.Type == ProductType.NonComp);

            Assert.AreEqual(incrementNonComps.Count, comps.Values.Count(value => value == default));
            Assert.AreEqual(incrementComps.Count, nonComps.Values.Count(value => value == default));
        }

        [Test]
        public async Task GetCumulativeClaims_WithProductType_ReturnsMatchingProductType([Values] ProductType productType)
        {
            // Arrange
            var service = new CumulativeService();

            var claim1 = new Claim { OriginYear = 1, DevelopmentYear = 1, Value = 1, Type = productType };
            var claim2 = new Claim { OriginYear = 1, DevelopmentYear = 2, Value = 2, Type = productType };

            // Act
            var result = await service.GetCumulativeClaims(new List<Claim> { claim1, claim2 });

            // Assert
            Assert.That(result, Has.All.Matches<Claim>(claim => claim.Type == productType));
        }

        [Test]
        public async Task GetCumulativeClaims_WithMultipleProductType_ReturnsMatchingProductType()
        {
            // Arrange
            var service = new CumulativeService();

            var claim1 = new Claim { OriginYear = 1, DevelopmentYear = 1, Value = 1, Type = ProductType.Comp };
            var claim2 = new Claim { OriginYear = 1, DevelopmentYear = 2, Value = 2, Type = ProductType.Comp };
            var claim3 = new Claim { OriginYear = 1, DevelopmentYear = 1, Value = 1, Type = ProductType.NonComp };
            var claim4 = new Claim { OriginYear = 1, DevelopmentYear = 2, Value = 2, Type = ProductType.NonComp };

            var incrementalData = new List<Claim> { claim1, claim2, claim3, claim4 };

            // Act
            var result = (await service.GetCumulativeClaims(incrementalData)).ToList();

            // Assert
            var expectedComp = incrementalData.Count(i => i.Type == ProductType.Comp);
            var expectedNonComp = incrementalData.Count(i => i.Type == ProductType.NonComp);

            Assert.That(result, Has.Exactly(expectedComp).Matches<Claim>(claim => claim.Type == ProductType.Comp));
            Assert.That(result,
                Has.Exactly(expectedNonComp).Matches<Claim>(claim => claim.Type == ProductType.NonComp));
        }

        [Test]
        public async Task GetCumulativeClaims_WithinSameOriginYear_ReturnsCumulativeData([Values] ProductType productType)
        {
            // Arrange
            var service = new CumulativeService();

            const int originYear = 2000;

            var claim1 = new Claim { OriginYear = originYear, DevelopmentYear = originYear, Value = 5, Type = productType };
            var claim2 = new Claim { OriginYear = originYear, DevelopmentYear = originYear + 1, Value = 10, Type = productType };
            var claim3 = new Claim { OriginYear = originYear, DevelopmentYear = originYear + 2, Value = 15, Type = productType };

            var incrementalData = new List<Claim> { claim1, claim2, claim3 };

            // Act
            var result = (await service.GetCumulativeClaims(incrementalData)).ToList();

            // Assert
            var claim1Cumulative = result.First(c =>
                c.Type == claim1.Type &&
                c.OriginYear == claim1.OriginYear &&
                c.DevelopmentYear == claim1.DevelopmentYear);

            var claim2Cumulative = result.First(c =>
                c.Type == claim2.Type &&
                c.OriginYear == claim2.OriginYear &&
                c.DevelopmentYear == claim2.DevelopmentYear);

            var claim3Cumulative = result.First(c =>
                c.Type == claim3.Type &&
                c.OriginYear == claim3.OriginYear &&
                c.DevelopmentYear == claim3.DevelopmentYear);

            Assert.AreEqual(claim1.Value, claim1Cumulative.Value);
            Assert.AreEqual(claim1.Value + claim2.Value, claim2Cumulative.Value);
            Assert.AreEqual(claim1.Value + claim2.Value + claim3.Value, claim3Cumulative.Value);
        }

        [Test]
        public async Task GetCumulativeClaims_MultipleOriginYear_ReturnsCumulativeData([Values] ProductType productType)
        {
            // Arrange
            var service = new CumulativeService();

            const int originYear1 = 2000;
            const int originYear2 = 3000;

            var claim1 = new Claim { OriginYear = originYear1, DevelopmentYear = originYear1, Value = 5, Type = productType };
            var claim2 = new Claim { OriginYear = originYear1, DevelopmentYear = originYear1 + 1, Value = 10, Type = productType };
            var claim3 = new Claim { OriginYear = originYear1, DevelopmentYear = originYear1 + 2, Value = 15, Type = productType };

            var claimA = new Claim { OriginYear = originYear2, DevelopmentYear = originYear2, Value = 50, Type = productType };
            var claimB = new Claim { OriginYear = originYear2, DevelopmentYear = originYear2 + 1, Value = 100, Type = productType };

            var incrementalData = new List<Claim>
            {
                claim1,
                claim2,
                claim3,
                claimA,
                claimB
            };

            // Act
            var result = (await service.GetCumulativeClaims(incrementalData)).ToList();

            // Assert
            var claim1Cumulative = result.First(c =>
                c.Type == claim1.Type &&
                c.OriginYear == claim1.OriginYear &&
                c.DevelopmentYear == claim1.DevelopmentYear);

            var claim2Cumulative = result.First(c =>
                c.Type == claim2.Type &&
                c.OriginYear == claim2.OriginYear &&
                c.DevelopmentYear == claim2.DevelopmentYear);

            var claim3Cumulative = result.First(c =>
                c.Type == claim3.Type &&
                c.OriginYear == claim3.OriginYear &&
                c.DevelopmentYear == claim3.DevelopmentYear);

            var claimACumulative = result.First(c =>
                c.Type == claimA.Type &&
                c.OriginYear == claimA.OriginYear &&
                c.DevelopmentYear == claimA.DevelopmentYear);

            var claimBCumulative = result.First(c =>
                c.Type == claimB.Type &&
                c.OriginYear == claimB.OriginYear &&
                c.DevelopmentYear == claimB.DevelopmentYear);

            Assert.AreEqual(claim1.Value, claim1Cumulative.Value);
            Assert.AreEqual(claim1.Value + claim2.Value, claim2Cumulative.Value);
            Assert.AreEqual(claim1.Value + claim2.Value + claim3.Value, claim3Cumulative.Value);

            Assert.AreEqual(claimA.Value, claimACumulative.Value);
            Assert.AreEqual(claimA.Value + claimB.Value, claimBCumulative.Value);
        }

        [Test]
        public async Task GetCumulativeClaims_MultipleProductType_WithinSameOriginYear_ReturnsCumulativeData()
        {
            // Arrange
            var service = new CumulativeService();

            const int originYear = 2000;

            var claim1 = new Claim { OriginYear = originYear, DevelopmentYear = originYear, Value = 5, Type = ProductType.Comp };
            var claim2 = new Claim { OriginYear = originYear, DevelopmentYear = originYear + 1, Value = 10, Type = ProductType.Comp };

            var claimA = new Claim { OriginYear = originYear, DevelopmentYear = originYear, Value = 150, Type = ProductType.NonComp };
            var claimB = new Claim { OriginYear = originYear, DevelopmentYear = originYear + 1, Value = 200, Type = ProductType.NonComp };

            var incrementalData = new List<Claim> { claim1, claim2, claimA, claimB };

            // Act
            var result = (await service.GetCumulativeClaims(incrementalData)).ToList();

            // Assert
            var claim1Cumulative = result.First(c =>
                c.Type == claim1.Type &&
                c.OriginYear == claim1.OriginYear &&
                c.DevelopmentYear == claim1.DevelopmentYear);

            var claim2Cumulative = result.First(c =>
                c.Type == claim2.Type &&
                c.OriginYear == claim2.OriginYear &&
                c.DevelopmentYear == claim2.DevelopmentYear);

            var claimACumulative = result.First(c =>
                c.Type == claimA.Type &&
                c.OriginYear == claimA.OriginYear &&
                c.DevelopmentYear == claimA.DevelopmentYear);

            var claimBCumulative = result.First(c =>
                c.Type == claimB.Type &&
                c.OriginYear == claimB.OriginYear &&
                c.DevelopmentYear == claimB.DevelopmentYear);

            Assert.AreEqual(claim1.Value, claim1Cumulative.Value);
            Assert.AreEqual(claim1.Value + claim2.Value, claim2Cumulative.Value);

            Assert.AreEqual(claimA.Value, claimACumulative.Value);
            Assert.AreEqual(claimA.Value + claimB.Value, claimBCumulative.Value);
        }

        [Test]
        public async Task GetCumulativeClaims_MultipleOriginYearAndProductType_ReturnsCumulativeData()
        {
            // Arrange
            var service = new CumulativeService();

            const int originYear1 = 2000;
            const int originYear2 = 3000;

            var claim1 = new Claim
            { OriginYear = originYear1, DevelopmentYear = originYear1, Value = 5, Type = ProductType.Comp };
            var claim2 = new Claim
            { OriginYear = originYear1, DevelopmentYear = originYear1 + 1, Value = 10, Type = ProductType.Comp };
            var claim3 = new Claim
            { OriginYear = originYear1, DevelopmentYear = originYear1 + 2, Value = 15, Type = ProductType.Comp };

            var claimA = new Claim
            { OriginYear = originYear2, DevelopmentYear = originYear2, Value = 50, Type = ProductType.NonComp };
            var claimB = new Claim
            { OriginYear = originYear2, DevelopmentYear = originYear2 + 1, Value = 100, Type = ProductType.NonComp };

            var incrementalData = new List<Claim>
            {
                claim1,
                claim2,
                claim3,
                claimA,
                claimB
            };

            // Act
            var result = (await service.GetCumulativeClaims(incrementalData)).ToList();

            // Assert
            var claim1Cumulative = result.First(c =>
                c.Type == claim1.Type &&
                c.OriginYear == claim1.OriginYear &&
                c.DevelopmentYear == claim1.DevelopmentYear);

            var claim2Cumulative = result.First(c =>
                c.Type == claim2.Type &&
                c.OriginYear == claim2.OriginYear &&
                c.DevelopmentYear == claim2.DevelopmentYear);

            var claim3Cumulative = result.First(c =>
                c.Type == claim3.Type &&
                c.OriginYear == claim3.OriginYear &&
                c.DevelopmentYear == claim3.DevelopmentYear);

            var claimACumulative = result.First(c =>
                c.Type == claimA.Type &&
                c.OriginYear == claimA.OriginYear &&
                c.DevelopmentYear == claimA.DevelopmentYear);

            var claimBCumulative = result.First(c =>
                c.Type == claimB.Type &&
                c.OriginYear == claimB.OriginYear &&
                c.DevelopmentYear == claimB.DevelopmentYear);

            Assert.AreEqual(claim1.Value, claim1Cumulative.Value);
            Assert.AreEqual(claim1.Value + claim2.Value, claim2Cumulative.Value);
            Assert.AreEqual(claim1.Value + claim2.Value + claim3.Value, claim3Cumulative.Value);

            Assert.AreEqual(claimA.Value, claimACumulative.Value);
            Assert.AreEqual(claimA.Value + claimB.Value, claimBCumulative.Value);
        }

        [Test]
        public async Task GetCumulativeClaims_MissingDevelopmentYear_ReturnsCumulativeData_WithCompleteDevelopmentYears()
        {
            // Arrange
            var service = new CumulativeService();

            const int originYear = 2000;

            var claim1 = new Claim { OriginYear = originYear, DevelopmentYear = originYear, Value = 5 };
            var claim4 = new Claim { OriginYear = originYear, DevelopmentYear = originYear + 3, Value = 15 };

            var incrementalData = new List<Claim> { claim1, claim4 };

            // Act
            var result = (await service.GetCumulativeClaims(incrementalData)).ToList();

            // Assert
            var claim1Cumulative = result.First(c =>
                c.OriginYear == claim1.OriginYear &&
                c.DevelopmentYear == claim1.DevelopmentYear);

            var claim2Cumulative = result.First(c =>
                c.OriginYear == originYear &&
                c.DevelopmentYear == originYear + 1);

            var claim3Cumulative = result.First(c =>
                c.OriginYear == originYear &&
                c.DevelopmentYear == originYear + 2);

            var claim4Cumulative = result.First(c =>
                c.OriginYear == claim4.OriginYear &&
                c.DevelopmentYear == claim4.DevelopmentYear);

            Assert.AreEqual(claim1.Value, claim1Cumulative.Value);
            Assert.AreEqual(claim1.Value, claim2Cumulative.Value);
            Assert.AreEqual(claim1.Value, claim3Cumulative.Value);
            Assert.AreEqual(claim1.Value + claim4.Value, claim4Cumulative.Value);
        }
    }
}
