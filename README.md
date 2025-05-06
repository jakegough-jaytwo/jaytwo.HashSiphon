# jaytwo.HashSiphon

[![NuGet Version](https://img.shields.io/nuget/v/jaytwo.HashSiphon.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/jaytwo.HashSiphon)
[![NuGet Downloads](https://img.shields.io/nuget/dt/jaytwo.HashSiphon.svg?style=flat)](https://www.nuget.org/packages/jaytwo.HashSiphon)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

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

Read mode

```csharp
using var input = File.OpenRead("myfile.txt");
using var hashStream = HashSiphonStream.CreateSHA256Read(input);

using var output = File.Create("copy.txt");
await hashStream.CopyToAsync(output);

// Final hash is now available
Console.WriteLine(hashStream.GetHashHex());
```

Write mode
```csharp
using var output = File.Create("output.txt");
using var hashStream = HashSiphonStream.CreateSHA256Write(output);

var buffer = Encoding.UTF8.GetBytes("hello world");
await hashStream.WriteAsync(buffer, 0, buffer.Length);

// finalize the hash without disposing
await hashStream.FlushAsync(finalizeHash: true);

Console.WriteLine(hashStream.GetHashHex());
```

Factory Methods

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


If you want to use a custom HashAlgorithm:

```csharp
var sha512Stream = new HashSiphonStream(myStream, () => SHA512.Create(), StreamDirection.Read);
```

All constructors and overloads accept `leaveInnerStreamOpen = true` if you don't want the wrapped stream disposed when `HashSiphonStream` is disposed.


## Notes

* In read mode: `Hash` (and `GetHashHex()` / `GetHashBase64()`) will return null or empty until the stream is fully read.
* In write mode: `Hash` (and `GetHashHex()` / `GetHashBase64()`) will return null or empty until the stream is eitehr disposed or flushed with `finalizeHash = true`.
* You can use any of the `TryGetHash()` variants if you want check for the hash availability and retreive the hash in a single line (though honestly, I don't understand the use case of not knowing whether the hash is ready yet).
* Supports `DisposeAsync()` and cancellation tokens on .NET 5+.
* `Seek` and `SetLength` are not supported.

---

Made with &hearts; by Jake
