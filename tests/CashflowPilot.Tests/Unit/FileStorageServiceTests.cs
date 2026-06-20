using CashflowPilot.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text;
using Xunit;

namespace CashflowPilot.Tests.Unit;

public class FileStorageServiceTests : IDisposable
{
    private readonly string _root;
    private readonly FileStorageService _service;

    public FileStorageServiceTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "cfpilot-tests-" + Guid.NewGuid());
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Storage:Root"] = _root })
            .Build();
        _service = new FileStorageService(config);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }

    private static IFormFile CreateFormFile(string content, string fileName)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName) { Headers = new HeaderDictionary() };
    }

    [Fact]
    public void Constructor_CreatesStorageRootDirectory()
    {
        Directory.Exists(_root).Should().BeTrue();
        _service.GetStorageRoot().Should().Be(_root);
    }

    [Fact]
    public async Task SaveUploadAsync_WritesFileContentsUnderSubfolder()
    {
        var (storedName, fullPath) = await _service.SaveUploadAsync(CreateFormFile("a,b,c", "data.csv"), "uploads/1");

        fullPath.Should().StartWith(Path.Combine(_root, "uploads", "1"));
        storedName.Should().EndWith(".csv");
        File.Exists(fullPath).Should().BeTrue();
        (await File.ReadAllTextAsync(fullPath)).Should().Be("a,b,c");
    }

    [Fact]
    public async Task SaveUploadAsync_SameOriginalFileNameTwice_GeneratesDistinctStoredNames()
    {
        var (storedName1, path1) = await _service.SaveUploadAsync(CreateFormFile("first", "data.csv"), "uploads/1");
        var (storedName2, path2) = await _service.SaveUploadAsync(CreateFormFile("second", "data.csv"), "uploads/1");

        storedName1.Should().NotBe(storedName2);
        path1.Should().NotBe(path2);
        (await File.ReadAllTextAsync(path1)).Should().Be("first");
        (await File.ReadAllTextAsync(path2)).Should().Be("second");
    }

    [Fact]
    public async Task OpenRead_ReturnsStreamWithSavedContent()
    {
        var (_, fullPath) = await _service.SaveUploadAsync(CreateFormFile("hello world", "x.csv"), "uploads/2");

        using var stream = _service.OpenRead(fullPath);
        using var reader = new StreamReader(stream);
        (await reader.ReadToEndAsync()).Should().Be("hello world");
    }

    [Fact]
    public async Task DeleteFile_RemovesExistingFile()
    {
        var (_, fullPath) = await _service.SaveUploadAsync(CreateFormFile("temp", "y.csv"), "uploads/3");
        File.Exists(fullPath).Should().BeTrue();

        _service.DeleteFile(fullPath);

        File.Exists(fullPath).Should().BeFalse();
    }

    [Fact]
    public void DeleteFile_NonExistentPath_DoesNotThrow()
    {
        var missingPath = Path.Combine(_root, "uploads", "does-not-exist.csv");
        var act = () => _service.DeleteFile(missingPath);
        act.Should().NotThrow();
    }
}
