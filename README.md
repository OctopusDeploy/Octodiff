Octodiff is a 100% managed implementation of remote delta compression. Usage is inspired by [rdiff](https://librsync.github.io/rdiff.html), and like rdiff the [algorithm is based on rsync](http://rsync.samba.org/tech_report/tech_report.html). Octodiff was designed to be used in [Octopus Deploy](http://octopusdeploy.com), an automated deployment tool for .NET developers.

Octodiff can make deltas of files by comparing a remote file with a local file, without needing both files to exist on the same machine. It does this in three phases:

 1. Machine A reads the *basis file*, and computes a *signature* of all the chunks in the file
 2. Machine B uses the *signature* and the *new file*, to produce a *delta file*, specifying what changes need to be made
 3. Machine A applies the *delta file* to the *basis file*, which produces an exact copy of the *new file*

Of course, the benefit is that instead of transferring the entire *new file* to machine A, we just transfer a small signature, and a delta file containing the differences. We're trading off CPU usage for potentially large bandwidth savings.

Octodiff is an executable, but can also be referenced and used as any .NET assembly.

## Signatures

```
Usage: Octodiff signature <basis-file> [<signature-file>] [<options>]

Arguments:

      basis-file             The file to read and create a signature from.
      signature-file         The file to write the signature to.

Options:

      --chunk-size=VALUE     Maximum bytes per chunk. Defaults to 2048. Min of
                             128, max of 31744.
      --progress             Whether progress should be written to stdout
```

Example:

```
octodiff signature MyApp.1.0.nupkg MyApp.1.0.nupkg.octosig --progress
```

This command calculates the signature of a given file. As per the rsync algorithm, the signature is calculated by reading the file into fixed-size chunks, and then calculating a signature of each chunk. The resulting signature file contains:

 - Metadata about the signature file and algorithms used
 - Hash of the file that the signature was created from (*basis file hash*)
 - A list of chunk signatures, each 26 bytes long, consisting of:
   - The length of the chunk (short)
   - Rolling checksum (uint) - calculated using Adler32
   - Hash (20 bytes) - calculated using SHA1

Given that the default chunk size is 2048 bytes, and this is turned into a 26 byte signature, the resulting file is about 1.3% of the size of the original. For example, a 306MB file creates a 3.9MB signature file. The signature of a 300mb file can be calculated in ~3 seconds using ~6mb of memory on a 2013 Macbook Pro. Memory usage during signature calculation should remain constant no matter the size of the file.

## Deltas

```
Usage: Octodiff delta <signature-file> <new-file> [<delta-file>] [<options>]

Arguments:

      signature-file         The file containing the signature from the basis
                             file.
      new-file               The file to create the delta from.
      delta-file             The file to write the delta to.

Options:

      --progress             Whether progress should be written to stdout
```

Example:

```
octodiff delta MyApp.1.0.nupkg.octosig MyApp.1.1.nupkg MyApp.1.0_to_1.1.octodelta --progress
```

This command creates a delta, that specfies how the *basis-file* (using just the information in its *signature file*) can be turned into the *new-file*. First, the signature file is read into memory. Then we scan the new file, looking for chunks that we find in the signature. You can learn more about the process in [the rsync algorithm](http://rsync.samba.org/tech_report/node4.html).

The delta file contains:

 - Metadata about the signature file and algorithms used
 - Hash of the file that the original signature was created from (*basis file hash*)
 - A series of instructions to re-create the *new-file* which reference the *basis file*.

Instructions are either copy commands (read offset X, length Y from the *basis file*) or data commands (add this data). Example:

1. Copy 0x0000 to 0x8C00
2. Data: 5C 9F D9 C7...
3. Copy 0x8C31 to 0x93C0

The delta file uses a binary file format to keep encoding overhead to a minimum - copy instructions start with 0x60 and then the start offset and length; data commands are 0x80 followed by the length of the data and then the data to copy.

For debugging, you can use the following command to print an explanation of what is in a given delta file:

```
octodiff explain-delta MyApp.1.0_to_1.1.octodelta
```

## Patching

```
Usage: Octodiff patch <basis-file> <delta-file> <new-file> [<options>]

Arguments:

      basis-file             The file that the delta was created for.
      delta-file             The delta to apply to the basis file
      new-file               The file to write the result to.

Options:

      --progress             Whether progress should be written to stdout
      --skip-verification    Skip checking whether the basis file is the same
                             as the file used to produce the signature that
                             created the delta.
```

Example:

```
octodiff patch MyApp.1.0.nupkg MyApp.1.0_to_1.1.octodelta MyApp.1.1.nupkg --progress
```

This command recreates the *new-file* using simply the *basis-file* and the *delta-file*.

Applying the delta is the easiest part of the process. We simply open the *delta-file*, and follow the instructions. When there's a copy instruction, we seek to that offset in the *basis-file* and copy until we hit the length. When we encounter a data instruction, we append that data. At the end of the process, we have the *new-file*.

Octodiff embeds a SHA1 hash of the *new-file* in the *delta-file*. After patching, Octodiff compares this hash to the SHA1 hash of the resulting patched file. If they don't match, Octodiff returns a non-zero exit code.

## Performance

**The following section isn't meant to be mathematically accurate, but to give you a rough idea of real-world performance to expect from Octodiff. The tests were done on a Windows 8 VM, running in a 2013 Macbook Pro, with 4 cores and 8GB of memory assigned to the VM. The machine uses an SSD which mean the I/O bound tasks could run significantly slower on non-SSD drives. All measurements were done using simply Windows Task Manager.**

Signature creation is relatively easy - we're reading the file in fixed-size chunks and computing a checksum. Memory usage should be constant no matter how big the file is - around 8.2 MB.

 - The signature for an 85 MB file can be calculated in ~832 ms
 - The signature for a 4.4 GB file can be calculated in ~36 seconds

We also compute a SHA1 hash of the entire basis file (this takes around 1/3 of the total time above). The resulting signature file size is always ~1.3% of the basis file size.

Delta creation is the most CPU and memory-intensive aspect of the whole process. First, we assume that we can fit all signatures into memory, which means at a minimum we'll consume at least ~1.3% of the *basis file* in memory, plus extra to store a dictionary of the chunks and buffers as we read data. Budget for about 5x the signature file size in memory (e.g., for a 57 MB signature file (a 4.4 GB basis file), expect to use 250mb of memory).

 - Delta from a 85 MB file took 5 seconds
 - Delta from a 4.3 GB ISO took 170 seconds

Delta creation takes roughly the same amount of time whether there are many differences or none at all. If there are many differences, the resulting delta file will be much larger, so additional I/O producing it may have an impact.

Patching is the fastest part of the algorithm.

## Output and exit codes

If all goes well, Octodiff produces no output. You can use the `--progress` switch to write progress messages to stdout.

Octodiff uses the following exit codes:

 - `0` - success
 - `1` - environmental problems
 - `2` - corrupt signature or delta file
 - `3` - internal error or unhandled situation
 - `4` - usage problem (you did something wrong, maybe passing the wrong file)

## Using OctoDiff classes within your own application

To use the OctoDiff classes to create signature/delta/final files from within your own application, you can use the below example which creates the signature and delta file and then applies the delta file to create the new file.

```csharp
// Create signature file
var signatureBaseFilePath = @"C:\OctoDiffExample\MyPackage.1.0.0.zip";
var signatureFilePath = @"C:\OctoDiffExample\Output\MyPackage.1.0.0.zip.octosig";
var signatureOutputDirectory = Path.GetDirectoryName(signatureFilePath);
if(!Directory.Exists(signatureOutputDirectory))
	Directory.CreateDirectory(signatureOutputDirectory);
var signatureBuilder = new SignatureBuilder();
using (var basisStream = new FileStream(signatureBaseFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
using (var signatureStream = new FileStream(signatureFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
{
	signatureBuilder.Build(basisStream, new SignatureWriter(signatureStream));
}

// Create delta file
var newFilePath = @"C:\OctoDiffExample\MyPackage.1.0.1.zip";
var deltaFilePath = @"C:\OctoDiffExample\Output\MyPackage.1.0.1.zip.octodelta";
var deltaOutputDirectory = Path.GetDirectoryName(deltaFilePath);
if(!Directory.Exists(deltaOutputDirectory))
	Directory.CreateDirectory(deltaOutputDirectory);
var deltaBuilder = new DeltaBuilder();
using(var newFileStream = new FileStream(newFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
using(var signatureFileStream = new FileStream(signatureFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
using(var deltaStream = new FileStream(deltaFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
{
	deltaBuilder.BuildDelta(newFileStream, new SignatureReader(signatureFileStream, new ConsoleProgressReporter()), new AggregateCopyOperationsDecorator(new BinaryDeltaWriter(deltaStream)));
}

// Apply delta file to create new file
var newFilePath2 = @"C:\OctoDiffExample\Output\MyPackage.1.0.1.zip";
var newFileOutputDirectory = Path.GetDirectoryName(newFilePath2);
if(!Directory.Exists(newFileOutputDirectory))
	Directory.CreateDirectory(newFileOutputDirectory);
var deltaApplier = new DeltaApplier { SkipHashCheck = false };
using(var basisStream = new FileStream(signatureBaseFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
using(var deltaStream = new FileStream(deltaFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
using(var newFileStream = new FileStream(newFilePath2, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
{
	deltaApplier.Apply(basisStream, new BinaryDeltaReader(deltaStream, new ConsoleProgressReporter()), newFileStream);
}
```

## Development
You need:
- VSCode or Visual Studio 15.3 to compile the solution
- .NET Core 2.0 SDK (https://download.microsoft.com/download/0/F/D/0FD852A4-7EA1-4E2A-983A-0484AC19B92C/dotnet-sdk-2.0.0-win-x64.exe)

Run `Build.cmd` to build, test and package the project.

To release to Nuget, tag `master` with the next major, minor or patch number, [TeamCity](https://build.octopushq.com/viewType.html?buildTypeId=OctopusDeploy_LIbraries_Octodiff) will do the rest.

Every successful TeamCity build for all branches will be pushed to MyGet.
