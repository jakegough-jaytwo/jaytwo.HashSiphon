using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jaytwo.HashSiphon.Tests;

public class TrackingStream : MemoryStream
{
    public bool IsDisposed { get; private set; }

    public override async ValueTask DisposeAsync()
    {
        IsDisposed = true;
        await base.DisposeAsync();
    }

    protected override void Dispose(bool disposing)
    {
        IsDisposed = true;
        base.Dispose(disposing);
    }
}
