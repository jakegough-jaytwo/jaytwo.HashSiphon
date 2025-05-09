using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Force.Crc32;
using Xunit;

namespace jaytwo.HashSiphon.Tests;

public class HashSiphonStreamTests
{
    public const string BananaMd5Hex = "72b302bf297a228a75730123efef7c41";

    [Theory]
    [InlineData("helloworld.txt", "97F73FF3")]
    [InlineData("lipsum.txt", "8F827C30")]
    public void read_produces_expected_CRC32_hash_hex(string file, string expectedHashHex)
        => AssertReadHashMatches(file, expectedHashHex, x => new HashSiphonStream(x, () => new Crc32Algorithm(), streamDirection: StreamDirection.Read));

    [Theory]
    [InlineData("helloworld.txt", "29b933a8d9a0fcef0af75f1713f4940e")]
    [InlineData("lipsum.txt", "c545355fdbfbfb5149dbe35bea3ff6e8")]
    public void read_produces_expected_MD5_hash_hex(string file, string expectedHashHex)
        => AssertReadHashMatches(file, expectedHashHex, x => HashSiphonStream.CreateMD5Read(x));

    [Theory]
    [InlineData("helloworld.txt", "9fbc3fafddca353898269a2f4069e4653083bcdb")]
    [InlineData("lipsum.txt", "a3e4eba4936f15ba0b119cb1c487e889c400b156")]
    public void read_produces_expected_SHA1_hash_hex(string file, string expectedHashHex)
        => AssertReadHashMatches(file, expectedHashHex, x => HashSiphonStream.CreateSHA1Read(x));

    [Theory]
    [InlineData("helloworld.txt", "92b772380a3f8e27a93e57e6deeca6c01da07f5aadce78bb2fbb20de10a66925")]
    [InlineData("lipsum.txt", "42afaa42c7b47e870b5b2ef41bb03511098700dc46911ad2e7c2d320861de059")]
    public void read_produces_expected_SHA256_hash_hex(string file, string expectedHashHex)
        => AssertReadHashMatches(file, expectedHashHex, x => HashSiphonStream.CreateSHA256Read(x));

    [Theory]
    [InlineData("helloworld.txt", "97F73FF3")]
    [InlineData("lipsum.txt", "8F827C30")]
    public void write_produces_expected_CRC32_hash_hex(string file, string expectedHashHex)
        => AssertWriteHashMatches(file, expectedHashHex, x => new HashSiphonStream(x, () => new Crc32Algorithm(), streamDirection: StreamDirection.Write));

    [Theory]
    [InlineData("helloworld.txt", "29b933a8d9a0fcef0af75f1713f4940e")]
    [InlineData("lipsum.txt", "c545355fdbfbfb5149dbe35bea3ff6e8")]
    public void write_produces_expected_MD5_hash_hex(string file, string expectedHashHex)
        => AssertWriteHashMatches(file, expectedHashHex, x => HashSiphonStream.CreateMD5Write(x));

    [Theory]
    [InlineData("helloworld.txt", "9fbc3fafddca353898269a2f4069e4653083bcdb")]
    [InlineData("lipsum.txt", "a3e4eba4936f15ba0b119cb1c487e889c400b156")]
    public void write_produces_expected_SHA1_hash_hex(string file, string expectedHashHex)
        => AssertWriteHashMatches(file, expectedHashHex, x => HashSiphonStream.CreateSHA1Write(x));

    [Theory]
    [InlineData("helloworld.txt", "92b772380a3f8e27a93e57e6deeca6c01da07f5aadce78bb2fbb20de10a66925")]
    [InlineData("lipsum.txt", "42afaa42c7b47e870b5b2ef41bb03511098700dc46911ad2e7c2d320861de059")]
    public void write_produces_expected_SHA256_hash_hex(string file, string expectedHashHex)
        => AssertWriteHashMatches(file, expectedHashHex, x => HashSiphonStream.CreateSHA256Write(x));

    [Theory]
    [InlineData("helloworld.txt", "97F73FF3")]
    [InlineData("lipsum.txt", "8F827C30")]
    public async Task read_async_produces_expected_CRC32_hash_hex(string file, string expectedHashHex)
        => await AssertReadAsyncHashMatches(file, expectedHashHex, x => new HashSiphonStream(x, () => new Crc32Algorithm(), streamDirection: StreamDirection.Read));

