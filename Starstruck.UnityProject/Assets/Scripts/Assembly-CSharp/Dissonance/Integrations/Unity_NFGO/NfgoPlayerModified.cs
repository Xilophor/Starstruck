using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Dissonance.Integrations.Unity_NFGO
{
	[RequireComponent(typeof(NetworkObject))]
	public class NfgoPlayerModified : NetworkBehaviour, IDissonancePlayer
	{
		private static readonly Log Log;

		private DissonanceComms _comms;

		private Transform _transform;

		private string _playerIdString;

		private readonly NetworkVariable<FixedString128Bytes> _playerId;

		private bool hasStartedTracking;

		[NotNull]
		private Transform Transform => null;

		public Vector3 Position => default(Vector3);

		public Quaternion Rotation => default(Quaternion);

		public bool IsTracking
		{
			[CompilerGenerated]
			get
			{
				return false;
			}
			[CompilerGenerated]
			private set
			{
			}
		}

		public string PlayerId => null;

		public NetworkPlayerType Type => default(NetworkPlayerType);

		public override void OnDestroy()
		{
		}

		public void VoiceChatTrackingStart()
		{
		}

		[ServerRpc]
		public void SetNameServerRpc(string playerName)
		{
		}

		private void OnLocalPlayerIdChanged(string _)
		{
		}

		private void OnNetworkVariablePlayerIdChanged<T>(T previousvalue, T newvalue)
		{
		}

		private void StartTracking()
		{
		}

		private void StopTracking()
		{
		}
	}
}
