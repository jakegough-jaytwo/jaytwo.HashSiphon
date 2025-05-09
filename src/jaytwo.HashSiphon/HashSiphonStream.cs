using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace jaytwo.HashSiphon;

public class HashSiphonStream : Stream
{
    private readonly Stream _innerStream;
    private readonly HashAlgorithm _hashAlgorithm;
    private readonly bool _leaveOpen;
    private readonly StreamDirection _streamDirection;
    private bool _finalized = false;
    private byte[]? _finalHash;
    private long _bytesWritten = 0;
    private long _bytesRead = 0;

    public HashSiphonStream(Stream innerStream, Func<HashAlgorithm> hashAlgorithmFactory, StreamDirection streamDirection, bool leaveOpen = false)
    {
        _innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
        _hashAlgorithm = hashAlgorithmFactory?.Invoke() ?? throw new ArgumentNullException(nameof(hashAlgorithmFactory));
        _leaveOpen = leaveOpen;
        _streamDirection = streamDirection;
    }

    public bool IsHashAvailable => _finalized;

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

    public long BytesWritten => _bytesWritten;

    public long BytesRead => _bytesRead;

    public override bool CanRead => _streamDirection == StreamDirection.Read && _innerStream.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => _streamDirection == StreamDirection.Write && _innerStream.CanWrite;

    public override long Length => _innerStream.Length;

    public override long Position
    {
        get => _innerStream.Position;
        set => throw new NotSupportedException($"Set {nameof(Position)} is not supported.");
    }

    internal bool IsDisposed { get; private set; } = false;

    public static HashSiphonStream CreateSHA256Read(Stream innerStream, bool leaveInnerStreamOpen = false)
        => CreateRead(innerStream, () => SHA256.Create(), leaveInnerStreamOpen);

    public static HashSiphonStream CreateSHA1Read(Stream innerStream, bool leaveInnerStreamOpen = false)
        => CreateRead(innerStream, () => SHA1.Create(), leaveInnerStreamOpen);

    public static HashSiphonStream CreateMD5Read(Stream innerStream, bool leaveInnerStreamOpen = false)
        => CreateRead(innerStream, () => MD5.Create(), leaveInnerStreamOpen);

    public static HashSiphonStream CreateSHA256Write(Stream innerStream, bool leaveInnerStreamOpen = false)
        => CreateWrite(innerStream, () => SHA256.Create(), leaveInnerStreamOpen);

    public static HashSiphonStream CreateSHA1Write(Stream innerStream, bool leaveInnerStreamOpen = false)
        => CreateWrite(innerStream, () => SHA1.Create(), leaveInnerStreamOpen);

    public static HashSiphonStream CreateMD5Write(Stream innerStream, bool leaveInnerStreamOpen = false)
        => CreateWrite(innerStream, () => MD5.Create(), leaveInnerStreamOpen);

    public static HashSiphonStream CreateRead(Stream innerStream, Func<HashAlgorithm> hashAlgorithm, bool leaveInnerStreamOpen = false)
        => new(innerStream, hashAlgorithm, StreamDirection.Read, leaveInnerStreamOpen);

    public static HashSiphonStream CreateWrite(Stream innerStream, Func<HashAlgorithm> hashAlgorithm, bool leaveInnerStreamOpen = false)
        => new(innerStream, hashAlgorithm, StreamDirection.Write, leaveInnerStreamOpen);

    public bool TryGetHash(out byte[]? hash)
    {
        hash = IsHashAvailable ? _finalHash : null;
        return IsHashAvailable;
    }

    public bool TryGetHashHex(out string? hash)
    {
        hash = IsHashAvailable ? GetHashHex() : null;
        return IsHashAvailable;
    }

    public bool TryGetHashBase64(out string? hash)
    {
        hash = IsHashAvailable ? GetHashBase64() : null;
        return IsHashAvailable;
    }

    public string GetHashHex() =>
        IsHashAvailable ? BitConverter.ToString(_finalHash!).Replace("-", string.Empty).ToLowerInvariant() : string.Empty;

    public string GetHashBase64() =>
        IsHashAvailable ? Convert.ToBase64String(_finalHash!) : string.Empty;

    public void Flush(bool finalizeHash)
    {
        Flush();

        if (finalizeHash && !_finalized)
        {
            FinalizeHash();
        }
    }

    public override void Flush()
    {
        ThrowIfDisposed();
        _innerStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_streamDirection != StreamDirection.Read)
        {
            throw new NotSupportedException($"{nameof(Read)} is not supported.");
        }

        ThrowIfDisposed();

        int readBytes = _innerStream.Read(buffer, offset, count);

        if (readBytes > 0)
        {
            _hashAlgorithm.TransformBlock(buffer, offset, readBytes, null, 0);
            _bytesRead += readBytes;
        }
        else if (!_finalized)
        {
            FinalizeHash();
        }

        return readBytes;
    }

    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException($"{nameof(Seek)} is not supported.");

    public override void SetLength(long value)
        => throw new NotSupportedException($"{nameof(SetLength)} is not supported.");

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (_streamDirection != StreamDirection.Write)
        {
            throw new NotSupportedException($"{nameof(Write)} is not supported.");
        }

        ThrowIfDisposed();

        _innerStream.Write(buffer, offset, count);
        _hashAlgorithm.TransformBlock(buffer, offset, count, null, 0);
        _bytesWritten += count;
    }

    public override void Close()
    {
        base.Close();

        if (!_leaveOpen)
        {
            _innerStream.Close();
        }
    }

#if NET5_0_OR_GREATER

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (_streamDirection != StreamDirection.Read)
        {
            throw new NotSupportedException($"{nameof(ReadAsync)} is not supported.");
        }

        ThrowIfDisposed();

        int readBytes = await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);

        if (readBytes > 0)
        {
            _hashAlgorithm.TransformBlock(buffer, offset, readBytes, null, 0);
            _bytesRead += readBytes;
        }
        else if (!_finalized)
        {
            FinalizeHash();
        }

        return readBytes;
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (_streamDirection != StreamDirection.Write)
        {
            throw new NotSupportedException($"{nameof(Write)} is not supported.");
        }

        ThrowIfDisposed();

        await _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        _hashAlgorithm.TransformBlock(buffer, offset, count, null, 0);
        _bytesWritten += count;
    }

    public async Task FlushAsync(bool finalizeHash, CancellationToken cancellationToken = default)
    {
        await FlushAsync();

        if (finalizeHash && !_finalized)
        {
            FinalizeHash();
        }
    }

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        await _innerStream.FlushAsync(cancellationToken);
    }

    public override async ValueTask DisposeAsync()
    {
        if (!_finalized && _streamDirection == StreamDirection.Write)
        {
            FinalizeHash();
        }

        // Dispose synchronous resources before awaiting
        _hashAlgorithm.Dispose();

        try
        {
            if (!_leaveOpen)
            {
                await _innerStream.DisposeAsync().ConfigureAwait(false);
            }
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
            if (!_finalized && _streamDirection == StreamDirection.Write)
            {
                FinalizeHash();
            }

            _hashAlgorithm.Dispose();

            if (!_leaveOpen)
            {
                _innerStream.Dispose();
            }
        }

        base.Dispose(disposing);

        IsDisposed = true;
    }

    private void FinalizeHash()
    {
        if (_finalized)
        {
            return;
        }

        _hashAlgorithm.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        _finalHash = _hashAlgorithm.Hash;
        _finalized = true;
    }

    private void ThrowIfDisposed()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(HashSiphonStream));
        }
    }
}