    [Theory]
    [InlineData("helloworld.txt", "29b933a8d9a0fcef0af75f1713f4940e")]
    [InlineData("lipsum.txt", "c545355fdbfbfb5149dbe35bea3ff6e8")]
    public async Task read_async_produces_expected_MD5_hash_hex(string file, string expectedHashHex)
        => await AssertReadAsyncHashMatches(file, expectedHashHex, x => HashSiphonStream.CreateMD5Read(x));

    [Theory]
    [InlineData("helloworld.txt", "9fbc3fafddca353898269a2f4069e4653083bcdb")]
    [InlineData("lipsum.txt", "a3e4eba4936f15ba0b119cb1c487e889c400b156")]
    public async Task read_async_produces_expected_SHA1_hash_hex(string file, string expectedHashHex)
        => await AssertReadAsyncHashMatches(file, expectedHashHex, x => HashSiphonStream.CreateSHA1Read(x));

    [Theory]
    [InlineData("helloworld.txt", "92b772380a3f8e27a93e57e6deeca6c01da07f5aadce78bb2fbb20de10a66925")]
    [InlineData("lipsum.txt", "42afaa42c7b47e870b5b2ef41bb03511098700dc46911ad2e7c2d320861de059")]
    public async Task read_async_produces_expected_SHA256_hash_hex(string file, string expectedHashHex)
        => await AssertReadAsyncHashMatches(file, expectedHashHex, x => HashSiphonStream.CreateSHA256Read(x));

    [Theory]
    [InlineData("helloworld.txt", "97F73FF3")]
    [InlineData("lipsum.txt", "8F827C30")]
    public async Task write_async_produces_expected_CRC32_hash_hex(string file, string expectedHashHex)
        => await AssertWriteAsyncHashMatches(file, expectedHashHex, x => new HashSiphonStream(x, () => new Crc32Algorithm(), streamDirection: StreamDirection.Write));

    [Theory]
    [InlineData("helloworld.txt", "29b933a8d9a0fcef0af75f1713f4940e")]
    [InlineData("lipsum.txt", "c545355fdbfbfb5149dbe35bea3ff6e8")]
    public async Task write_async_produces_expected_MD5_hash_hex(string file, string expectedHashHex)
        => await AssertWriteAsyncHashMatches(file, expectedHashHex, x => HashSiphonStream.CreateMD5Write(x));

    [Theory]
    [InlineData("helloworld.txt", "9fbc3fafddca353898269a2f4069e4653083bcdb")]
    [InlineData("lipsum.txt", "a3e4eba4936f15ba0b119cb1c487e889c400b156")]
    public async Task write_async_produces_expected_SHA1_hash_hex(string file, string expectedHashHex)
        => await AssertWriteAsyncHashMatches(file, expectedHashHex, x => HashSiphonStream.CreateSHA1Write(x));

    [Theory]
    [InlineData("helloworld.txt", "92b772380a3f8e27a93e57e6deeca6c01da07f5aadce78bb2fbb20de10a66925")]
    [InlineData("lipsum.txt", "42afaa42c7b47e870b5b2ef41bb03511098700dc46911ad2e7c2d320861de059")]
    public async Task write_async_produces_expected_SHA256_hash_hex(string file, string expectedHashHex)
        => await AssertWriteAsyncHashMatches(file, expectedHashHex, x => HashSiphonStream.CreateSHA256Write(x));

    [Fact]
    public async Task GetHashBase64_produces_expected_encoded_hash()
    {
        // arrange
        using var stream = GetStreamFromString("banana");
        using var hashSiphonStream = HashSiphonStream.CreateMD5Read(stream);
        await ConsumeStreamAsync(hashSiphonStream);

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
        using var hashSiphonStream = HashSiphonStream.CreateMD5Read(stream);
        await ConsumeStreamAsync(hashSiphonStream);

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
        using var hashSiphonStream = HashSiphonStream.CreateMD5Read(stream);

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
        using var hashSiphonStream = HashSiphonStream.CreateMD5Read(stream);
        await ConsumeStreamAsync(hashSiphonStream);

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
        using var hashSiphonStream = HashSiphonStream.CreateMD5Read(stream);
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
        using var hashSiphonStream = HashSiphonStream.CreateMD5Read(stream);
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
        var hashSiphonStream = HashSiphonStream.CreateMD5Read(stream);

        // act
        await hashSiphonStream.DisposeAsync(); // should not throw

        // assert
        Assert.True(hashSiphonStream.IsDisposed);
    }

