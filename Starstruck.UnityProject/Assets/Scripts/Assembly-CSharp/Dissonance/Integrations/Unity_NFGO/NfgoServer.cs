using System;
using Dissonance.Networking;
using Unity.Netcode;

namespace Dissonance.Integrations.Unity_NFGO
{
	public class NfgoServer : BaseServer<NfgoServer, NfgoClient, NfgoConn>
	{
		private readonly NfgoCommsNetwork _network;

		private byte[] _receiveBuffer;

		private NetworkManager _networkManager;

		public NfgoServer(NfgoCommsNetwork network)
		{
		}

		public override void Connect()
		{
		}

		public override void Disconnect()
		{
		}

		private void Disconnected(ulong client)
		{
		}

		private void NamedMessageHandler(ulong sender, FastBufferReader stream)
		{
		}

		protected override void ReadMessages()
		{
		}

		protected override void SendReliable(NfgoConn destination, ArraySegment<byte> packet)
		{
		}

		protected override void SendUnreliable(NfgoConn destination, ArraySegment<byte> packet)
		{
		}
	}
}
