﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Octodiff.Core
{
	public class SignatureWriter : ISignatureWriter
	{
		private readonly BinaryWriter signatureStream;

		public SignatureWriter (Stream signatureStream)
		{
			this.signatureStream = new BinaryWriter (signatureStream);
		}

		public void WriteMetadata (IHashAlgorithm hashAlgorithm, IRollingChecksum rollingChecksumAlgorithm, byte[] hash)
		{
			signatureStream.Write (BinaryFormat.SignatureHeader);
			signatureStream.Write (BinaryFormat.Version);
			signatureStream.Write (hashAlgorithm.Name);
			signatureStream.Write (rollingChecksumAlgorithm.Name);
			signatureStream.Write (BinaryFormat.EndOfMetadata);
		}

		public void WriteChunk (ChunkSignature signature)
		{
			signatureStream.Write (signature.Length);
			signatureStream.Write (signature.RollingChecksum);
			signatureStream.Write (signature.Hash);
		}
	}
}
