using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LS.FileReader.Interfaces;
using LS.FileReader.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace LS.FileReader.Reader
{
    internal class JsonReader : IFormatReader
    {
        public FileImportResult<T> Read<T>(IFormFile file) where T : new()
        {
            var result = new FileImportResult<T>
            {
                FileName = file.FileName,
                ContentType = file.ContentType ?? "application/json"
            };

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var content = reader.ReadToEnd();

            try
            {
                var items = JsonSerializer.Deserialize<List<T>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                result.Data = items ?? new List<T>();
                result.RowCount = result.Data.Count;
            }
            catch (Exception ex)
            {
                result.ErrorRows.Add(new FileImportError(0, $"JSON parse failed: {ex.Message}"));
            }

            return result;
        }

        public IAsyncEnumerable<StreamRowResult<T>> ReadStream<T>(IFormFile file) where T : new()
        {
            throw new NotImplementedException("Streaming JSON is not supported yet. Consider reading the whole file instead.");
        }
    }
}
