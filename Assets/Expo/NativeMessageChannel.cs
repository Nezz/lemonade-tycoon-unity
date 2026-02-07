using System;
using Newtonsoft.Json;
using UnityEngine;
using Yousician.Expo.Messages;

namespace Yousician.Expo
{
	/// <summary>
	/// Static message channel for communicating with the native mobile app.
	/// Subscribe to <see cref="MessageReceived"/> to handle inbound messages.
	/// Call <see cref="Send"/> to send outbound messages.
	/// </summary>
	public static class NativeMessageChannel
	{
		private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore,
		};

		/// <summary>
		/// Raised when a typed message is received from the native mobile app.
		/// </summary>
		public static event Action<InboundMessage> MessageReceived;

		/// <summary>
		/// Sends a typed message to the native mobile app, serialized as JSON.
		/// </summary>
		public static void Send(OutboundMessage message)
		{
			try
			{
				var json = JsonConvert.SerializeObject(message, SerializerSettings);
				NativeMessageSender.SendMessageToMobileApp(json);
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to send native message: {e}");
			}
		}

		/// <summary>
		/// Called internally when a raw JSON string arrives from native.
		/// Deserializes it into an <see cref="InboundMessage"/> and raises the event.
		/// </summary>
		internal static void OnMessageReceived(string json)
		{
			try
			{
				var message = JsonConvert.DeserializeObject<InboundMessage>(json, SerializerSettings);
				MessageReceived?.Invoke(message);
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to deserialize inbound native message: {e}\nRaw JSON: {json}");
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// Triggers a fake inbound message for editor testing.
		/// </summary>
		public static void TriggerFakeMessage(InboundMessage message)
		{
			MessageReceived?.Invoke(message);
		}

		/// <summary>
		/// Triggers a fake inbound message from a raw JSON string for editor testing.
		/// </summary>
		public static void TriggerFakeMessage(string json)
		{
			OnMessageReceived(json);
		}
#endif
	}
}
