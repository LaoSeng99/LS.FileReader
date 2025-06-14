using LS.FileReader.Interfaces;
using LS.FileReader.Models;
using LS.FileReader.Reader;
using LS.FileReader.Tests.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
namespace LS.FileReader.Tests;

public class LsFileReaderTests
{
    private readonly ILsFileReader _reader = new LsFileReader();

    [Theory]
    [InlineData("Samples/sample-valid.xlsx")]
    [InlineData("Samples/sample.json")]
    [InlineData("Samples/sample-valid.csv")]
    public void Should_Read_File_And_Map_To_Model(string path)
    {
        var file = WrapAsFormFile(path);
        var result = _reader.Read<TestPerson>(file);

        Assert.NotEmpty(result.Data);
        Assert.All(result.Data, p =>
        {
            Assert.False(string.IsNullOrWhiteSpace(p.Name));
            Assert.InRange(p.Age, 0, 120);
        });
    }

    [Fact]
    public async Task Should_Stream_Read_Csv()
    {
        var file = WrapAsFormFile("Samples/sample-valid.csv");
        var rows = new List<StreamRowResult<TestPerson>>();

        await foreach (var row in _reader.ReadAsync<TestPerson>(file))
            rows.Add(row);

        Assert.NotEmpty(rows);
        Assert.Contains(rows, r => r.IsSuccess);
    }

    [Fact]
    public void Should_Capture_Errors_When_Data_Is_Invalid()
    {
        var file = WrapAsFormFile("Samples/sample-invalid.csv");
        var result = _reader.Read<TestPerson>(file);

        Assert.NotEmpty(result.ErrorRows);
        Assert.True(result.FailureRowsCount > 0);
    }

    private IFormFile WrapAsFormFile(string filePath)
    {
        var stream = File.OpenRead(filePath);
        return new FormFile(stream, 0, stream.Length, "file", Path.GetFileName(filePath))
        {
            Headers = new HeaderDictionary(),
            ContentType = GetContentType(filePath)
        };
    }

    private string GetContentType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".csv" => "text/csv",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".json" => "application/json",
            _ => "application/octet-stream"
        };
    }
}
