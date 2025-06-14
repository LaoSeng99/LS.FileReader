using System.Collections.Generic;
using System.IO;
using System.Text;
using LS.FileReader.Helper;
using LS.FileReader.Interfaces;
using LS.FileReader.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace LS.FileReader.Reader
{
    internal class CsvReader : IFormatReader
    {
        public FileImportResult<T> Read<T>(IFormFile file) where T : new()
        {
            var result = new FileImportResult<T>
            {
                FileName = file.FileName,
                ContentType = file.ContentType ?? "text/csv"
            };

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);

            var headerLine = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(headerLine))
                return result;

            var headers = headerLine.Split(',').Select(h => h.Trim()).ToList();
            var propMap = PropertyMappingCache.Build<T>();
            int rowIndex = 1;

            while (!reader.EndOfStream)
            {
                rowIndex++;
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = line.Split(',');
                var item = new T();
                bool hasError = false;
                string error = null;

                for (int i = 0; i < headers.Count && i < values.Length; i++)
                {
                    var header = headers[i];
                    if (!propMap.TryGetValue(header, out var prop)) continue;

                    if (!TypeConversionHelper.TryConvert(values[i], prop.Type, out var val))
                    {
                        hasError = true;
                        error = $"Row {rowIndex}, Column '{header}' parse failed.";
                        break;
                    }

                    prop.Setter(item, val);
                }

                if (hasError)
                    result.ErrorRows.Add(new FileImportError(rowIndex, error));
                else
                    result.Data.Add(item);
            }

            result.RowCount = result.Data.Count;
            return result;
        }

        public async IAsyncEnumerable<StreamRowResult<T>> ReadStream<T>(IFormFile file) where T : new()
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);

            var headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(headerLine))
                yield break;

            var headers = headerLine.Split(',').Select(h => h.Trim()).ToList();
            var propMap = PropertyMappingCache.Build<T>();
            int rowIndex = 1;

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                rowIndex++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = line.Split(',');
                var item = new T();
                string error = null;
                bool hasError = false;

                for (int i = 0; i < headers.Count && i < values.Length; i++)
                {
                    var header = headers[i];
                    if (!propMap.TryGetValue(header, out var prop)) continue;

                    if (!TypeConversionHelper.TryConvert(values[i], prop.Type, out var val))
                    {
                        hasError = true;
                        error = $"Row {rowIndex}, Column '{header}' parse failed.";
                        break;
                    }

                    prop.Setter(item, val);
                }

                yield return new StreamRowResult<T>
                {
                    RowIndex = rowIndex,
                    ColumnCount = headers.Count,
                    IsSuccess = !hasError,
                    Data = hasError ? default : item,
                    Error = error
                };
            }
        }
 }
}
