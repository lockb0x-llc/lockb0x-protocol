using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Lockb0x.Storage.Internal;

/// <summary>
/// Stream wrapper that computes an incremental hash as data is read.
/// </summary>
internal sealed class HashingStream : Stream
{
    private readonly Stream _inner;
    private readonly IncrementalHash _hash;
    private readonly bool _leaveOpen;
    private bool _disposed;

    public HashingStream(Stream inner, bool leaveOpen = false, HashAlgorithmName? algorithm = null)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        if (!inner.CanRead)
        {
            throw new ArgumentException("The supplied stream must be readable.", nameof(inner));
        }

        _leaveOpen = leaveOpen;
        _hash = IncrementalHash.CreateHash(algorithm ?? HashAlgorithmName.SHA256);
    }

    public long BytesProcessed { get; private set; }

    public override bool CanRead => !_disposed && _inner.CanRead;
    public override bool CanSeek => !_disposed && _inner.CanSeek;
    public override bool CanWrite => false;
    public override long Length => _inner.Length;

    public override long Position
    {
        get => _inner.Position;
        set => _inner.Position = value;
    }

    public byte[] GetHashAndReset()
    {
        EnsureNotDisposed();
        return _hash.GetHashAndReset();
    }

    public override void Flush() => _inner.Flush();

    public override int Read(byte[] buffer, int offset, int count)
    {
        var read = _inner.Read(buffer, offset, count);
        if (read > 0)
        {
            _hash.AppendData(buffer, offset, read);
            BytesProcessed += read;
        }
        return read;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var read = await _inner.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
        if (read > 0)
        {
            _hash.AppendData(buffer, offset, read);
            BytesProcessed += read;
        }
        return read;
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        => ReadAsyncCore(buffer, cancellationToken);

    private async ValueTask<int> ReadAsyncCore(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var read = await _inner.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        if (read > 0)
        {
            _hash.AppendData(buffer.Span[..read]);
            BytesProcessed += read;
        }
        return read;
    }

    public override int ReadByte()
    {
        var value = _inner.ReadByte();
        if (value >= 0)
        {
            Span<byte> buffer = stackalloc byte[1];
            buffer[0] = (byte)value;
            _hash.AppendData(buffer);
            BytesProcessed++;
        }
        return value;
    }

    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _hash.Dispose();
            if (!_leaveOpen)
            {
                _inner.Dispose();
            }
        }

        _disposed = true;
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _hash.Dispose();
        if (!_leaveOpen)
        {
            await _inner.DisposeAsync().ConfigureAwait(false);
        }

        _disposed = true;
        await base.DisposeAsync().ConfigureAwait(false);
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(HashingStream));
        }
    }
}
