using System.Collections.Generic;
using System.Threading.Tasks;
using Claims.Polygon.Core;
using Claims.Polygon.Core.Constants;
using Claims.Polygon.Core.Csv;
using Claims.Polygon.Services.Interfaces;
using Claims.Polygon.Web.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Claims.Polygon.Tests.Unit.Web.Pages
{
    public class IndexModelTests
    {
        [Test]
        public void OnGet_ReturnsPage()
        {
            // Arrange
            var csvService = new Mock<ICsvService>(MockBehavior.Strict);
            var cumulativeService = new Mock<ICumulativeService>(MockBehavior.Strict);
            var page = new IndexModel(csvService.Object, cumulativeService.Object);

            // Act
            var result = page.OnGet();

            // Assert
            Assert.NotNull(result);
        }

        [Test]
        public async Task OnPostAsync_ReturnsFileStreamResult()
        {
            // Arrange
            var csvService = new Mock<ICsvService>(MockBehavior.Strict);
            var cumulativeService = new Mock<ICumulativeService>(MockBehavior.Strict);
            var page = new IndexModel(csvService.Object, cumulativeService.Object) { CsvFile = GetCsvFile() };

            var incrementalData = new List<Claim> { new Claim() };
            var cumulativeData = new CumulativeData();

            csvService.Setup(cs => cs.GetIncrementalClaims(page.CsvFile))
                .ReturnsAsync(incrementalData);
            csvService.Setup(cs => cs.GetCumulativeCsv(cumulativeData))
                .ReturnsAsync(new byte[1]);
            cumulativeService.Setup(cs => cs.GetCumulativeData(incrementalData))
                .ReturnsAsync(cumulativeData);

            // Act
            var result = await page.OnPostAsync();

            // Assert
            Assert.IsInstanceOf(typeof(FileStreamResult), result);
        }

        [Test]
        public async Task OnPostAsync_ReturnsFileStreamResult_WithCsvContentType()
        {
            // Arrange
            var csvService = new Mock<ICsvService>(MockBehavior.Strict);
            var cumulativeService = new Mock<ICumulativeService>(MockBehavior.Strict);
            var page = new IndexModel(csvService.Object, cumulativeService.Object) { CsvFile = GetCsvFile() };

            var incrementalData = new List<Claim> { new Claim() };
            var cumulativeData = new CumulativeData();

            csvService.Setup(cs => cs.GetIncrementalClaims(page.CsvFile))
                .ReturnsAsync(incrementalData);
            csvService.Setup(cs => cs.GetCumulativeCsv(cumulativeData))
                .ReturnsAsync(new byte[1]);
            cumulativeService.Setup(cs => cs.GetCumulativeData(incrementalData))
                .ReturnsAsync(cumulativeData);

            // Act
            var result = await page.OnPostAsync() as FileStreamResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ContentType == FileUpload.CsvContentType);
        }

        [Test]
        public async Task OnPostAsync_ReturnsFileStreamResult_WithFileName()
        {
            // Arrange
            var csvService = new Mock<ICsvService>(MockBehavior.Strict);
            var cumulativeService = new Mock<ICumulativeService>(MockBehavior.Strict);
            var page = new IndexModel(csvService.Object, cumulativeService.Object) { CsvFile = GetCsvFile() };

            var incrementalData = new List<Claim> { new Claim() };
            var cumulativeData = new CumulativeData();

            csvService.Setup(cs => cs.GetIncrementalClaims(page.CsvFile))
                .ReturnsAsync(incrementalData);
            csvService.Setup(cs => cs.GetCumulativeCsv(cumulativeData))
                .ReturnsAsync(new byte[1]);
            cumulativeService.Setup(cs => cs.GetCumulativeData(incrementalData))
                .ReturnsAsync(cumulativeData);

            // Act
            var result = await page.OnPostAsync() as FileStreamResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.FileDownloadName == FileUpload.CumulativeCsvFileName);
        }

        [Test]
        public async Task OnPostAsync_ReturnsError_IfInvalidFileType()
        {
            // Arrange
            var csvService = new Mock<ICsvService>(MockBehavior.Strict);
            var cumulativeService = new Mock<ICumulativeService>(MockBehavior.Strict);
            var page = new IndexModel(csvService.Object, cumulativeService.Object);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(_ => _.FileName).Returns("input.jpg");

            page.CsvFile = fileMock.Object;

            var incrementalData = new List<Claim> { new Claim() };
            var cumulativeData = new CumulativeData();

            csvService.Setup(cs => cs.GetIncrementalClaims(page.CsvFile))
                .ReturnsAsync(incrementalData);
            csvService.Setup(cs => cs.GetCumulativeCsv(cumulativeData))
                .ReturnsAsync(new byte[1]);
            cumulativeService.Setup(cs => cs.GetCumulativeData(incrementalData))
                .ReturnsAsync(cumulativeData);

            // Act
            await page.OnPostAsync();

            // Assert
            Assert.False(page.ModelState.IsValid);
            Assert.True(page.ModelState.ErrorCount == 1);
        }

        private static IFormFile GetCsvFile()
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(_ => _.FileName).Returns("input.csv");
            fileMock.Setup(_ => _.Length).Returns(1);

            return fileMock.Object;
        }
    }
}
