using System;
using UnityEngine;

namespace Yousician.Expo
{
	/// <summary>
	/// Static message channel for communicating with the native mobile app.
	/// Subscribe to <see cref="MessageReceived"/> to handle inbound messages.
	/// Call <see cref="Send"/> to send outbound messages.
	/// </summary>
	public static class NativeMessageChannel
	{
		/// <summary>
		/// Raised when a message is received from the native mobile app.
		/// </summary>
		public static event Action<string> MessageReceived;

		/// <summary>
		/// Sends a string message to the native mobile app.
		/// </summary>
		public static void Send(string message)
		{
			try
			{
				NativeMessageSender.SendMessageToMobileApp(message);
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to send native message: {e}");
			}
		}

		/// <summary>
		/// Called internally when an inbound message arrives from native.
		/// </summary>
		internal static void OnMessageReceived(string message)
		{
			MessageReceived?.Invoke(message);
		}

#if UNITY_EDITOR
		/// <summary>
		/// Triggers a fake inbound message for editor testing.
		/// </summary>
		public static void TriggerFakeMessage(string message)
		{
			OnMessageReceived(message);
		}
#endif
	}
}
