Octodiff is a 100% managed implementation of remote delta compression. Usage is inspired by [rdiff](http://librsync.sourcefrog.net/doc/rdiff.html), and like rdiff the [algorithm is based on rsync](http://rsync.samba.org/tech_report/tech_report.html). Octodiff was designed to be used in [Octopus Deploy](http://octopusdeploy.com), an automated deployment tool for .NET developers.

Octodiff can make deltas of files by comparing a remote file with a local file, without needing both files to exist on the same machine. It does this in three phases:

 1. Machine A reads the *basis file*, and computes a *signature* of all the chunks in the file
 2. Machine B uses the *signature* and the *new file*, to produce a *delta file*, specifying what changes need to be made
 3. Machine A applies the *delta file* to the *basis file*, which produces an exact copy of the *new file*

Of course, the benefit is that instead of transferring the entire *new file* to machine A, we just transfer a small signature, and a delta file containing the differences. We're trading off CPU usage for potentially large bandwidth savings. 

Octodiff is an executable, but can also be referenced and used as any .NET assembly. 

Usage:

    octodiff signature <basis-file> [options]


## Signatures

As per the rsync algorithm, the signature is calculated by breaking the file into fixed-size blocks, and then calculating a signature of each block. The resulting signature file contains:

 - Metadata about the signature file and algorithms used
 - Hash of the file that the signature was created from
 - A list of chunk signatures, each 26 bytes long, consisting of:
   - The length of the chunk (short)
   - Rolling checksum (uint)
   - Hash (20 bytes)

Given that the default chunk size is 2048 bytes, and this is turned into a 26 byte signature, the resulting file is about 1.3% of the size of the original. For example, a 306MB file creates a 3.9MB signature file. 

The signature of a 300mb file can be calculated in ~3 seconds using ~6mb of memory on a 2013 Macbook Pro.

## Deltas





Exit codes: 

0 - success
1 - environmental problems
2 - corrupt signature or delta file
3 - internal error or unhandled situation
4 - usage problem
