﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Claims.Polygon.Core;
using Claims.Polygon.Core.Csv;
using Claims.Polygon.Core.Enums;
using Claims.Polygon.Core.Exceptions;
using Claims.Polygon.Services.Interfaces;
using Claims.Polygon.Services.Mappings;
using CsvHelper;
using Microsoft.AspNetCore.Http;

namespace Claims.Polygon.Services
{
    public class CsvService : ICsvService
    {
        public async Task<IEnumerable<Claim>> GetIncrementalClaims(IFormFile csvFile)
        {
            using var streamReader = new StreamReader(csvFile.OpenReadStream());

            using var csvReader = new CsvReader(streamReader);
            csvReader.Configuration.RegisterClassMap<ClaimMap>();

            var result = await Task.Run(() =>
            {
                try
                {
                    return csvReader.GetRecords<Claim>().ToList();
                }
                catch (CsvHelperException ex)
                {
                    throw new CsvException(CsvExceptionType.FailedToRead, 
                        "There was problem reading the file", ex);
                }
            });

            return result;
        }

        public async Task<byte[]> GetCumulativeCsv(CumulativeData cumulativeData)
        {
            try
            {
                await using var memoryStream = new MemoryStream();
                await using var writer = new StreamWriter(memoryStream);

                using var csvWriter = new CsvWriter(writer);
                csvWriter.Configuration.HasHeaderRecord = false;
                csvWriter.Configuration.RegisterClassMap<CumulativeValueMap>();

                csvWriter.WriteRecord(cumulativeData.Header);
                await csvWriter.NextRecordAsync();

                foreach (var value in cumulativeData.CumulativeValues)
                {
                    csvWriter.WriteRecord(value);
                    await csvWriter.NextRecordAsync();
                }

                await writer.FlushAsync();

                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                throw new CsvException(CsvExceptionType.FailedToWrite,
                    "There was problem creating the cumulative file", ex);
            }
        }
    }
}
