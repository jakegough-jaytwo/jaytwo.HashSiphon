using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace jaytwo.HashSiphon;

public class HashSiphonStream : Stream
{
    private readonly Stream _innerStream;
    private readonly HashAlgorithm _hashAlgorithm;
    private bool _finalized = false;
    private byte[]? _finalHash;

    public HashSiphonStream(Stream innerStream, HashAlgorithm hashAlgorithm)
    {
        _innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
        _hashAlgorithm = hashAlgorithm ?? throw new ArgumentNullException(nameof(hashAlgorithm));
    }

    public byte[]? Hash
    {
        get
        {
            if (!_finalized)
            {
                return null;
            }

            return _finalHash;
        }
    }

    public override bool CanRead => _innerStream.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => _innerStream.Length;

    public override long Position
    {
        get => _innerStream.Position;
        set => throw new NotSupportedException($"Set {nameof(Position)} is not supported.");
    }

    internal bool IsDisposed { get; private set; } = false;

    public static HashSiphonStream CreateSHA256(Stream innerStream)
        => new HashSiphonStream(innerStream, SHA256.Create());

    public static HashSiphonStream CreateSHA1(Stream innerStream)
        => new HashSiphonStream(innerStream, SHA1.Create());

    public static HashSiphonStream CreateMD5(Stream innerStream)
        => new HashSiphonStream(innerStream, MD5.Create());

    public string GetHashHex() =>
        _finalized ? BitConverter.ToString(_finalHash!).Replace("-", string.Empty).ToLowerInvariant() : string.Empty;

    public string GetHashBase64() =>
        _finalized ? Convert.ToBase64String(_finalHash!) : string.Empty;

    public override void Flush() => _innerStream.Flush();

    public override int Read(byte[] buffer, int offset, int count)
    {
        int readBytes = _innerStream.Read(buffer, offset, count);

        if (readBytes > 0)
        {
            _hashAlgorithm.TransformBlock(buffer, offset, readBytes, null, 0);
        }
        else if (!_finalized)
        {
            // Finalize the hash when we reach the end of the stream
            _hashAlgorithm.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            _finalHash = _hashAlgorithm.Hash;
            _finalized = true;
        }

        return readBytes;
    }

    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException($"{nameof(Seek)} is not supported.");

    public override void SetLength(long value)
        => throw new NotSupportedException($"{nameof(SetLength)} is not supported.");

    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException($"{nameof(Write)} is not supported.");

    public override void Close()
    {
        base.Close();
        _innerStream.Close();
    }

#if NET5_0_OR_GREATER
    public override async ValueTask DisposeAsync()
    {
        // Dispose synchronous resources before awaiting
        _hashAlgorithm.Dispose();

        try
        {
            await _innerStream.DisposeAsync().ConfigureAwait(false);
        }
        finally
        {
            // Always call base DisposeAsync
            await base.DisposeAsync().ConfigureAwait(false);
        }

        IsDisposed = true;
    }
#endif

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hashAlgorithm.Dispose();
            _innerStream.Dispose();
        }

        base.Dispose(disposing);

        IsDisposed = true;
    }
}
