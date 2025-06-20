# ![Icon](https://raw.githubusercontent.com/LaoSeng99/LS.FileReader/refs/heads/main/LS.FileReader/icon.png) LS.FileReader

**A lightweight C# utility to read CSV, Excel (XLSX), and JSON files into strongly-typed models using attribute-based mapping and optional streaming support.**

- ✅ Supports `.csv`, `.xlsx`, `.json`
- ✅ Attribute-based property mapping
- ✅ `Read<T>()` for small files (sync, memory-based)
- ✅ `ReadAsync<T>()` for large files (async stream)
- ✅ Built with clean architecture & extensibility
- ✨ Created with AI-assisted engineering (OpenAI ChatGPT)
- ⚙️ Powered by [ExcelDataReader](https://github.com/ExcelDataReader/ExcelDataReader)

---

## 🔧 Installation

```bash
dotnet add package LS.FileReader
```

---

## 🚀 Quick Start

### 1. Define your model with `HeaderColumnAttribute`

```csharp
using LS.FileReader.Attributes;

public class PersonDto
{
    [HeaderColumn("Name", "FullName")]
    public string Name { get; set; }

    [HeaderColumn("Age")]
    public int Age { get; set; }

    [HeaderColumn("DOB")]
    public DateTime BirthDate { get; set; }
}
```

---

### 2. Use `ILsFileReader.Read<T>()` to fully load and parse the file

```csharp
var reader = new LsFileReader();

var result = reader.Read<PersonDto>(formFile); // Supports .csv, .xlsx, .json

foreach (var person in result.Data)
{
    Console.WriteLine($"{person.Name}, {person.Age}");
}

foreach (var error in result.ErrorRows)
{
    Console.WriteLine($"Row {error.RowIndex} failed: {error.Error}");
}
```

---

### 3. Use `ReadAsync<T>()` for large files (streaming)

```csharp
await foreach (var row in reader.ReadAsync<PersonDto>(formFile))
{
    if (row.IsSuccess)
    {
        var person = row.Data;
        // Process person
        // Do something at here, add to db / signal r notify
    }
    else
    {
        Console.WriteLine($"Row {row.RowIndex} failed: {row.Error}");
    }
}
```

---


### 4. Use `ReadAsync<T>()` + `FileEstimateHelper` for large files (streaming) and batch process

```csharp
var batch = new List<PersonDto>();
var batchSize = 100;
var lastNotifyTime = DateTime.UtcNow;
var estimatedTotalRows = 0;

await foreach (var row in reader.ReadAsync<PersonDto>(formFile))
{
    if (!row.IsSuccess)
    {
        Console.WriteLine($"Row {row.RowIndex} failed: {row.Error}");
        continue;
    }

    // Estimate total rows once (based on file size and column count)
    if (estimatedTotalRows == 0)
    {
        estimatedTotalRows = FileEstimateHelper.EstimateTotalRows(formFile, row.ColumnCount);
    }

    batch.Add(row.Data);

    if (batch.Count >= batchSize)
    {
        await db.SaveBatchAsync(batch); // Replace with your batch save logic
        batch.Clear();
    }

    // Notify progress every 250ms
    if ((DateTime.UtcNow - lastNotifyTime).TotalMilliseconds >= 250)
    {
        double percent = (double)row.RowIndex / estimatedTotalRows * 100;
        await notifier.SendProgressAsync(connectionId, percent);
        lastNotifyTime = DateTime.UtcNow;
    }
}

// Flush remaining rows
if (batch.Count > 0)
{
    await db.SaveBatchAsync(batch);
}
```

---

## 📂 Supported Formats

| Format  | Method                        | Notes                          |
| ------- | ----------------------------- | ------------------------------ |
| `.csv`  | `Read<T>()`, `ReadAsync<T>()` | Header must match column/alias |
| `.xlsx` | `Read<T>()`, `ReadAsync<T>()` | First sheet is used by default |
| `.json` | `Read<T>()` only              | Must be JSON array             |

---

## 📘 Notes

- All file reads are limited to **10MB** to avoid memory spikes.
- Property names are matched case-insensitively to headers or aliases.
- Internally uses `System.Text.Json` for JSON and `ExcelDataReader` for Excel files.

---

## 👤 Author

- Laoseng (developer)
- OpenAI ChatGPT (AI-assisted architecture & implementation)

---

## 📝 License

MIT License  
Includes Excel parsing powered by [ExcelDataReader (MIT)](https://github.com/ExcelDataReader/ExcelDataReader)
