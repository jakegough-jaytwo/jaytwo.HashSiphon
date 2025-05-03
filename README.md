# jaytwo.HashSiphon

[![NuGet Version](https://img.shields.io/nuget/v/jaytwo.HashSiphon.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/jaytwo.HashSiphon)
[![NuGet Downloads](https://img.shields.io/nuget/dt/jaytwo.HashSiphon.svg?style=flat)](https://www.nuget.org/packages/jaytwo.HashSiphon)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Once or twice, I've accepted user uploads to an API which I then stream to an object store. Sometimes I want a hash of that file—whether for deduplication, verification, or future audit. Usually, however, the file isn't supplied with a hash up front.

This package provides a way to hash the content **as it's being read** from a stream.

Since we don't have the full hash until the stream is consumed, this doesn't allow us to set the `Content-MD5` header (for example) before sending to the object store. It **does**, however, allow us to persist the final hash to a database or log it for tracking, all without reading the stream twice.

## Installation

Add the NuGet package:

```powershell
PM> Install-Package jaytwo.HashSiphon
```

## Usage

Wrap the input stream with a HashSiphonStream, then read it as usual. The hash becomes available after the stream is fully consumed.

```csharp
using var input = File.OpenRead("myfile.txt");
using var hashStream = HashSiphonStream.CreateSHA256(input);

using var output = File.Create("copy.txt");

// Stream the file somewhere (e.g. to disk or an object store)
await hashStream.CopyToAsync(output);

// Now get the hash!
Console.WriteLine("SHA-256 (hex):   " + hashStream.HashHex);
Console.WriteLine("SHA-256 (base64): " + hashStream.HashBase64);
```

You can also create it with MD5 or SHA1:

```csharp
var md5Stream = HashSiphonStream.CreateMD5(myStream);
var sha1Stream = HashSiphonStream.CreateSHA1(myStream);
```

If you want to use a custom HashAlgorithm:

```csharp
var sha512Stream = new HashSiphonStream(myStream, SHA512.Create());
```

## Caveats

`Hash` (and `GetHashHex()` / `GetHashBase64()`) will return null or empty until the stream is fully read.

This is a read-only stream wrapper — `Write()` and `Seek()` are not supported.

---

Made with &hearts; by Jake
