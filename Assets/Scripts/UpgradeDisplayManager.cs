using System;
using System.Collections.Generic;
using UnityEngine;
using Yousician.Expo;
using Yousician.Expo.Messages;
using Yousician.Expo.Messages.Inbound;

namespace Yousician
{
	/// <summary>
	/// Toggles GameObjects on/off based on the active upgrades received from the native app.
	/// Wire up the upgrade-to-GameObject mappings in the Inspector.
	/// </summary>
	public sealed class UpgradeDisplayManager : MonoBehaviour
	{
		[Serializable]
		public struct UpgradeEntry
		{
			[Tooltip("The upgrade identifier from the native app.")]
			public UpgradeId upgradeId;

			[Tooltip("The GameObject to enable when this upgrade is active.")]
			public GameObject target;
		}

		[SerializeField]
		private UpgradeEntry[] upgrades = Array.Empty<UpgradeEntry>();

		private void OnEnable()
		{
			NativeMessageChannel.MessageReceived += OnMessageReceived;
		}

		private void OnDisable()
		{
			NativeMessageChannel.MessageReceived -= OnMessageReceived;
		}

		private void OnMessageReceived(InboundMessage message)
		{
			if (message is UpgradesChanged changed)
			{
				ApplyUpgrades(changed.Upgrades);
			}
		}

		private void ApplyUpgrades(List<UpgradeId> activeUpgrades)
		{
			foreach (var entry in upgrades)
			{
				if (entry.target == null) continue;
				bool isActive = activeUpgrades.Contains(entry.upgradeId);
				entry.target.SetActive(isActive);
			}
		}
	}
}
