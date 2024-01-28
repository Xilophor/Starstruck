using System;
using Dissonance.Networking;
using JetBrains.Annotations;
using Unity.Netcode;

namespace Dissonance.Integrations.Unity_NFGO
{
	public class NfgoClient : BaseClient<NfgoServer, NfgoClient, NfgoConn>
	{
		private readonly NfgoCommsNetwork _network;

		private NetworkManager _networkManager;

		private byte[] _receiveBuffer;

		public NfgoClient([NotNull] NfgoCommsNetwork network) : base(network)
		{
		}

		public override void Connect()
		{
		}

		public override void Disconnect()
		{
		}

		private void NamedMessageHandler(ulong sender, FastBufferReader stream)
		{
		}

		protected override void ReadMessages()
		{
		}

		protected override void SendReliable(ArraySegment<byte> packet)
		{
		}

		protected override void SendUnreliable(ArraySegment<byte> packet)
		{
		}
	}
}
