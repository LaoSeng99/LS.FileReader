using System.Collections.Generic;
using LS.FileReader.Models;
using Microsoft.AspNetCore.Http;

namespace LS.FileReader.Interfaces
{
    /// <summary>
    /// Format-specific reader interface (e.g., CSV, XLSX, JSON)
    /// </summary>
    internal interface IFormatReader
    {
        /// <summary>
        /// Read whole file and return import result with success & failure info.
        /// </summary>
        FileImportResult<T> Read<T>(IFormFile file) where T : new();

        /// <summary>
        /// Stream file row by row (async), for large file scenarios.
        /// </summary>
        IAsyncEnumerable<StreamRowResult<T>> ReadStream<T>(IFormFile file) where T : new();
    }
}
