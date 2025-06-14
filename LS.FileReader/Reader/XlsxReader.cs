using System;
using System.Collections.Generic;
using System.Text;
using ExcelDataReader;
using LS.FileReader.Helper;
using LS.FileReader.Interfaces;
using LS.FileReader.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace LS.FileReader.Reader
{
    internal class XlsxReader : IFormatReader
    {
        public FileImportResult<T> Read<T>(IFormFile file) where T : new()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var result = new FileImportResult<T>
            {
                FileName = file.FileName,
                ContentType = file.ContentType ?? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };

            using var stream = file.OpenReadStream();
            using var reader = ExcelReaderFactory.CreateReader(stream);

            var propMap = PropertyMappingCache.Build<T>();
            bool headerParsed = false;
            List<string> headers = null;
            int rowIndex = 0;

            do
            {
                while (reader.Read())
                {
                    rowIndex++;

                    if (!headerParsed)
                    {
                        headers = new List<string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                            headers.Add(reader.GetValue(i)?.ToString()?.Trim() ?? string.Empty);
                        headerParsed = true;
                        continue;
                    }

                    var item = new T();
                    bool hasError = false;
                    string error = null;

                    for (int i = 0; i < headers.Count && i < reader.FieldCount; i++)
                    {
                        var header = headers[i];
                        if (!propMap.TryGetValue(header, out var prop)) continue;

                        var raw = reader.GetValue(i)?.ToString();
                        if (!TypeConversionHelper.TryConvert(raw, prop.Type, out var val))
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
            } while (reader.NextResult());

            result.RowCount = result.Data.Count;
            return result;
        }

        public async IAsyncEnumerable<StreamRowResult<T>> ReadStream<T>(IFormFile file) where T : new()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using var stream = file.OpenReadStream();
            using var reader = ExcelReaderFactory.CreateReader(stream);

            var propMap = PropertyMappingCache.Build<T>();
            bool headerParsed = false;
            List<string> headers = null;
            int rowIndex = 0;
            int estimatedTotal = 0;

            do
            {
                while (reader.Read())
                {
                    rowIndex++;

                    if (!headerParsed)
                    {
                        headers = new List<string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                            headers.Add(reader.GetValue(i)?.ToString()?.Trim() ?? string.Empty);
                        estimatedTotal = FileEstimateHelper.EstimateTotalRows(file, headers.Count);
                        headerParsed = true;
                        continue;
                    }

                    var item = new T();
                    bool hasError = false;
                    string error = null;

                    for (int i = 0; i < headers.Count && i < reader.FieldCount; i++)
                    {
                        var header = headers[i];
                        if (!propMap.TryGetValue(header, out var prop)) continue;

                        var raw = reader.GetValue(i)?.ToString();
                        if (!TypeConversionHelper.TryConvert(raw, prop.Type, out var val))
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

                    if (FileEstimateHelper.ShouldYield(rowIndex, estimatedTotal))
                        await Task.Yield();
                }
            } while (reader.NextResult());
        }
  }
}
