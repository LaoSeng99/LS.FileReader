using System.Collections.Generic;
using LS.FileReader.Models;
using Microsoft.AspNetCore.Http;

namespace LS.FileReader.Interfaces
{
    public interface ILsFileReader
    {
        /// <summary>
        /// Read entire file into memory, with basic mapping. Max 10MB limit.
        /// </summary>
        FileImportResult<T> Read<T>(IFormFile file, string sheetName = null) where T : new();

        /// <summary>
        /// Stream file row by row asynchronously. Suitable for large files.
        /// </summary>
        IAsyncEnumerable<StreamRowResult<T>> ReadAsync<T>(IFormFile file) where T : new();
    }
}
