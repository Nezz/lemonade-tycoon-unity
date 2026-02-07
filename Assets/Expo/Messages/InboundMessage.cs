using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Yousician.Expo.Json;
using Yousician.Expo.Messages.Inbound;

namespace Yousician.Expo.Messages
{
	/// <summary>
	/// Base class for all messages received from the native mobile app.
	/// Concrete subtypes are resolved via the <c>messageType</c> discriminator field.
	/// </summary>
	[JsonConverter(typeof(DiscriminatorJsonConverter<InboundMessage, InboundMessage.MessageType>), "messageType")]
	public abstract class InboundMessage
	{
		[JsonConverter(typeof(StringEnumConverter))]
		private enum MessageType
		{
			[EnumMember(Value = "UpgradesChanged")]
			[JsonTypeDiscriminator(typeof(UpgradesChanged))]
			UpgradesChanged,

			[EnumMember(Value = "CameraViewChanged")]
			[JsonTypeDiscriminator(typeof(CameraViewChanged))]
			CameraViewChanged,
		}
	}
}
