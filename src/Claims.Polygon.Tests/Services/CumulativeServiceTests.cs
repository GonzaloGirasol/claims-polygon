using System;
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
        public async Task GetCumulativeData_WithProductType_ReturnsMatchingProductType([Values] ProductType productType)
        {
            // Arrange
            var service = new CumulativeService();

            var claim1 = new Claim { OriginYear = 1, DevelopmentYear = 1, Value = 1, Type = productType };
            var claim2 = new Claim { OriginYear = 1, DevelopmentYear = 2, Value = 2, Type = productType };

            // Act
            var result = await service.GetCumulativeData(new List<Claim> { claim1, claim2 });

            // Assert
            Assert.That(result, Has.All.Matches<Claim>(claim => claim.Type == productType));
        }

        [Test]
        public async Task GetCumulativeData_WithMultipleProductType_ReturnsMatchingProductType()
        {
            // Arrange
            var service = new CumulativeService();

            var claim1 = new Claim { OriginYear = 1, DevelopmentYear = 1, Value = 1, Type = ProductType.Comp };
            var claim2 = new Claim { OriginYear = 1, DevelopmentYear = 2, Value = 2, Type = ProductType.Comp };
            var claim3 = new Claim { OriginYear = 1, DevelopmentYear = 1, Value = 1, Type = ProductType.NonComp };
            var claim4 = new Claim { OriginYear = 1, DevelopmentYear = 2, Value = 2, Type = ProductType.NonComp };

            var incrementalData = new List<Claim> { claim1, claim2, claim3, claim4 };

            // Act
            var result = (await service.GetCumulativeData(incrementalData)).ToList();

            // Assert
            var expectedComp = incrementalData.Count(i => i.Type == ProductType.Comp);
            var expectedNonComp = incrementalData.Count(i => i.Type == ProductType.NonComp);

            Assert.That(result, Has.Exactly(expectedComp).Matches<Claim>(claim => claim.Type == ProductType.Comp));
            Assert.That(result,
                Has.Exactly(expectedNonComp).Matches<Claim>(claim => claim.Type == ProductType.NonComp));
        }

        [Test]
        public async Task GetCumulativeData_WithinSameOriginYear_ReturnsCumulativeData([Values] ProductType productType)
        {
            // Arrange
            var service = new CumulativeService();

            const int originYear = 2000;

            var claim1 = new Claim { OriginYear = originYear, DevelopmentYear = originYear, Value = 5, Type = productType };
            var claim2 = new Claim { OriginYear = originYear, DevelopmentYear = originYear + 1, Value = 10, Type = productType };
            var claim3 = new Claim { OriginYear = originYear, DevelopmentYear = originYear + 2, Value = 15, Type = productType };

            var incrementalData = new List<Claim> { claim1, claim2, claim3 };

            // Act
            var result = await service.GetCumulativeData(incrementalData);

            // Assert
            var claim1Cumulative = claim1.Value.Value;
            var claim2Cumulative = claim1.Value.Value + claim2.Value.Value;
            var claim3Cumulative = claim2Cumulative + claim3.Value.Value;

            Assert.That(result,
                Has.One.Matches<Claim>(claim =>
                    claim.Type == claim1.Type &&
                    claim.OriginYear == claim1.OriginYear &&
                    claim.DevelopmentYear == claim1.DevelopmentYear &&
                    claim.Value.Value == claim1Cumulative));

            Assert.That(result,
                Has.One.Matches<Claim>(claim =>
                    claim.Type == claim2.Type &&
                    claim.OriginYear == claim2.OriginYear &&
                    claim.DevelopmentYear == claim2.DevelopmentYear &&
                    claim.Value.Value == claim2Cumulative));

            Assert.That(result,
                Has.One.Matches<Claim>(claim =>
                    claim.Type == claim3.Type &&
                    claim.OriginYear == claim3.OriginYear &&
                    claim.DevelopmentYear == claim3.DevelopmentYear &&
                    claim.Value.Value == claim3Cumulative));
        }

        [Test]
        public async Task GetCumulativeData_MultipleOriginYear_ReturnsCumulativeData([Values] ProductType productType)
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
            var result = await service.GetCumulativeData(incrementalData);

            // Assert
            var claim1Cumulative = claim1.Value.Value;
            var claim2Cumulative = claim1.Value.Value + claim2.Value.Value;
            var claim3Cumulative = claim2Cumulative + claim3.Value.Value;
            var claimACumulative = claimA.Value.Value;
            var claimBCumulative = claimA.Value.Value + claimB.Value.Value;

            Assert.That(result,
                Has.One.Matches<Claim>(claim =>
                    claim.Type == claim1.Type &&
                    claim.OriginYear == claim1.OriginYear &&
                    claim.DevelopmentYear == claim1.DevelopmentYear &&
                    claim.Value.Value == claim1Cumulative));

            Assert.That(result,
                Has.One.Matches<Claim>(claim =>
                    claim.Type == claim2.Type &&
                    claim.OriginYear == claim2.OriginYear &&
                    claim.DevelopmentYear == claim2.DevelopmentYear &&
                    claim.Value.Value == claim2Cumulative));

            Assert.That(result,
                Has.One.Matches<Claim>(claim =>
                    claim.Type == claim3.Type &&
                    claim.OriginYear == claim3.OriginYear &&
                    claim.DevelopmentYear == claim3.DevelopmentYear &&
                    claim.Value.Value == claim3Cumulative));

            Assert.That(result,
                Has.One.Matches<Claim>(claim =>
                    claim.Type == claimA.Type &&
                    claim.OriginYear == claimA.OriginYear &&
                    claim.DevelopmentYear == claimA.DevelopmentYear &&
                    claim.Value.Value == claimACumulative));

            Assert.That(result,
                Has.One.Matches<Claim>(claim =>
                    claim.Type == claimB.Type &&
                    claim.OriginYear == claimB.OriginYear &&
                    claim.DevelopmentYear == claimB.DevelopmentYear &&
                    claim.Value.Value == claimBCumulative));
        }

        [Test]
        public async Task GetCumulativeData_MultipleProductType_WithinSameOriginYear_ReturnsCumulativeData()
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
            var result = await service.GetCumulativeData(incrementalData);

            // Assert
            var claim1Cumulative = claim1.Value;
            var claim2Cumulative = claim1.Value + claim2.Value;

            var claimACumulative = claimA.Value;
            var claimBCumulative = claimA.Value + claimB.Value;

            Assert.That(result,
                Has.One.Matches<Claim>(claim =>
                    claim.Type == claim1.Type &&
                    claim.OriginYear == claim1.OriginYear &&
                    claim.DevelopmentYear == claim1.DevelopmentYear &&
                    claim.Value == claim1Cumulative.Value));

            Assert.That(result,
                Has.One.Matches<Claim>(claim =>
                    claim.Type == claim2.Type &&
                    claim.OriginYear == claim2.OriginYear &&
                    claim.DevelopmentYear == claim2.DevelopmentYear &&
                    claim.Value == claim2Cumulative));

            Assert.That(result,
                Has.One.Matches<Claim>(claim =>
                    claim.Type == claimA.Type &&
                    claim.OriginYear == claimA.OriginYear &&
                    claim.DevelopmentYear == claimA.DevelopmentYear &&
                    claim.Value == claimACumulative));

            Assert.That(result,
                Has.One.Matches<Claim>(claim =>
                    claim.Type == claimB.Type &&
                    claim.OriginYear == claimB.OriginYear &&
                    claim.DevelopmentYear == claimB.DevelopmentYear &&
                    claim.Value.Value == claimBCumulative));
        }

        [Test]
        public async Task GetCumulativeData_MultipleOriginYearAndProductType_ReturnsCumulativeData()
        {
            // Arrange
            var service = new CumulativeService();

            const int originYear1 = 2000;
            const int originYear2 = 3000;

            var claim1 = new Claim
                {OriginYear = originYear1, DevelopmentYear = originYear1, Value = 5, Type = ProductType.Comp};
            var claim2 = new Claim
                {OriginYear = originYear1, DevelopmentYear = originYear1 + 1, Value = 10, Type = ProductType.Comp};
            var claim3 = new Claim
                {OriginYear = originYear1, DevelopmentYear = originYear1 + 2, Value = 15, Type = ProductType.Comp};

            var claimA = new Claim
                {OriginYear = originYear2, DevelopmentYear = originYear2, Value = 50, Type = ProductType.NonComp};
            var claimB = new Claim
                {OriginYear = originYear2, DevelopmentYear = originYear2 + 1, Value = 100, Type = ProductType.NonComp};

            var incrementalData = new List<Claim>
            {
                claim1,
                claim2,
                claim3,
                claimA,
                claimB
            };

            // Act
            var result = await service.GetCumulativeData(incrementalData);

            // Assert
            var claim1Cumulative = claim1.Value.Value;
            var claim2Cumulative = claim1.Value.Value + claim2.Value.Value;
            var claim3Cumulative = claim2Cumulative + claim3.Value.Value;
            var claimACumulative = claimA.Value.Value;
            var claimBCumulative = claimA.Value.Value + claimB.Value.Value;

            Assert.That(result,
                Has.One.Matches<Claim>(claim =>
                    claim.Type == claim1.Type &&
                    claim.OriginYear == claim1.OriginYear &&
                    claim.DevelopmentYear == claim1.DevelopmentYear &&
                    claim.Value.Value == claim1Cumulative));

            Assert.That(result,
                Has.One.Matches<Claim>(claim =>
                    claim.Type == claim2.Type &&
                    claim.OriginYear == claim2.OriginYear &&
                    claim.DevelopmentYear == claim2.DevelopmentYear &&
                    claim.Value.Value == claim2Cumulative));

            Assert.That(result,
                Has.One.Matches<Claim>(claim =>
                    claim.Type == claim3.Type &&
                    claim.OriginYear == claim3.OriginYear &&
                    claim.DevelopmentYear == claim3.DevelopmentYear &&
                    claim.Value.Value == claim3Cumulative));

            Assert.That(result,
                Has.One.Matches<Claim>(claim =>
                    claim.Type == claimA.Type &&
                    claim.OriginYear == claimA.OriginYear &&
                    claim.DevelopmentYear == claimA.DevelopmentYear &&
                    claim.Value.Value == claimACumulative));

            Assert.That(result,
                Has.One.Matches<Claim>(claim =>
                    claim.Type == claimB.Type &&
                    claim.OriginYear == claimB.OriginYear &&
                    claim.DevelopmentYear == claimB.DevelopmentYear &&
                    claim.Value.Value == claimBCumulative));
        }
    }
}
