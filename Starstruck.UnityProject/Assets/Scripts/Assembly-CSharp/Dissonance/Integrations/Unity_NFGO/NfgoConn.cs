using System;

namespace Dissonance.Integrations.Unity_NFGO
{
	public readonly struct NfgoConn : IEquatable<NfgoConn>
	{
		public readonly ulong ClientId;

		public bool Equals(NfgoConn other)
		{
			return false;
		}

		public override bool Equals(object obj)
		{
			return false;
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public static bool operator ==(NfgoConn left, NfgoConn right)
		{
			return false;
		}

		public static bool operator !=(NfgoConn left, NfgoConn right)
		{
			return false;
		}
	}
}
