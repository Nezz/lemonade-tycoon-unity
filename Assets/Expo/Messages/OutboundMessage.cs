using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Yousician.Expo.Json;
using Yousician.Expo.Messages.Outbound;

namespace Yousician.Expo.Messages
{
	/// <summary>
	/// Base class for all messages sent to the native mobile app.
	/// Concrete subtypes are resolved via the <c>messageType</c> discriminator field.
	/// </summary>
	[JsonConverter(typeof(DiscriminatorJsonConverter<OutboundMessage, MessageType>), "messageType")]
	public abstract class OutboundMessage
	{
		[JsonConverter(typeof(StringEnumConverter))]
		private enum MessageType
		{
			[EnumMember(Value = "Initialized")]
			[JsonTypeDiscriminator(typeof(Initialized))]
			Initialized,

			// Add outbound message types here as the protocol grows.
		}
	}
}