    [Fact]
    public void Constructor_throws_on_null_stream()
    {
        // arrange
        var hashAlgorithm = () => SHA256.Create();
        Stream stream = null!;

        // act & assert
        Assert.Throws<ArgumentNullException>(() => new HashSiphonStream(stream, hashAlgorithm, StreamDirection.Read));
    }

    [Fact]
    public void Constructor_throws_on_null_hash_algorithm()
    {
        // arrange
        using var stream = new MemoryStream();
        Func<HashAlgorithm> hashAlgorithm = null!;

        // act & assert
        Assert.Throws<ArgumentNullException>(() => new HashSiphonStream(stream, hashAlgorithm, StreamDirection.Read));
    }

    [Fact]
    public void Write_throws_NotSupportedException()
    {
        // arrange
        using var stream = new MemoryStream();
        using var hashSiphonStream = HashSiphonStream.CreateMD5Read(stream);

        // act & assert
        Assert.Throws<NotSupportedException>(() => hashSiphonStream.Write(Array.Empty<byte>(), 0, 0));
    }

    [Fact]
    public void Seek_throws_NotSupportedException()
    {
        // arrange
        using var stream = new MemoryStream();
        using var hashSiphonStream = HashSiphonStream.CreateMD5Read(stream);

        // act & assert
        Assert.Throws<NotSupportedException>(() => hashSiphonStream.Seek(0, SeekOrigin.Begin));
    }

    [Fact]
    public void SetLength_throws_NotSupportedException()
    {
        // arrange
        using var stream = new MemoryStream();
        using var hashSiphonStream = HashSiphonStream.CreateMD5Read(stream);

        // act & assert
        Assert.Throws<NotSupportedException>(() => hashSiphonStream.SetLength(100));
    }

    [Fact]
    public async Task Write_mode_finalizes_hash_on_DisposeAsync()
    {
        // arrange
        var buffer = Encoding.UTF8.GetBytes("banana");
        var stream = new MemoryStream();
        var hashStream = HashSiphonStream.CreateMD5Write(stream);

        // act
        await hashStream.WriteAsync(buffer, 0, buffer.Length);
        await hashStream.DisposeAsync();

        // assert
        Assert.True(hashStream.IsHashAvailable);
        Assert.Equal(BananaMd5Hex, hashStream.GetHashHex());
    }

    [Fact]
    public void Write_mode_finalizes_hash_on_Dispose()
    {
        // arrange
        var buffer = Encoding.UTF8.GetBytes("banana");
        var stream = new MemoryStream();
        var hashStream = HashSiphonStream.CreateMD5Write(stream);

        // act
        hashStream.Write(buffer, 0, buffer.Length);
        hashStream.Dispose();

        // assert
        Assert.True(hashStream.IsHashAvailable);
        Assert.Equal(BananaMd5Hex, hashStream.GetHashHex());
    }

    [Fact]
    public void TryGetHash_returns_false_before_finalization()
    {
        // arrange
        var stream = GetStreamFromString("banana");
        using var hashStream = HashSiphonStream.CreateMD5Read(stream);

        // act
        var gotBytes = hashStream.TryGetHash(out var hash);
        var gotHex = hashStream.TryGetHashHex(out var hex);
        var gotBase64 = hashStream.TryGetHashBase64(out var b64);

        // assert
        Assert.False(gotBytes);
        Assert.False(gotHex);
        Assert.False(gotBase64);
        Assert.Null(hash);
        Assert.Null(hex);
        Assert.Null(b64);
    }

    [Fact]
    public void Write_throws_ObjectDisposedException_after_dispose()
    {
        var stream = new MemoryStream();
        var hashStream = HashSiphonStream.CreateMD5Write(stream);
        hashStream.Dispose();

        Assert.Throws<ObjectDisposedException>(() => hashStream.Write(Array.Empty<byte>(), 0, 0));
    }

    [Fact]
    public async Task Write_throws_ObjectDisposedException_after_DisposeAsync()
    {
        var stream = new MemoryStream();
        var hashStream = HashSiphonStream.CreateMD5Write(stream);
        await hashStream.DisposeAsync();

        Assert.Throws<ObjectDisposedException>(() => hashStream.Write(Array.Empty<byte>(), 0, 0));
    }

    [Fact]
    public void Flush_throws_ObjectDisposedException_after_dispose()
    {
        var stream = new MemoryStream();
        var hashStream = HashSiphonStream.CreateMD5Read(stream);
        hashStream.Dispose();

        Assert.Throws<ObjectDisposedException>(() => hashStream.Flush());
    }

