using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace jaytwo.HashSiphon.Tests;

public class HashSiphonStreamTests
{
    [Theory]
    [InlineData("helloworld.txt", "29b933a8d9a0fcef0af75f1713f4940e")]
    [InlineData("lipsum.txt", "c545355fdbfbfb5149dbe35bea3ff6e8")]
    public async Task MD5_produces_expected_hash_hex(string file, string expectedHashHex)
        => await AssertHashMatches(file, expectedHashHex, HashSiphonStream.CreateMD5);

    [Theory]
    [InlineData("helloworld.txt", "9fbc3fafddca353898269a2f4069e4653083bcdb")]
    [InlineData("lipsum.txt", "a3e4eba4936f15ba0b119cb1c487e889c400b156")]
    public async Task SHA1_produces_expected_hash_hex(string file, string expectedHashHex)
        => await AssertHashMatches(file, expectedHashHex, HashSiphonStream.CreateSHA1);

    [Theory]
    [InlineData("helloworld.txt", "92b772380a3f8e27a93e57e6deeca6c01da07f5aadce78bb2fbb20de10a66925")]
    [InlineData("lipsum.txt", "42afaa42c7b47e870b5b2ef41bb03511098700dc46911ad2e7c2d320861de059")]
    public async Task SHA256_produces_expected_hash_hex(string file, string expectedHashHex)
        => await AssertHashMatches(file, expectedHashHex, HashSiphonStream.CreateSHA256);

    [Fact]
    public async Task GetHashBase64_produces_expected_encoded_hash()
    {
        // arrange
        using var stream = GetStreamFromString("banana");
        using var hashSiphonStream = HashSiphonStream.CreateMD5(stream);
        await ConsumeStream(hashSiphonStream);

        // act
        var actual = hashSiphonStream.GetHashBase64();

        // assert
        Assert.Equal(Convert.ToBase64String(hashSiphonStream.Hash!), actual);
    }

    [Fact]
    public async Task GetHashHex_produces_expected_encoded_hash()
    {
        // arrange
        using var stream = GetStreamFromString("banana");
        using var hashSiphonStream = HashSiphonStream.CreateMD5(stream);
        await ConsumeStream(hashSiphonStream);

        // act
        var actual = hashSiphonStream.GetHashHex();

        // assert
        Assert.Equal(BitConverter.ToString(hashSiphonStream.Hash!).Replace("-", string.Empty).ToLowerInvariant(), actual);
    }

    [Fact]
    public void Hash_is_null_before_stream_is_consumed()
    {
        // arrange
        using var stream = GetStreamFromString("banana");

        // act
        using var hashSiphonStream = HashSiphonStream.CreateMD5(stream);

        // assert
        Assert.Null(hashSiphonStream.Hash);
        Assert.Equal(string.Empty, hashSiphonStream.GetHashHex());
        Assert.Equal(string.Empty, hashSiphonStream.GetHashBase64());
    }

    [Fact]
    public async Task Hash_can_be_accessed_multiple_times_after_finalization()
    {
        // arrange
        using var stream = GetStreamFromString("banana");
        using var hashSiphonStream = HashSiphonStream.CreateMD5(stream);
        await ConsumeStream(hashSiphonStream);

        // act
        var hash1 = hashSiphonStream.Hash;
        var hash2 = hashSiphonStream.Hash;

        // assert
        Assert.Same(hash1, hash2);
    }

    [Fact]
    public async Task HashSiphonStream_returns_original_content()
    {
        // arrange
        const string content = "banana";
        using var stream = GetStreamFromString(content);
        using var hashSiphonStream = HashSiphonStream.CreateMD5(stream);
        using var reader = new StreamReader(hashSiphonStream);

        // act
        var result = await reader.ReadToEndAsync();

        // assert
        Assert.Equal(content, result);
    }

    [Fact]
    public async Task Zero_length_stream_finalizes_hash_on_first_read()
    {
        // arrange
        using var stream = new MemoryStream(); // empty
        using var hashSiphonStream = HashSiphonStream.CreateMD5(stream);
        byte[] buffer = new byte[16];

        // act
        int bytesRead = await hashSiphonStream.ReadAsync(buffer, 0, buffer.Length);

        // assert
        Assert.Equal(0, bytesRead);
        Assert.NotNull(hashSiphonStream.Hash);
    }

    [Fact]
    public async Task DisposeAsync_completes_without_error()
    {
        // arrange
        var stream = GetStreamFromString("banana");
        var hashSiphonStream = HashSiphonStream.CreateMD5(stream);

        // act
        await hashSiphonStream.DisposeAsync(); // should not throw

        // assert
        Assert.True(hashSiphonStream.IsDisposed);
    }

    [Fact]
    public void Constructor_throws_on_null_stream()
    {
        // arrange
        var hashAlgorithm = SHA256.Create();
        Stream stream = null!;

        // act & assert
        Assert.Throws<ArgumentNullException>(() => new HashSiphonStream(stream, hashAlgorithm));
    }

    [Fact]
    public void Constructor_throws_on_null_hash_algorithm()
    {
        // arrange
        using var stream = new MemoryStream();
        HashAlgorithm hashAlgorithm = null!;

        // act & assert
        Assert.Throws<ArgumentNullException>(() => new HashSiphonStream(stream, hashAlgorithm));
    }

    [Fact]
    public void Write_throws_NotSupportedException()
    {
        // arrange
        using var stream = new MemoryStream();
        using var hashSiphonStream = HashSiphonStream.CreateMD5(stream);

        // act & assert
        Assert.Throws<NotSupportedException>(() => hashSiphonStream.Write(Array.Empty<byte>(), 0, 0));
    }

    [Fact]
    public void Seek_throws_NotSupportedException()
    {
        // arrange
        using var stream = new MemoryStream();
        using var hashSiphonStream = HashSiphonStream.CreateMD5(stream);

        // act & assert
        Assert.Throws<NotSupportedException>(() => hashSiphonStream.Seek(0, SeekOrigin.Begin));
    }

    [Fact]
    public void SetLength_throws_NotSupportedException()
    {
        // arrange
        using var stream = new MemoryStream();
        using var hashSiphonStream = HashSiphonStream.CreateMD5(stream);

        // act & assert
        Assert.Throws<NotSupportedException>(() => hashSiphonStream.SetLength(100));
    }

    private static MemoryStream GetStreamFromString(string content) =>
        new MemoryStream(Encoding.UTF8.GetBytes(content));

    private static async Task ConsumeStream(Stream stream)
    {
        byte[] buffer = new byte[8192];
        while (await stream.ReadAsync(buffer, 0, buffer.Length) > 0)
        {
        }
    }

    private async Task AssertHashMatches(string file, string expectedHashHex, Func<Stream, HashSiphonStream> hashSiphonStreamFactory)
    {
        // arrange
        using var stream = GetEmbeddedResourceStream(file);
        using var hashSiphonStream = hashSiphonStreamFactory(stream);

        // act
        await ConsumeStream(hashSiphonStream);

        // assert
        Assert.Equal(expectedHashHex, hashSiphonStream.GetHashHex(), ignoreCase: true);
    }

    private Stream GetEmbeddedResourceStream(string file)
        => this.GetType().Assembly.GetManifestResourceStream(
            this.GetType().Assembly.GetManifestResourceNames().Single(x => x.EndsWith(file, StringComparison.OrdinalIgnoreCase)))!;
}
