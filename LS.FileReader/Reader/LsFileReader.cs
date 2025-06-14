using System;
using System.Collections.Generic;
using System.IO;
using LS.FileReader.Interfaces;
using LS.FileReader.Models;
using Microsoft.AspNetCore.Http;

namespace LS.FileReader.Reader
{
    public class LsFileReader : ILsFileReader
    {
        private const long MaxSize = 10 * 1024 * 1024;

        private readonly Dictionary<string, IFormatReader> _readers;

        public LsFileReader()
        {
            _readers = new Dictionary<string, IFormatReader>(StringComparer.OrdinalIgnoreCase)
            {
                [".csv"] = new CsvReader(),
                [".xlsx"] = new XlsxReader(),
                [".json"] = new JsonReader(),
                // e.g. [".json"] = new JsonReader() — easy extension later
            };
        }

        private IFormatReader GetReader(string ext)
        {
            if (!_readers.TryGetValue(ext, out var reader))
                throw new NotSupportedException($"File format '{ext}' is not supported.");
            return reader;
        }

        public FileImportResult<T> Read<T>(IFormFile file, string sheetName = null) where T : new()
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty.");
            if (file.Length > MaxSize)
                throw new InvalidOperationException("File too large. Limit is 10MB.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            return GetReader(ext).Read<T>(file);
        }

        public IAsyncEnumerable<StreamRowResult<T>> ReadAsync<T>(IFormFile file) where T : new()
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            return GetReader(ext).ReadStream<T>(file);
        }
    }
}
