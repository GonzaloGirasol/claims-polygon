using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Claims.Polygon.Core.Csv;
using Claims.Polygon.Core.Enums;
using Claims.Polygon.Core.Exceptions;
using Claims.Polygon.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace Claims.Polygon.Tests.Unit.Services
{
    public class CsvServiceTests
    {
        [Test]
        public async Task GetIncrementalClaims_ValidInput_ReturnsClaims()
        {
            // Arrange
            var service = new CsvService();

            var fileMock = new Mock<IFormFile>();

            const string content = @"
                            Product, Origin Year, Development Year, Incremental Value
                            Comp, 1992, 1992, 110.0";
            const string fileName = "incremental.csv";

            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();

            ms.Position = 0;

            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);

            var file = fileMock.Object;

            // Act
            var result = await service.GetIncrementalClaims(file);

            // Assert
            Assert.True(result.Count() == 1);
        }

        [Test]
        public void GetIncrementalClaims_InvalidInput_ThrowsException()
        {
            // Arrange
            var service = new CsvService();

            var fileMock = new Mock<IFormFile>();

            // Content missing Product
            const string content = @"
                            Product, Origin Year, Development Year, Incremental Value
                            1992, 1992, 110.0";
            const string fileName = "incremental.csv";

            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();

            ms.Position = 0;

            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);

            var file = fileMock.Object;

            // Act
            Task AsyncTest() => service.GetIncrementalClaims(file);

            // Assert
            var ex = Assert.ThrowsAsync<CsvException>(AsyncTest);
            Assert.AreEqual(CsvExceptionType.FailedToRead, ex.Type);
        }

        [Test]
        public async Task GetCumulativeCsv_ValidInput_ReturnsByteArray()
        {
            // Arrange
            var service = new CsvService();

            var cumulativeData = new CumulativeData
            {
                Header = new CumulativeHeader {MinOriginYear = 2000, DevelopmentYears = 5},
                CumulativeValues = new List<CumulativeValue>
                {
                    new CumulativeValue
                    {
                        Type = ProductType.Comp,
                        Values = new List<double> {1, 2, 3}
                    }
                }
            };

            // Act
            var result = await service.GetCumulativeCsv(cumulativeData);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Test]
        public void GetCumulativeCsv_InvalidInput_ThrowsException()
        {
            // Arrange
            var service = new CsvService();

            // Act
            Task AsyncTest() => service.GetCumulativeCsv(null);

            // Assert
            var ex = Assert.ThrowsAsync<CsvException>(AsyncTest);
            Assert.AreEqual(CsvExceptionType.FailedToWrite, ex.Type);
        }
    }
}
