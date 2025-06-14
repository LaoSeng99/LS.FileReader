﻿using System;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;

[assembly: InternalsVisibleTo("LS.FileReader.Tests")]
namespace LS.FileReader.Helper
{
    public static class FileEstimateHelper
    {
        private const int AvgBytesPerRow = 34;     // Based on real-world sampling
        private const double BufferRatio = 1.03;   // Safety margin

        /// <summary>
        /// Estimate the total number of data rows in the file based on file size and column count.
        /// Assumes average bytes per row from real-world samples.
        /// </summary>
        internal static int EstimateTotalRows(IFormFile file, int columnCount)
        {
            if (file == null || file.Length == 0 || columnCount <= 0)
                return 0;

            var estimated = file.Length / AvgBytesPerRow * BufferRatio;
            return (int)Math.Ceiling(estimated);
        }
    }
}
