using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yousician.Expo.Messages.Inbound
{
	/// <summary>
	/// Received from the native app when the player's active upgrades change.
	/// </summary>
	public sealed class UpgradesChanged : InboundMessage
	{
		[JsonProperty("upgrades")]
		public List<UpgradeId> Upgrades { get; set; } = new List<UpgradeId>();
	}
}