    [Fact]
    public async Task Flush_throws_ObjectDisposedException_after_DisposeAsync()
    {
        var stream = new MemoryStream();
        var hashStream = HashSiphonStream.CreateMD5Read(stream);
        await hashStream.DisposeAsync();

        Assert.Throws<ObjectDisposedException>(() => hashStream.Flush());
    }

    [Fact]
    public async Task ReadAsync_throws_in_write_mode()
    {
        var stream = new MemoryStream();
        using var hashStream = HashSiphonStream.CreateMD5Write(stream);

        var buffer = new byte[4];
        await Assert.ThrowsAsync<NotSupportedException>(() =>
            hashStream.ReadAsync(buffer, 0, buffer.Length));
    }

    [Fact]
    public void Read_throws_in_write_mode()
    {
        var stream = new MemoryStream();
        using var hashStream = HashSiphonStream.CreateMD5Write(stream);

        var buffer = new byte[4];
        Assert.Throws<NotSupportedException>(() =>
            hashStream.Read(buffer, 0, buffer.Length));
    }

    [Fact]
    public async Task WriteAsync_throws_in_read_mode()
    {
        var stream = new MemoryStream();
        using var hashStream = HashSiphonStream.CreateMD5Read(stream);

        var buffer = new byte[4];
        await Assert.ThrowsAsync<NotSupportedException>(() =>
            hashStream.WriteAsync(buffer, 0, buffer.Length));
    }

    [Fact]
    public void Write_throws_in_read_mode()
    {
        var stream = new MemoryStream();
        using var hashStream = HashSiphonStream.CreateMD5Read(stream);

        var buffer = new byte[4];
        Assert.Throws<NotSupportedException>(() =>
            hashStream.Write(buffer, 0, buffer.Length));
    }

    [Fact]
    public void Flush_does_not_finalize_hash_when_flag_is_false()
    {
        var stream = new MemoryStream();
        var hashStream = HashSiphonStream.CreateMD5Write(stream);

        var buffer = Encoding.UTF8.GetBytes("banana");
        hashStream.Write(buffer, 0, buffer.Length);
        hashStream.Flush(finalizeHash: false);

        Assert.False(hashStream.IsHashAvailable);
        Assert.Null(hashStream.Hash);
    }

    [Fact]
    public void Flush_finalizes_hash_when_flag_is_true()
    {
        var stream = new MemoryStream();
        var hashStream = HashSiphonStream.CreateMD5Write(stream);

        var buffer = Encoding.UTF8.GetBytes("banana");
        hashStream.Write(buffer, 0, buffer.Length);
        hashStream.Flush(finalizeHash: true);

        Assert.True(hashStream.IsHashAvailable);
        Assert.Equal(HashSiphonStreamTests.BananaMd5Hex, hashStream.GetHashHex());
    }

    [Fact]
    public async Task FlushAsync_does_not_finalize_hash_when_flag_is_false()
    {
        var stream = new MemoryStream();
        var hashStream = HashSiphonStream.CreateMD5Write(stream);

        var buffer = Encoding.UTF8.GetBytes("banana");
        await hashStream.WriteAsync(buffer, 0, buffer.Length);
        await hashStream.FlushAsync(finalizeHash: false);

        Assert.False(hashStream.IsHashAvailable);
        Assert.Null(hashStream.Hash);
    }

    [Fact]
    public async Task FlushAsync_finalizes_hash_when_flag_is_true()
    {
        var stream = new MemoryStream();
        var hashStream = HashSiphonStream.CreateMD5Write(stream);

        var buffer = Encoding.UTF8.GetBytes("banana");
        await hashStream.WriteAsync(buffer, 0, buffer.Length);
        await hashStream.FlushAsync(finalizeHash: true);

        Assert.True(hashStream.IsHashAvailable);
        Assert.Equal(HashSiphonStreamTests.BananaMd5Hex, hashStream.GetHashHex());
    }

    [Fact]
    public void Dispose_disposes_inner_stream_when_flag_is_false()
    {
        using var innerStream = new TrackingStream();

        using (var hashStream = HashSiphonStream.CreateMD5Read(innerStream, leaveInnerStreamOpen: false))
        {
            // do nothing
        }

        Assert.True(innerStream.IsDisposed);
    }

    [Fact]
    public void Dispose_does_not_dispose_inner_stream_when_flag_is_true()
    {
        using var innerStream = new TrackingStream();

        using (var hashStream = HashSiphonStream.CreateMD5Read(innerStream, leaveInnerStreamOpen: true))
        {
            // do nothing
        }

        Assert.False(innerStream.IsDisposed);
    }

