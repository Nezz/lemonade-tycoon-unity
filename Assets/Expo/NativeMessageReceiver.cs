using UnityEngine;
using UnityEngine.Scripting;

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
			NativeMessageChannel.Send("initialized");
		}

		[Preserve]
		public void Receive(string message)
		{
			NativeMessageChannel.OnMessageReceived(message);
		}
	}
}
