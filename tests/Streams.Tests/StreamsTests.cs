using Streams.FileIO;
using Streams.Memory;
using Streams.Pipelines;

namespace Streams.Tests;

public class FileStreamsTests
{
    [Fact]
    public async Task WriteAndReadText_RoundTrips()
    {
        string path    = Path.GetTempFileName();
        string content = "Hello, Streams!";
        try
        {
            await FileStreams.WriteTextAsync(path, content);
            string read = await FileStreams.ReadTextAsync(path);
            Assert.Equal(content, read);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task AppendLine_AppendsContent()
    {
        string path = Path.GetTempFileName();
        try
        {
            await FileStreams.WriteTextAsync(path, "line1");
            await FileStreams.AppendLineAsync(path, "\nline2");
            string content = await FileStreams.ReadTextAsync(path);
            Assert.Contains("line1", content);
            Assert.Contains("line2", content);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task ReadLinesAsync_ReadsAllLines()
    {
        string path  = Path.GetTempFileName();
        string[] expected = ["line one", "line two", "line three"];
        try
        {
            await File.WriteAllLinesAsync(path, expected);
            var actual = new List<string>();
            await foreach (var line in FileStreams.ReadLinesAsync(path))
                actual.Add(line);
            Assert.Equal(expected, actual);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task CopyFileAsync_CopiesContent()
    {
        string src  = Path.GetTempFileName();
        string dest = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(src, "copy me");
            await FileStreams.CopyFileAsync(src, dest);
            Assert.Equal("copy me", await File.ReadAllTextAsync(dest));
        }
        finally { File.Delete(src); File.Delete(dest); }
    }

    [Fact]
    public async Task ReadWriteBinary_RoundTrips()
    {
        string path = Path.GetTempFileName();
        byte[] data = [1, 2, 3, 4, 5];
        try
        {
            await FileStreams.WriteBinaryAsync(path, data);
            byte[] read = await FileStreams.ReadBinaryAsync(path);
            Assert.Equal(data, read);
        }
        finally { File.Delete(path); }
    }
}

public class MemoryStreamTests
{
    [Fact]
    public void SerialiseDeserialise_Integers_RoundTrips()
    {
        var original = new List<int> { 1, 2, 3, 42, -7 };
        byte[] bytes  = MemoryStreamExamples.SerialiseIntegers(original);
        var restored  = MemoryStreamExamples.DeserialiseIntegers(bytes);
        Assert.Equal(original, restored);
    }

    [Fact]
    public void EncodeUtf8_ProducesValidBytes()
    {
        string text  = "Hello, 世界!";
        byte[] bytes = MemoryStreamExamples.EncodeUtf8(text);
        Assert.Equal(text, System.Text.Encoding.UTF8.GetString(bytes));
    }
}

public class CompressionTests
{
    [Fact]
    public async Task GZip_CompressDecompress_RoundTrips()
    {
        byte[] original   = System.Text.Encoding.UTF8.GetBytes("Hello, compression!");
        byte[] compressed = await CompressionExamples.GZipCompressAsync(original);
        byte[] restored   = await CompressionExamples.GZipDecompressAsync(compressed);
        Assert.Equal(original, restored);
        Assert.True(compressed.Length > 0);
    }

    [Fact]
    public async Task Brotli_CompressDecompress_RoundTrips()
    {
        byte[] original   = System.Text.Encoding.UTF8.GetBytes(new string('a', 1000));
        byte[] compressed = await CompressionExamples.BrotliCompressAsync(original);
        byte[] restored   = await CompressionExamples.BrotliDecompressAsync(compressed);
        Assert.Equal(original, restored);
        Assert.True(compressed.Length < original.Length); // Brotli should compress 1000 'a's
    }
}

public class PipelineExamplesTests
{
    [Fact]
    public async Task CountLinesAsync_CountsCorrectly()
    {
        var lines = Enumerable.Range(1, 10).Select(i => $"line {i}");
        int count = await PipelineExamples.CountLinesAsync(lines);
        Assert.Equal(10, count);
    }

    [Fact]
    public async Task ParseKeyValuesAsync_ParsesCorrectly()
    {
        var lines = new[] { "host = localhost", "port = 5432", "db = mydb" };
        var dict  = await PipelineExamples.ParseKeyValuesAsync(lines);

        Assert.Equal("localhost", dict["host"]);
        Assert.Equal("5432",      dict["port"]);
        Assert.Equal("mydb",      dict["db"]);
    }
}
