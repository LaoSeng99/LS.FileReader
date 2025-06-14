using System;
using System.Collections.Generic;

namespace LS.FileReader.Models
{
    public class FileImportResult<T>
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Author { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public int TotalRows => RowCount + ErrorRows.Count;
        public int RowCount { get; set; }
        public int FailureRowsCount => ErrorRows.Count;
        public List<FileImportError> ErrorRows { get; set; } = new List<FileImportError>();
        public List<T> Data { get; set; } = new List<T>();
    }

    public class StreamRowResult<T>
    {
        public int RowIndex { get; set; }
        public int ColumnCount { get; set; }
        public bool IsSuccess { get; set; }
        public T Data { get; set; }
        public string Error { get; set; }
    }


    public class FileImportError
    {
        public int RowIndex { get; set; }
        public string Error { get; set; }
        public FileImportError(int rowIndex, string error)
        {
            RowIndex = rowIndex;
            Error = error;
        }
    }
}