    [Fact]
    public async Task DisposeAsync_disposes_inner_stream_when_flag_is_false()
    {
        using var innerStream = new TrackingStream();

        var hashStream = HashSiphonStream.CreateMD5Read(innerStream, leaveInnerStreamOpen: false);

        await hashStream.DisposeAsync();

        Assert.True(innerStream.IsDisposed);
    }

    [Fact]
    public async Task DisposeAsync_does_not_dispose_inner_stream_when_flag_is_true()
    {
        using var innerStream = new TrackingStream();

        var hashStream = HashSiphonStream.CreateMD5Read(innerStream, leaveInnerStreamOpen: true);

        await hashStream.DisposeAsync();

        Assert.False(innerStream.IsDisposed);
    }

    [Fact]
    public void CanRead_returns_true_only_in_read_mode()
    {
        using var stream = new MemoryStream();
        using var readStream = HashSiphonStream.CreateMD5Read(stream);
        using var writeStream = HashSiphonStream.CreateMD5Write(stream);

        Assert.True(readStream.CanRead);
        Assert.False(writeStream.CanRead);
    }

    [Fact]
    public void CanWrite_returns_true_only_in_write_mode()
    {
        using var stream = new MemoryStream();
        using var readStream = HashSiphonStream.CreateMD5Read(stream);
        using var writeStream = HashSiphonStream.CreateMD5Write(stream);

        Assert.False(readStream.CanWrite);
        Assert.True(writeStream.CanWrite);
    }

    [Fact]
    public void CanSeek_is_always_false()
    {
        using var stream = new MemoryStream();
        using var readStream = HashSiphonStream.CreateMD5Read(stream);
        using var writeStream = HashSiphonStream.CreateMD5Write(stream);

        Assert.False(readStream.CanSeek);
        Assert.False(writeStream.CanSeek);
    }

    [Fact]
    public void Position_returns_inner_stream_position()
    {
        using var innerStream = new MemoryStream(new byte[100]);
        innerStream.Position = 42;

        using var hashStream = HashSiphonStream.CreateMD5Read(innerStream);

        Assert.Equal(42, hashStream.Position);
    }

    [Fact]
    public void Length_returns_inner_stream_length()
    {
        using var innerStream = new MemoryStream(new byte[100]);

        using var hashStream = HashSiphonStream.CreateMD5Read(innerStream);

        Assert.Equal(100, hashStream.Length);
    }

