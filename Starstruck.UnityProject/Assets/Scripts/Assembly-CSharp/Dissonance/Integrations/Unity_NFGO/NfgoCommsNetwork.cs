using System;
using System.Collections.Generic;
using Dissonance.Datastructures;
using Dissonance.Networking;
using JetBrains.Annotations;
using Unity.Netcode;

namespace Dissonance.Integrations.Unity_NFGO
{
	public class NfgoCommsNetwork : BaseCommsNetwork<NfgoServer, NfgoClient, NfgoConn, Unit, Unit>
	{
		private readonly ConcurrentPool<byte[]> _loopbackBuffers;

		private readonly List<ArraySegment<byte>> _loopbackQueueToServer;

		private readonly List<ArraySegment<byte>> _loopbackQueueToClient;

		protected override NfgoClient CreateClient(Unit connectionParameters)
		{
			return null;
		}

		protected override NfgoServer CreateServer(Unit connectionParameters)
		{
			return null;
		}

		protected override void Update()
		{
		}

		internal void SendToServer(ArraySegment<byte> packet, bool reliable, [NotNull] NetworkManager netManager)
		{
		}

		internal void SendToClient(ArraySegment<byte> packet, NfgoConn client, bool reliable, [NotNull] NetworkManager netManager)
		{
		}

		private static FastBufferWriter WritePacket(ArraySegment<byte> packet)
		{
			return default(FastBufferWriter);
		}

		internal static ArraySegment<byte> ReadPacket(ref FastBufferReader reader, [CanBeNull] ref byte[] buffer)
		{
			return default(ArraySegment<byte>);
		}

		public NfgoCommsNetwork()
		{
		}
	}
}
