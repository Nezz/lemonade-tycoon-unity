using Newtonsoft.Json;

namespace Yousician.Expo.Messages.Inbound
{
	/// <summary>
	/// Received from the native app when the active camera view changes.
	/// </summary>
	public sealed class CameraViewChanged : InboundMessage
	{
		[JsonProperty("view")]
		public CameraView View { get; set; }
	}
}
