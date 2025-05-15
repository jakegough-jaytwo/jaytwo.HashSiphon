# jaytwo.HashSiphon

[![NuGet Version](https://img.shields.io/nuget/v/jaytwo.HashSiphon.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/jaytwo.HashSiphon)
[![NuGet Downloads](https://img.shields.io/nuget/dt/jaytwo.HashSiphon.svg?style=flat)](https://www.nuget.org/packages/jaytwo.HashSiphon)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://mit-license.org/)

jaytwo.HashSiphon is a .NET stream wrapper that computes a hash (SHA256, SHA1, MD5, or custom) while reading from or writing to a stream — without needing to buffer or read the stream twice.

[View source on GitHub](https://github.com/jakegough-jaytwo/jaytwo.HashSiphon)

## Features

- Hash streams in real time (read or write)
- SHA256, SHA1, MD5, or custom algorithm support
- Works with Stream APIs like `CopyToAsync`, `WriteAsync`, etc.
- Async and cancellation token support
- Does not buffer or re-read the stream
- Hash available after stream is fully consumed

## Background

Once or twice, I've accepted user uploads to an API which I then stream to an object store. Sometimes I want a hash of that file—whether for deduplication, verification, or future audit. Usually, however, the file isn't supplied with a hash up front.

This package provides a way to hash the content **as it's being read from or written to** the underlying stream.

Since we don't have the full hash until the stream is consumed (referring to the use case above), this doesn't allow us to set the `Content-MD5` header (for example) before sending to the object store. It **does**, however, allow us to persist the final hash to a database or log it for tracking, all without reading the stream twice.

## Installation

Add the NuGet package:

```powershell
PM> Install-Package jaytwo.HashSiphon
```

## Usage

Wrap the input stream with a HashSiphonStream, then read it as usual. The hash becomes available after the stream is fully consumed.

### Examples

```csharp
// Example: Hashing While Reading

using var input = File.OpenRead("myfile.txt");
using var hashStream = HashSiphonStream.CreateSHA256Read(input);

using var output = File.Create("copy.txt");
await hashStream.CopyToAsync(output);

// Final hash is now available
Console.WriteLine(hashStream.HashHex);
```

```csharp
// Example: Hashing While Writing

using var output = File.Create("output.txt");
using var hashStream = HashSiphonStream.CreateSHA256Write(output);

var buffer = Encoding.UTF8.GetBytes("hello world");
await hashStream.WriteAsync(buffer, 0, buffer.Length);

// finalize the hash without disposing
await hashStream.FlushAsync(finalizeHash: true);

Console.WriteLine(hashStream.HashHex);
```

### Factory Methods

| Method                | Description                           |
| --------------------- | ------------------------------------- |
| `CreateSHA256Read()`  | Creates a SHA256 stream in read mode  |
| `CreateSHA1Read()`    | Creates a SHA1 stream in read mode    |
| `CreateMD5Read()`     | Creates a MD5 stream in read mode     |
| `CreateSHA256Write()` | Creates a SHA256 stream in write mode |
| `CreateSHA1Write()`   | Creates a SHA1 stream in write mode   |
| `CreateMD5Write()`    | Creates a MD5 stream in write mode    |
| `CreateRead()`        | Custom algorithm, read mode           |
| `CreateWrite()`       | Custom algorithm, write mode          |


### Custom Algorithms

If you want to use a custom HashAlgorithm:

```csharp
// I used this approach to write multiple generated XML documents into a ZIP package (which requires a CRC32 checksum for each entry) when streaming an XLSX file to a write-only output.

using (var hashSiphonStream = HashSiphonStream.CreateWrite(_outputStream, () => new Crc32Algorithm(isBigEndian: false), leaveOpen: true))
{
    await WriteXml(hashSiphon);
    await hashSiphonStream.FlushAsync(finalizeHash: true);
    crc32 = BitConverter.ToUInt32(hashSiphon.Hash);
}
```

All constructor overloads accept `leaveOpen = true` if you don't want the wrapped stream disposed when `HashSiphonStream` is disposed.


## Notes

* Hash computation is finalized automatically when a read stream hits EOF or when a write stream is flushed with `finalizeHash = true` or disposed.
* `Hash` (and `HashHex` / `HashBase64`) will return null until the hash computation is finalized.
* You can use any of the `TryGetHash()` variants to check for hash availability and retrieve the value in a single line. These are mostly useful for scenarios where you're uncertain whether the hash has been finalized yet (e.g., mid-stream inspection).
* Supports `DisposeAsync()` on .NET 5+.
* `Seek` and `SetLength` are not supported.

---

Made with &hearts; by Jake — Licensed under the [MIT License](https://mit-license.org/)
