using UnityEngine;
using UnityEngine.Scripting;
using Yousician.Expo.Messages.Outbound;

namespace Yousician.Expo
{
	/// <summary>
	/// MonoBehaviour that receives messages from the native mobile app via Unity's SendMessage.
	/// Must live on a GameObject whose name matches the native-side SendMessage target.
	/// </summary>
	internal sealed class NativeMessageReceiver : MonoBehaviour
	{
		private void Start()
		{
			NativeMessageChannel.Send(new Initialized());
		}

		/// <summary>
		/// Called by the native app via UnitySendMessage with a JSON string payload.
		/// </summary>
		[Preserve]
		public void Receive(string json)
		{
			NativeMessageChannel.OnMessageReceived(json);
		}
	}
}