    [Fact]
    public void TryGetHash_returns_true_after_dispose_without_finalization()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("banana"));
        var hashStream = HashSiphonStream.CreateMD5Write(stream);
        hashStream.Dispose(); // No explicit Flush(true)

        Assert.True(hashStream.TryGetHash(out _));
        Assert.True(hashStream.TryGetHashHex(out _));
        Assert.True(hashStream.TryGetHashBase64(out _));
    }

    [Fact]
    public async Task FlushAsync_calls_inner_flush_async()
    {
        var inner = new TrackingStream();
        var hashStream = HashSiphonStream.CreateMD5Read(inner);

        await hashStream.FlushAsync();

        Assert.True(inner.FlushAsyncCalled);
    }

    [Fact]
    public void BytesRead_increments_with_read_operations()
    {
        // Arrange
        byte[] data = Encoding.UTF8.GetBytes("abcdef");
        using var innerStream = new MemoryStream(data);
        using var hashStream = HashSiphonStream.CreateMD5Read(innerStream);

        byte[] buffer = new byte[3];

        // Act
        int firstRead = hashStream.Read(buffer, 0, buffer.Length);  // Should read 3
        int secondRead = hashStream.Read(buffer, 0, buffer.Length); // Should read 3
        int thirdRead = hashStream.Read(buffer, 0, buffer.Length);  // Should read 0 (EOF)

        // Assert
        Assert.Equal(3, firstRead);
        Assert.Equal(3, secondRead);
        Assert.Equal(0, thirdRead);
        Assert.Equal(6, hashStream.BytesRead);
    }

    [Fact]
    public void BytesWritten_increments_with_write_operations()
    {
        // Arrange
        using var innerStream = new MemoryStream();
        using var hashStream = HashSiphonStream.CreateMD5Write(innerStream);

        byte[] buffer1 = Encoding.UTF8.GetBytes("abc");
        byte[] buffer2 = Encoding.UTF8.GetBytes("defg");

        // Act
        hashStream.Write(buffer1, 0, buffer1.Length);
        hashStream.Write(buffer2, 0, buffer2.Length);

        // Assert
        Assert.Equal(3 + 4, hashStream.BytesWritten);
    }

    [Fact]
    public async Task BytesRead_increments_with_async_reads()
    {
        byte[] data = Encoding.UTF8.GetBytes("123456");
        using var innerStream = new MemoryStream(data);
        using var hashStream = HashSiphonStream.CreateMD5Read(innerStream);

        byte[] buffer = new byte[2];
        while (await hashStream.ReadAsync(buffer, 0, buffer.Length) > 0)
        {
        }

        Assert.Equal(6, hashStream.BytesRead);
    }

    [Fact]
    public async Task BytesWritten_increments_with_async_writes()
    {
        using var innerStream = new MemoryStream();
        using var hashStream = HashSiphonStream.CreateMD5Write(innerStream);

        byte[] buffer = Encoding.UTF8.GetBytes("testing");
        await hashStream.WriteAsync(buffer, 0, buffer.Length);

        Assert.Equal(buffer.Length, hashStream.BytesWritten);
    }

    private static MemoryStream GetStreamFromString(string content) =>
        new MemoryStream(Encoding.UTF8.GetBytes(content));

    private static async Task ConsumeStreamAsync(Stream stream)
    {
        byte[] buffer = new byte[8192];
        while (await stream.ReadAsync(buffer, 0, buffer.Length) > 0)
        {
        }
    }

    private static void ConsumeStream(Stream stream)
    {
        byte[] buffer = new byte[8192];
        while (stream.Read(buffer, 0, buffer.Length) > 0)
        {
        }
    }

    private void AssertReadHashMatches(string file, string expectedHashHex, Func<Stream, HashSiphonStream> hashSiphonStreamFactory)
    {
        // arrange
        using var stream = GetEmbeddedResourceStream(file);
        using var hashSiphonStream = hashSiphonStreamFactory(stream);

        // act
        ConsumeStream(hashSiphonStream);

        // assert
        Assert.Equal(expectedHashHex, hashSiphonStream.GetHashHex(), ignoreCase: true);
    }

    private void AssertWriteHashMatches(string file, string expectedHashHex, Func<Stream, HashSiphonStream> hashSiphonStreamFactory)
    {
        // arrange
        using var sourceStream = GetEmbeddedResourceStream(file);
        using var targetStream = new MemoryStream();
        using var hashSiphonStream = hashSiphonStreamFactory(targetStream);

        // act
        sourceStream.CopyTo(hashSiphonStream);
        hashSiphonStream.Flush(finalizeHash: true);

        // assert
        Assert.Equal(expectedHashHex, hashSiphonStream.GetHashHex(), ignoreCase: true);
    }

    private async Task AssertReadAsyncHashMatches(string file, string expectedHashHex, Func<Stream, HashSiphonStream> hashSiphonStreamFactory)
    {
        // arrange
        using var stream = GetEmbeddedResourceStream(file);
        using var hashSiphonStream = hashSiphonStreamFactory(stream);

        // act
        await ConsumeStreamAsync(hashSiphonStream);

        // assert
        Assert.Equal(expectedHashHex, hashSiphonStream.GetHashHex(), ignoreCase: true);
    }

    private async Task AssertWriteAsyncHashMatches(string file, string expectedHashHex, Func<Stream, HashSiphonStream> hashSiphonStreamFactory)
    {
        // arrange
        using var sourceStream = GetEmbeddedResourceStream(file);
        using var targetStream = new MemoryStream();
        using var hashSiphonStream = hashSiphonStreamFactory(targetStream);

        // act
        await sourceStream.CopyToAsync(hashSiphonStream);
        await hashSiphonStream.FlushAsync(finalizeHash: true);

        // assert
        Assert.Equal(expectedHashHex, hashSiphonStream.GetHashHex(), ignoreCase: true);
    }

    private Stream GetEmbeddedResourceStream(string file)
        => this.GetType().Assembly.GetManifestResourceStream(
            this.GetType().Assembly.GetManifestResourceNames().Single(x => x.EndsWith(file, StringComparison.OrdinalIgnoreCase)))!;

    private class TrackingStream : MemoryStream
    {
        public bool IsDisposed { get; private set; }

        public bool FlushAsyncCalled { get; private set; }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            FlushAsyncCalled = true;
            return base.FlushAsync(cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
